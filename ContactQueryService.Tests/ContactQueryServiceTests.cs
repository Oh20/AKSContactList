using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[TestFixture]
public class ContactQueryServiceTests
{
    private Mock<IConnectionFactory> _connectionFactoryMock;
    private Mock<IModel> _channelMock;
    private Mock<IConnection> _connectionMock;
    private DbContextOptions<AppDbContext> _dbContextOptions;

    [SetUp]
    public void Setup()
    {
        _connectionFactoryMock = new Mock<IConnectionFactory>();
        _channelMock = new Mock<IModel>();
        _connectionMock = new Mock<IConnection>();

        // Setup para mocks de RabbitMQ
        _connectionFactoryMock.Setup(f => f.CreateConnection()).Returns(_connectionMock.Object);
        _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

        // Configurar banco de dados em memória para testes
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"ContactQueryTestDb_{Guid.NewGuid()}")
            .Options;
    }

    [Test]
    public async Task GetContactByDDD_ShouldReturnOk_WhenContactsExist()
    {
        // Arrange
        var ddd = "12";
        var contacts = new List<ContactDto>
        {
            new ContactDto { Nome = "John Doe", Telefone = "123456789", Email = "john@example.com" },
            new ContactDto { Nome = "Jane Smith", Telefone = "129876543", Email = "jane@example.com" }
        };

        // Simulação do retorno de contatos com o DDD especificado
        var httpContext = new DefaultHttpContext();

        // Act
        var result = await new Func<Task<IResult>>(() =>
            Task.FromResult(GetContactsByDDD(ddd, httpContext))).Invoke();

        // Assert
        Assert.IsInstanceOf<Ok<List<ContactDto>>>(result);
        var okResult = result as Ok<List<ContactDto>>;
        Assert.AreEqual(contacts.Count, okResult?.Value?.Count);
    }

    [Test]
    public async Task GetContactByDDD_ShouldReturnNotFound_WhenNoContactsExist()
    {
        // Arrange
        var ddd = "00"; // DDD inválido para simulação
        var httpContext = new DefaultHttpContext();

        // Act
        var result = await new Func<Task<IResult>>(() =>
            Task.FromResult(GetContactsByDDD(ddd, httpContext))).Invoke();

        // Assert
        Assert.IsInstanceOf<NotFound<string>>(result);
    }

    [Test]
    [Category("DatabaseIntegration")]
    public async Task GetAllContacts_ShouldReturnContacts_WhenUsingInMemoryDatabase()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        
        // Adicionar contatos de teste
        var testContacts = new List<Contact>
        {
            new Contact { Nome = "João Silva", Telefone = "11987654321", Email = "joao@example.com" },
            new Contact { Nome = "Maria Santos", Telefone = "21987654322", Email = "maria@example.com" },
            new Contact { Nome = "Pedro Costa", Telefone = "31987654323", Email = "pedro@example.com" }
        };

        await dbContext.Contacts.AddRangeAsync(testContacts);
        await dbContext.SaveChangesAsync();

        // Act
        var allContacts = await dbContext.Contacts.ToListAsync();

        // Assert
        Assert.AreEqual(3, allContacts.Count);
        Assert.IsTrue(allContacts.Any(c => c.Nome == "João Silva"));
        Assert.IsTrue(allContacts.Any(c => c.Nome == "Maria Santos"));
        Assert.IsTrue(allContacts.Any(c => c.Nome == "Pedro Costa"));
    }

    [Test]
    [Category("DatabaseIntegration")]
    public async Task GetContactsByDDD_ShouldReturnFilteredContacts_WhenUsingInMemoryDatabase()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        
        // Adicionar contatos de teste com diferentes DDDs
        var testContacts = new List<Contact>
        {
            new Contact { Nome = "João Silva", Telefone = "11987654321", Email = "joao@example.com" },
            new Contact { Nome = "Maria Santos", Telefone = "21987654322", Email = "maria@example.com" },
            new Contact { Nome = "Pedro Costa", Telefone = "11987654323", Email = "pedro@example.com" }
        };

        await dbContext.Contacts.AddRangeAsync(testContacts);
        await dbContext.SaveChangesAsync();

        // Act - Buscar contatos com DDD 11
        var ddd11Contacts = await dbContext.Contacts
            .Where(c => c.Telefone.StartsWith("11"))
            .ToListAsync();

        // Assert
        Assert.AreEqual(2, ddd11Contacts.Count);
        Assert.IsTrue(ddd11Contacts.All(c => c.Telefone.StartsWith("11")));
    }

    [Test]
    [Category("DatabaseIntegration")]
    public async Task GetContactById_ShouldReturnContact_WhenContactExists()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        
        var testContact = new Contact 
        { 
            Nome = "João Silva", 
            Telefone = "11987654321", 
            Email = "joao@example.com" 
        };

        await dbContext.Contacts.AddAsync(testContact);
        await dbContext.SaveChangesAsync();

        var contactId = testContact.Id;

        // Act
        var foundContact = await dbContext.Contacts.FindAsync(contactId);

        // Assert
        Assert.IsNotNull(foundContact);
        Assert.AreEqual("João Silva", foundContact.Nome);
        Assert.AreEqual("11987654321", foundContact.Telefone);
        Assert.AreEqual("joao@example.com", foundContact.Email);
    }

    [Test]
    [Category("DatabaseIntegration")]
    public async Task GetContactById_ShouldReturnNull_WhenContactDoesNotExist()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);

        // Act
        var foundContact = await dbContext.Contacts.FindAsync(999);

        // Assert
        Assert.IsNull(foundContact);
    }

    // Métodos simulando o comportamento da API
    public static IResult GetContactsByDDD(string ddd, HttpContext httpContext)
    {
        var contacts = new List<ContactDto>
        {
            new ContactDto { Nome = "John Doe", Telefone = "123456789", Email = "john@example.com" },
            new ContactDto { Nome = "Jane Smith", Telefone = "129876543", Email = "jane@example.com" }
        };

        var filteredContacts = contacts.Where(c => c.Telefone.StartsWith(ddd)).ToList();

        if (filteredContacts.Any())
        {
            return Results.Ok(filteredContacts);
        }
        else
        {
            return Results.NotFound($"Nenhum contato encontrado com o DDD {ddd}");
        }
    }

    // DTO
    public class ContactDto
    {
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
    }
}