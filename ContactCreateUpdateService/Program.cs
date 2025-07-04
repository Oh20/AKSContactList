using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.ComponentModel.DataAnnotations;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura��o de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configura��o do RabbitMQ com valores padr�o para Kubernetes
var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq-service";
var rabbitMqPort = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672");
var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";

// Log da configura��o para debugging
app.Logger.LogInformation($"RabbitMQ Configuration - Host: {rabbitMqHost}, Port: {rabbitMqPort}, User: {rabbitMqUser}");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.MapPost("/contatos", async ([FromBody] ContactDto contact) =>
{
    try
    {
        // Valida��o de modelo
        var validationContext = new ValidationContext(contact, null, null);
        var validationResults = new List<ValidationResult>();

        if (!Validator.TryValidateObject(contact, validationContext, validationResults, true))
        {
            var errors = validationResults.Select(v => v.ErrorMessage).ToList();
            return Results.BadRequest(new { Errors = errors });
        }

        // Configura��o do RabbitMQ com timeout
        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqHost,
            Port = rabbitMqPort,
            UserName = rabbitMqUser,
            Password = rabbitMqPassword,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
            SocketReadTimeout = TimeSpan.FromSeconds(30),
            SocketWriteTimeout = TimeSpan.FromSeconds(30)
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Declara��o da fila
        channel.QueueDeclare(queue: "contact_queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        // Serializa��o do objeto de contato para JSON
        var message = System.Text.Json.JsonSerializer.Serialize(contact);
        var body = Encoding.UTF8.GetBytes(message);

        // Propriedades da mensagem para persist�ncia
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        // Publica a mensagem na fila
        channel.BasicPublish(exchange: "",
                             routingKey: "contact_queue",
                             basicProperties: properties,
                             body: body);

        app.Logger.LogInformation($"Contato {contact.Nome} enviado para fila de cria��o");
        return Results.Ok(new { Message = "Contato direcionado � fila de cria��o", ContactName = contact.Nome });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erro ao processar cria��o de contato");
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Erro interno do servidor"
        );
    }
});

app.MapPut("/editcontatos/{name}", async (string name, [FromBody] ContactDto contact) =>
{
    try
    {
        // Valida��o b�sica
        if (string.IsNullOrWhiteSpace(contact.Nome) ||
            string.IsNullOrWhiteSpace(contact.Telefone) ||
            string.IsNullOrWhiteSpace(contact.Email))
        {
            return Results.BadRequest(new { Error = "Nome, Telefone e Email s�o obrigat�rios." });
        }

        // Atualiza o nome do contato com o par�metro da URL
        contact.Nome = name;

        // Configura��o do RabbitMQ
        var factory = new ConnectionFactory()
        {
            HostName = rabbitMqHost,
            Port = rabbitMqPort,
            UserName = rabbitMqUser,
            Password = rabbitMqPassword,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
            SocketReadTimeout = TimeSpan.FromSeconds(30),
            SocketWriteTimeout = TimeSpan.FromSeconds(30)
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Declara��o da fila
        channel.QueueDeclare(queue: "contact_update_queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        // Serializa��o do objeto
        var message = System.Text.Json.JsonSerializer.Serialize(contact);
        var body = Encoding.UTF8.GetBytes(message);

        // Propriedades da mensagem para persist�ncia
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        // Publica��o na fila
        channel.BasicPublish(exchange: "",
                             routingKey: "contact_update_queue",
                             basicProperties: properties,
                             body: body);

        app.Logger.LogInformation($"Contato {name} enviado para fila de atualiza��o");
        return Results.Ok(new { Message = $"Contato {name} enviado para fila de atualiza��o" });
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erro ao processar atualiza��o de contato");
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Erro interno do servidor"
        );
    }
});

app.UseAuthorization();
app.MapControllers();

// Configura��o para escutar em todas as interfaces na porta 8080
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:8080");

app.Run();

// DTO para o contato
public class ContactDto
{
    [Required(ErrorMessage = "Nome � obrigat�rio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefone � obrigat�rio")]
    [Phone(ErrorMessage = "Formato de telefone inv�lido")]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email � obrigat�rio")]
    [EmailAddress(ErrorMessage = "Formato de email inv�lido")]
    public string Email { get; set; } = string.Empty;
}