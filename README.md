# Unofficial.Npgsql.DiagnosticSource

unofficial implementation for bridging Npgsql.Logging to DiagnosticSource

# Usage

1. add [nuget package](https://www.nuget.org/packages/Unofficial.Npgsql.DiagnosticSource/) to your project.
2. subscribe events via `System.Diagnostics.DiagnosticListener.AllListeners`
3. set `Unofficial.Npgsql.Logging.DiagnosticNpgsqlLoggingProvider` to `Npgsql.Logging.NpgsqlLogManager.Provider`.

[sample program is here](https://github.com/itn3000/Unofficial.Npgsql.DiagnosticSource/tree/master/example)

# Diagnostic events

This library provides [DiagnosticSource](https://github.com/dotnet/runtime/blob/e05718a2f810481cec6d2768aead9ba4580e3ddb/src/libraries/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md) named "Npgsql.Logging".
"Npgsql.Logging" produces following events.

|event name|when|argument type|
|----------|----|-------------|
|Log.*|emitted log from Npgsql.Logging.NpgsqlLogger|Unofficial.Npgsql.Logging.LogEventArgs|
|CreateLogger|first time of creating NpgsqlLogger instance|Unofficial.Npgsql.Logging.CreateLoggerEventArgs|

## LogEventArgs

`LogEventArgs` is the class which has following members.

|name|type|
|----|----|
|Level|NpgsqlLogLevel|
|Message|string|
|ConnectorId|int|
|Exception|Exception|

## CreateLoggerEventArgs

`CreateEventLoggerArgs is the class which has following members.

|name|type|
|----|----|
|Name|string|
|Level|NpgsqlLogLevel|
