using System.Data.SqlClient;

namespace DatabaseConnectionTests
{
    [TestFixture]
    public class DatabaseConnectionTests
    {
        [Test]
        [Category("DatabaseIntegration")]
        public async Task TestDatabaseConnection()
        {
            var connectionString = "Server=tcp:labserversqltreo.database.windows.net,1433;Initial Catalog=lab01;Persist Security Info=False;User ID=azureadmin;Password=Senha@2024;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            //var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Connection string não definida.");
                throw new InvalidOperationException("Connection string não definida nas variaves de ambiente.");
            }

            Console.WriteLine("Connection string: " + connectionString);

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Assert.AreEqual(System.Data.ConnectionState.Open, connection.State, "Falha ao conectar-se com o Banco de Dados.");
            }
        }
    }
}