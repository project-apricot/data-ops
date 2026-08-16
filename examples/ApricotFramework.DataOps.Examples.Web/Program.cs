using ApricotFramework.DataOps;
using ApricotFramework.DataOps.AspNetCore.Extensions;
using ApricotFramework.DataOps.Examples.Web.Data;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Data sources come from the DataOperations section; this binds the Sqlite provider to a driver.
builder.Services.AddDataOperations(builder.Configuration);
builder.Services.AddDataSourceConnection(SqlProvider.Sqlite, connectionString => new SqliteConnection(connectionString));

// Both sources contribute to one registry, so a group from either is addressed the same way.
builder.Services.AddOperationsDefinitionSource<EmbeddedOperationsSource>();
builder.Services.AddOperationsDefinitionSource<FilesOperationsSource>();

builder.Services.AddSingleton<IAuthorRepository, AuthorRepository>();

var app = builder.Build();

CreateSchema(app.Services.GetRequiredService<IDataOperations>());

app.MapGet("/authors", (IAuthorRepository repository, bool byName = false) => repository.GetAll(byName));

app.MapGet("/authors/report", async (IAuthorRepository repository) =>
{
    var (authors, count) = await repository.GetReport();
    return Results.Ok(new { authors, count });
});

app.MapPost("/authors", async (IAuthorRepository repository, Author[] authors) =>
    Results.Ok(new { total = await repository.AddAll(authors) }));

app.Run();

// The example owns its schema so that running it needs nothing but the SQLite file.
static void CreateSchema(IDataOperations dataOps)
{
    using var reference = dataOps.ConnectDirect();
    reference.Connection.Open();

    using var command = reference.Connection.CreateCommand();
    command.CommandText = """
        CREATE TABLE IF NOT EXISTS authors (
            id       INTEGER PRIMARY KEY,
            full_name TEXT    NOT NULL,
            born_in   INTEGER NOT NULL
        );
        DELETE FROM authors;
        INSERT INTO authors (full_name, born_in) VALUES ('Ursula Le Guin', 1929), ('Italo Calvino', 1923);
        """;
    command.ExecuteNonQuery();
}
