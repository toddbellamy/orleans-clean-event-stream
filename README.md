
Orleans Clean Event Stream is derived from Jon McGuire's excellent example from his blog post:

### [Event Sourcing with Orleans Journaled Grains](https://mcguirev10.com/2019/12/05/event-sourcing-with-orleans-journaled-grains.html)

Some differences are:
Domain logic pushed to the Domain Aggregates.
Encapsulation for Domain Entities.
Event Sourcing code is generic with some additional flexibility.
Easier to add Entities without modification of infrastructure.

