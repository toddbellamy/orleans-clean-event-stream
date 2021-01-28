using DomainBase;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcingInfrastructure
{
    public class AggregateJournal<TAggregate>
        : EventSourcedGrain<TAggregate, DomainEventBase>
        where TAggregate : AggregateRoot, new()
    {
        private static string ConnectionString = @"Server=MSILAPTOP\MSSQLSERVER01;Integrated Security=true;Database=OrleansCES";

        private readonly ILogger<AggregateJournal<TAggregate>> Log;

        private Type aggregateType;

        public AggregateJournal(ILogger<AggregateJournal<TAggregate>> log)
        {
            Log = log;
            aggregateType = typeof(TAggregate);
        }

        private Guid GrainPrimaryKey
        {
            get => ((Grain)this).GetPrimaryKey();
        }

        public async Task<KeyValuePair<int, TAggregate>> ReadStateFromStorage()
        {
            Log.LogInformation("ReadStateFromStorage: start");
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            var (etag, state) = await ReadSnapshot(connection);
            Log.LogInformation($"ReadStateFromStorage: ReadSnapshot loaded etag {etag}");
            var newETag = await ApplyNewerEvents(connection, etag, state);
            if (newETag != etag) await WriteNewSnapshot(connection, newETag, state);
            etag = newETag;
            await connection.CloseAsync();
            Log.LogInformation($"ReadStateFromStorage: returning etag {etag}");
            return new KeyValuePair<int, TAggregate>(etag, state);
        }


        public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<DomainEventBase> updates, int expectedversion)
        {
            Log.LogInformation($"ApplyUpdatesToStorage: start, expected etag {expectedversion}, update count {updates.Count}");
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            Log.LogInformation("ApplyUpdatesToStorage: checking persisted stream version");
            int ver = await GetEventStreamVersion(connection);
            Log.LogInformation($"ApplyUpdatesToStorage: persisted version {ver} is expected? {(ver == expectedversion)}");
            if (ver != expectedversion) return false;

            if (ver == 0)
            {
                Log.LogInformation("ApplyUpdatesToStorage: etag 0 special-case write Initialized event");
                await WriteEvent(connection, new InitializedEvent
                {
                    ETag = 0,
                    Id = GrainPrimaryKey
                });

                Log.LogInformation("ApplyUpdatesToStorage: etag 0 special-case write snapshot");
                await WriteNewSnapshot(connection, 0, State);
            }

            foreach (var e in updates)
            {
                ver++;
                Log.LogInformation($"ApplyUpdatesToStorage: update ver {ver} event {e.GetType().Name} has etag {e.ETag}");
                if (e.ETag == DomainEventBase.NEW_ETAG)
                {
                    e.ETag = ver;
                    await WriteEvent(connection, e);
                }
            }
            await connection.CloseAsync();
            Log.LogInformation("ApplyUpdatesToStorage: exit");
            return true;
        }

        private async Task<(int etag, TAggregate state)> ReadSnapshot(SqlConnection connection)
        {
            Log.LogInformation("ReadSnapshot: start");
            int etag = 0;
            var state = new TAggregate();
            state.Id = GrainPrimaryKey;

            using var cmd = new SqlCommand($"SELECT ETag, Snapshot FROM {aggregateType.Name}Snapshot WHERE Id=@Id;");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", GrainPrimaryKey);

            using var reader = await cmd.ExecuteReaderAsync();
            if (reader.HasRows)
            {
                Log.LogInformation("ReadSnapshot: found snapshot to load");
                await reader.ReadAsync();
                etag = reader.GetInt32(0);
                var snapshot = reader.GetString(1);
                state = JsonConvert.DeserializeObject<TAggregate>(snapshot, JsonSettings);
            }
            await reader.CloseAsync();

            Log.LogInformation($"ReadSnapshot: exit returning etag {etag}");
            return (etag, state);
        }


        private async Task<int> ApplyNewerEvents(SqlConnection connection, int snapshotETag, TAggregate state)
        {
            Log.LogInformation($"ApplyNewerEvents: start for etags newer than {snapshotETag}");
            using var cmd = new SqlCommand($"SELECT ETag, Payload FROM {aggregateType.Name}EventStream WHERE Id = @Id AND ETag > @etag ORDER BY ETag ASC;");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", GrainPrimaryKey);
            cmd.Parameters.AddWithValue("@etag", snapshotETag);

            using var reader = await cmd.ExecuteReaderAsync();
            int etag = snapshotETag;
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    etag = reader.GetInt32(0);
                    var payload = reader.GetString(1);
                    var eventbase = JsonConvert.DeserializeObject(payload, JsonSettings) as DomainEventBase;
                    Log.LogInformation($"ApplyNewerEvents: applying event {eventbase.GetType()} for etag {etag}");
                    MethodInfo apply = typeof(TAggregate).GetMethod("Apply", new Type[] { eventbase.GetType() });
                    apply.Invoke(state, new object[] { eventbase });
                }
            }
            await reader.CloseAsync();

            Log.LogInformation($"ApplyNewerEvents: exit returning etag {etag}");
            return etag;
        }

        private async Task WriteNewSnapshot(SqlConnection connection, int etag, TAggregate state)
        {
            Log.LogInformation($"WriteNewSnapshot: start write for etag {etag}");
            var snapshot = JsonConvert.SerializeObject(state, JsonSettings);
            using var cmd = new SqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = (etag == 0)
                ? $"INSERT INTO {aggregateType.Name}Snapshot (Id, ETag, Snapshot) VALUES (@Id, @etag, @snapshot);"
                : $"UPDATE {aggregateType.Name}Snapshot SET ETag = @etag, Snapshot = @snapshot WHERE Id = @Id;";
            cmd.Parameters.AddWithValue("@Id", GrainPrimaryKey);
            cmd.Parameters.AddWithValue("@etag", etag);
            cmd.Parameters.AddWithValue("@snapshot", snapshot);
            await cmd.ExecuteNonQueryAsync();
            Log.LogInformation("WriteNewSnapshot: exit");
        }

        private async Task<int> GetEventStreamVersion(SqlConnection connection)
        {
            // The MAX aggregate returns NULL for no rows, allowing ISNULL to substitute the 0 value, otherwise
            // ExecuteScalarAsync would return null for an empty recordset
            using var cmd = new SqlCommand($"SELECT TOP 1 ISNULL(MAX(ETag), 0) AS ETag FROM {aggregateType.Name}EventStream WHERE Id = @Id ORDER BY ETag DESC;");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", GrainPrimaryKey);
            int etag = (int)await cmd.ExecuteScalarAsync();
            return etag;
        }


        private async Task WriteEvent(SqlConnection connection, DomainEventBase e)
        {
            Log.LogInformation($"WriteEvent: start for {e.GetType().Name}");
            var payload = JsonConvert.SerializeObject(e, JsonSettings);
            using var cmd = new SqlCommand($"INSERT INTO {aggregateType.Name}EventStream (Id, ETag, Timestamp, EventType, Payload) VALUES (@Id, @etag, @timestamp, @typeName, @payload);");
            cmd.Connection = connection;
            cmd.Parameters.AddWithValue("@Id", GrainPrimaryKey);
            cmd.Parameters.AddWithValue("@etag", e.ETag);
            cmd.Parameters.AddWithValue("@timestamp", e.Timestamp.ToString("o"));
            cmd.Parameters.AddWithValue("@typeName", e.GetType().Name);
            cmd.Parameters.AddWithValue("@payload", payload);
            await cmd.ExecuteNonQueryAsync();
            Log.LogInformation("WriteEvent: exit");
        }

        private JsonSerializerSettings JsonSettings { get; }
            = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new EntitySerializationContractResolver()
            };
    }
}
