// tests/Library.Tests/TestHelpers/DatabaseCollection.cs
using Xunit;

namespace Library.Tests.TestHelpers;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<SqlServerContainerFixture>
{
    // Cette classe n'a besoin d'aucun code — elle sert juste de marqueur pour xUnit
}