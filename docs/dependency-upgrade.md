# Dependency upgrade (October 2026)

All direct NuGet dependencies and the local GitVersion tool use the latest stable
versions available on October 22, 2026. Dependencies already current, including
DotNet.ReproducibleBuilds, retain their versions. The AppVeyor logger has been removed.
GitHub Actions references use verified release commit hashes. CI and the local
Docker helper both pin the stable RabbitMQ 4.4.0 management image.
The shared integration-test queue is durable because RabbitMQ 4.4 disables
non-durable, non-exclusive queues by default.

## Compatibility

- Building requires .NET SDK 10.0.400 or later in the .NET 10 feature bands.
  `global.json` rolls forward to the latest installed feature band. This supplies
  a compiler capable of loading Microsoft.CodeAnalysis 5.9 source generators.
  The libraries still target .NET 8, 9, and 10.
- RabbitMQ.Client 7 replaces `IModel` with `IChannel`. Consequently,
  `RabbitMQConnector.GetChannel()` returns `IChannel`; callers using an explicit
  `IModel` declaration must update it. The connector, source, and sink retain
  synchronous pipeline operations and wait for the client's async operations.
  Channel caching and disposal are synchronized; shutdown callbacks no longer
  attempt to reconnect from the client's event dispatcher. A closed channel is
  replaced when the pipeline next requests it.
- The template generators now implement `IIncrementalGenerator`. Their inputs
  are embedded templates, so source is emitted during post-initialization.
- NUnit 5, the current test SDK/adapter, JsonPath.Net 3, and
  MethodBoundaryAspect.Fody 3 are exercised by the existing regression suites.
  Scriban 7.5.1 replaces the vulnerable 6.2.1 build dependency.

RabbitMQ's upstream API changes are documented in its
[version 7 migration guide](https://github.com/rabbitmq/rabbitmq-dotnet-client/blob/main/v7-MIGRATION.md).
