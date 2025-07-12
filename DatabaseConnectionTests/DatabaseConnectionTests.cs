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
            // Obter connection string das variáveis de ambiente ou usar a padrão
            var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") 
                ?? "Server=tcp:freetiertestdb.database.windows.net,1433;Initial Catalog=azlabtest;Persist Security Info=False;User ID=azureadmin;Password=Tecnologia@@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Connection string não definida.");
                throw new InvalidOperationException("Connection string não definida nas variáveis de ambiente.");
            }

            Console.WriteLine("Connection string configurada");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Assert.AreEqual(System.Data.ConnectionState.Open, connection.State, "Falha ao conectar-se com o Banco de Dados.");
                
                // Teste adicional: executar uma query simples
                using (var command = new SqlCommand("SELECT 1", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    Assert.AreEqual(1, result, "Falha ao executar query de teste.");
                }
            }
        }

        [Test]
        [Category("DatabaseIntegration")]
        public async Task TestDatabaseConnectionWithTimeout()
        {
            var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") 
                ?? "Server=tcp:freetiertestdb.database.windows.net,1433;Initial Catalog=azlabtest;Persist Security Info=False;User ID=azureadmin;Password=Tecnologia@@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=5;";

            using (var connection = new SqlConnection(connectionString))
            {
                // Teste com timeout reduzido
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                
                try
                {
                    await connection.OpenAsync(cts.Token);
                    Assert.AreEqual(System.Data.ConnectionState.Open, connection.State);
                }
                catch (OperationCanceledException)
                {
                    Assert.Fail("Conexão com banco de dados excedeu o timeout.");
                }
            }
        }

        [Test]
        [Category("DatabaseIntegration")]
        public async Task TestDatabaseTableExists()
        {
            var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING") 
                ?? "Server=tcp:freetiertestdb.database.windows.net,1433;Initial Catalog=azlabtest;Persist Security Info=False;User ID=azureadmin;Password=Tecnologia@@2025;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                // Verificar se a tabela Contatos existe
                using (var command = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Contatos'", connection))
                {
                    var result = await command.ExecuteScalarAsync();
                    var tableExists = Convert.ToInt32(result) > 0;
                    Assert.IsTrue(tableExists, "Tabela 'Contatos' não encontrada no banco de dados.");
                }
            }
        }
    }
}