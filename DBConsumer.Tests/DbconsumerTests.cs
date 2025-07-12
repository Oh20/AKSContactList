using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

[TestFixture]
public class DBConsumerTests
{
    private Mock<IConnectionFactory> _connectionFactoryMock;
    private Mock<IModel> _channelMock;
    private Mock<IConnection> _connectionMock;
    private Mock<IContactRepository> _repositoryMock;
    private DbContextOptions<AppDbContext> _dbContextOptions;

    [SetUp]
    public void Setup()
    {
        _connectionFactoryMock = new Mock<IConnectionFactory>();
        _channelMock = new Mock<IModel>();
        _connectionMock = new Mock<IConnection>();
        _repositoryMock = new Mock<IContactRepository>();

        // Setup para mocks de RabbitMQ
        _connectionFactoryMock.Setup(f => f.CreateConnection()).Returns(_connectionMock.Object);
        _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);

        // Configurar banco de dados em memória para testes
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    }

    [Test]
    public async Task ConsumeMessage_ShouldSaveContact_WhenMessageIsValid()
    {
        // Arrange
        var contact = new ContactDto
        {
            Nome = "John Doe",
            Telefone = "123456789",
            Email = "john@example.com"
        };

        var message = JsonSerializer.Serialize(contact);
        var body = Encoding.UTF8.GetBytes(message);

        var consumer = new DBConsumer(_repositoryMock.Object);

        // Act
        await consumer.ConsumeMessage(body);

        // Assert
        _repositoryMock.Verify(r => r.SaveContact(It.IsAny<ContactDto>()), Times.Once);
    }

    [Test]
    public async Task ConsumeMessage_ShouldNotSaveContact_WhenMessageIsInvalid()
    {
        // Arrange
        var invalidMessage = "Invalid Message";
        var body = Encoding.UTF8.GetBytes(invalidMessage);

        var consumer = new DBConsumer(_repositoryMock.Object);

        // Act
        await consumer.ConsumeMessage(body);

        // Assert
        _repositoryMock.Verify(r => r.SaveContact(It.IsAny<ContactDto>()), Times.Never);
    }

    [Test]
    public async Task ConsumeMessage_ShouldHandleException_WhenRepositoryThrows()
    {
        // Arrange
        var contact = new ContactDto
        {
            Nome = "John Doe",
            Telefone = "123456789",
            Email = "john@example.com"
        };

        var message = JsonSerializer.Serialize(contact);
        var body = Encoding.UTF8.GetBytes(message);

        _repositoryMock.Setup(r => r.SaveContact(It.IsAny<ContactDto>()))
            .Throws(new Exception("Database error"));

        var consumer = new DBConsumer(_repositoryMock.Object);

        // Act and Assert
        Assert.DoesNotThrowAsync(async () => await consumer.ConsumeMessage(body));
    }

    [Test]
    public async Task DatabaseIntegration_ShouldSaveContact_WhenUsingInMemoryDatabase()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        var repository = new ContactRepository(dbContext);
        var consumer = new DBConsumer(repository);

        var contact = new ContactDto
        {
            Nome = "Test User",
            Telefone = "11987654321",
            Email = "test@example.com"
        };

        var message = JsonSerializer.Serialize(contact);
        var body = Encoding.UTF8.GetBytes(message);

        // Act
        await consumer.ConsumeMessage(body);

        // Assert
        var savedContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Email == contact.Email);
        Assert.IsNotNull(savedContact);
        Assert.AreEqual(contact.Nome, savedContact.Nome);
        Assert.AreEqual(contact.Telefone, savedContact.Telefone);
        Assert.AreEqual(contact.Email, savedContact.Email);
    }

    [Test]
    public async Task DatabaseIntegration_ShouldNotSaveDuplicateContact()
    {
        // Arrange
        using var dbContext = new AppDbContext(_dbContextOptions);
        var repository = new ContactRepository(dbContext);
        var consumer = new DBConsumer(repository);

        var contact = new ContactDto
        {
            Nome = "Duplicate User",
            Telefone = "11987654322",
            Email = "duplicate@example.com"
        };

        var message = JsonSerializer.Serialize(contact);
        var body = Encoding.UTF8.GetBytes(message);

        // Act - Salvar o mesmo contato duas vezes
        await consumer.ConsumeMessage(body);
        await consumer.ConsumeMessage(body);

        // Assert - Deve ter apenas um contato salvo
        var savedContacts = await dbContext.Contacts.Where(c => c.Email == contact.Email).ToListAsync();
        Assert.AreEqual(1, savedContacts.Count);
    }
}

// Exemplo de implementação do DBConsumer
public class DBConsumer
{
    private readonly IContactRepository _repository;

    public DBConsumer(IContactRepository repository)
    {
        _repository = repository;
    }

    public async Task ConsumeMessage(byte[] body)
    {
        try
        {
            var message = Encoding.UTF8.GetString(body);
            var contact = JsonSerializer.Deserialize<ContactDto>(message);

            if (contact != null)
            {
                await _repository.SaveContact(contact);
            }
        }
        catch (Exception ex)
        {
            // Log ou tratamento de exceção
            Console.WriteLine($"Erro ao consumir mensagem: {ex.Message}");
        }
    }
}

// Interface simulada para o repositório de dados
public interface IContactRepository
{
    Task SaveContact(ContactDto contact);
}

// Implementação real do repositório para testes de integração
public class ContactRepository : IContactRepository
{
    private readonly AppDbContext _dbContext;

    public ContactRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveContact(ContactDto contact)
    {
        // Verificar duplicação
        var existingContact = await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Email == contact.Email || c.Telefone == contact.Telefone);

        if (existingContact == null)
        {
            var newContact = new Contact
            {
                Nome = contact.Nome,
                Telefone = contact.Telefone,
                Email = contact.Email
            };

            _dbContext.Contacts.Add(newContact);
            await _dbContext.SaveChangesAsync();
        }
    }
}

// DTO simples
public class ContactDto
{
    public string Nome { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
}
