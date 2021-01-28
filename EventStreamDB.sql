USE [OrleansCES]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- No clustered PK, all reads are by specific CustomerId ordered by ETag (TODO: why?)
CREATE TABLE [dbo].[CustomerEventStream] (
    [Id] UNIQUEIDENTIFIER  NOT NULL, 
	[ETag]       INT           NOT NULL,
    [Timestamp]  CHAR (33)     NOT NULL, 
    [EventType]  VARCHAR (MAX) NOT NULL,
    [Payload]    VARCHAR (MAX) NOT NULL
);
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_CustomerEventStream ON
[dbo].[CustomerEventStream] ([Id] ASC, [ETag] ASC);
GO

-- No clustered PK, all reads are for a specific CustomerId (TODO: why?)
CREATE TABLE [dbo].[CustomerSnapshot] (
    [Id] UNIQUEIDENTIFIER  NOT NULL,
	[ETag]       INT           NOT NULL,
    [Snapshot]   VARCHAR (MAX) NOT NULL
);
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_CustomerSnapshot ON
[dbo].[CustomerSnapshot] ([Id] ASC);
GO
