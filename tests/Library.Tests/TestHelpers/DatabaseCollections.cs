// tests/Library.Tests/TestHelpers/DatabaseCollection.cs
namespace Library.Tests.TestHelpers;

[CollectionDefinition("Database collection")]
public class DatabaseCollections : ICollectionFixture<SqlServerContainerFixture>
{
    // Cette classe n'a besoin d'aucun code — elle sert juste de marqueur pour xUnit
}