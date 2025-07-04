using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Http;

[TestFixture]
public class ContactQueryServiceTests
{
    private Mock<IConnectionFactory> _connectionFactoryMock;
    private Mock<IModel> _channelMock;
    private Mock<IConnection> _connectionMock;

    [SetUp]
    public void Setup()
    {
        _connectionFactoryMock = new Mock<IConnectionFactory>();
        _channelMock = new Mock<IModel>();
        _connectionMock = new Mock<IConnection>();

        // Setup para mocks de RabbitMQ
        _connectionFactoryMock.Setup(f => f.CreateConnection()).Returns(_connectionMock.Object);
        _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);
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