namespace ApricotFramework.DataOps.Tests;

/// <summary>
/// Definition documents the tests run against.
/// </summary>
internal static class Operations
{
    public const string People = """
        <?xml version="1.0" encoding="utf-8" ?>
        <DataOperations>
          <DataConfiguration Compatibility="Sqlite" Timeout="PT30S" />
          <OperationGroup Name="People">
            <SqlOperation Name="Insert">
              <TextCommand ExpectedResult="RowCount">
                INSERT INTO people (first_name, age) VALUES (@FirstName, @Age)
              </TextCommand>
            </SqlOperation>
            <SqlOperation Name="All">
              <TextCommand ExpectedResult="Table">
                SELECT id, first_name, age FROM people ORDER BY id
              </TextCommand>
            </SqlOperation>
            <SqlOperation Name="ById">
              <TextCommand ExpectedResult="Table">
                SELECT id, first_name, age FROM people WHERE id = @Id
              </TextCommand>
            </SqlOperation>
            <SqlOperation Name="Count">
              <TextCommand ExpectedResult="Scalar">
                SELECT COUNT(*) FROM people
              </TextCommand>
            </SqlOperation>
            <SqlOperation Name="AllAndCount">
              <TextCommand ExpectedResult="MultipleTables">
                SELECT id, first_name, age FROM people ORDER BY id; SELECT COUNT(*) FROM people;
              </TextCommand>
            </SqlOperation>
            <SqlOperation Name="Sorted">
              <TextCommand ExpectedResult="Table">
                SELECT id, first_name, age FROM people ORDER BY {if:byAge{ age DESC } else { first_name }}
              </TextCommand>
            </SqlOperation>
            <SqlOperation Name="Fail">
              <TextCommand ExpectedResult="RowCount">
                INSERT INTO people (id, first_name, age) VALUES (@Id, NULL, @Age)
              </TextCommand>
            </SqlOperation>
          </OperationGroup>
        </DataOperations>
        """;

    /// <summary>
    /// The same group with an operation that opens its own serializable transaction.
    /// </summary>
    public const string PeopleTransactional = """
        <?xml version="1.0" encoding="utf-8" ?>
        <DataOperations>
          <DataConfiguration Compatibility="Sqlite" AutoTransaction="Serializable" />
          <OperationGroup Name="Txn">
            <SqlOperation Name="Insert">
              <TextCommand ExpectedResult="RowCount">
                INSERT INTO people (first_name, age) VALUES (@FirstName, @Age)
              </TextCommand>
            </SqlOperation>
            <SqlOperation Name="InsertNoTransaction" AutoTransaction="No">
              <TextCommand ExpectedResult="RowCount">
                INSERT INTO people (first_name, age) VALUES (@FirstName, @Age)
              </TextCommand>
            </SqlOperation>
            <SqlOperation Name="Fail">
              <TextCommand ExpectedResult="RowCount">
                INSERT INTO people (first_name, age) VALUES (NULL, @Age)
              </TextCommand>
            </SqlOperation>
          </OperationGroup>
        </DataOperations>
        """;
}

/// <summary>
/// A row of the people table, named to exercise underscore column matching.
/// </summary>
internal sealed class Person
{
    public long Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public int Age { get; set; }
}
