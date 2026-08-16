# ApricotFramework.DataOps

[![NuGet](https://img.shields.io/nuget/v/ApricotFramework.DataOps.svg?label=ApricotFramework.DataOps)](https://www.nuget.org/packages/ApricotFramework.DataOps/)
[![NuGet](https://img.shields.io/nuget/v/ApricotFramework.DataOps.Abstractions.svg?label=ApricotFramework.DataOps.Abstractions)](https://www.nuget.org/packages/ApricotFramework.DataOps.Abstractions/)
[![NuGet](https://img.shields.io/nuget/v/ApricotFramework.DataOps.AspNetCore.svg?label=ApricotFramework.DataOps.AspNetCore)](https://www.nuget.org/packages/ApricotFramework.DataOps.AspNetCore/)
[![CI](https://github.com/project-apricot/data-ops/actions/workflows/ci.yml/badge.svg)](https://github.com/project-apricot/data-ops/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue.svg)](https://github.com/project-apricot/data-ops/blob/main/LICENSE)

SQL lives in XML files with a schema, addressed by group and name, and executed with Dapper — so a
query can be reviewed, reused across services, and written per database engine without the calling
code knowing which engine it reached.

`ApricotFramework.DataOps.Abstractions` is the **zero-dependency** contract.

## Install

```bash
dotnet add package ApricotFramework.DataOps.AspNetCore
dotnet add package ApricotFramework.DataOps.Abstractions   # for a library that only calls IDataOperations
```

## Usage

```xml
<!-- Operations/Authors.xml, embedded or deployed as a file -->
<DataOperations>
  <DataConfiguration Compatibility="Sqlite" Timeout="PT30S" />
  <OperationGroup Name="Authors">
    <SqlOperation Name="All">
      <TextCommand ExpectedResult="Table">
        SELECT id, full_name, born_in FROM authors ORDER BY {if:byName{ full_name } else { id }}
      </TextCommand>
    </SqlOperation>
  </OperationGroup>
</DataOperations>
```

```csharp
builder.Services.AddDataOperations(builder.Configuration);
builder.Services.AddDataSourceConnection(SqlProvider.Sqlite, cs => new SqliteConnection(cs));
builder.Services.AddOperationsDefinitionSource<AuthorOperationsSource>();
```

```csharp
// One operation: connect, address it, run it.
var authors = await dataOps.Connect().Query("Authors", "All")
    .WithBinding("byName", true)
    .ExecuteAsync<Author>();

// Several operations on one connection, in one transaction.
await using var scope = await dataOps.BeginAsync(AutoTransaction.Serializable);
await scope.Router.NonQuery("Authors", "Create").ExecuteAsync(author);
await scope.Router.NonQuery("Audit", "Append").ExecuteAsync(entry);
await scope.CommitAsync();      // disposing without this rolls back
```

Which engine an operation is written for is declared, not inferred: an operation compatible with
`MySql PostgreSql` is invisible to a SQLite connection, so the same key can carry a different
statement per engine.

> **Note.** Column matching ignores underscores by default, so `full_name` fills `FullName`. This is
> Dapper's process-wide setting and it applies to **settable properties only** — a positional
> `record` will not materialize from underscored columns. Turn it off with
> `DataOperations:MatchNamesWithUnderscores`.

Full documentation at [projectapricot.dev](https://projectapricot.dev).
