using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura��o do DbContext lendo da vari�vel de ambiente
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
                          ?? builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString);
});

var app = builder.Build();

// Configura��o RabbitMQ com fallbacks consistentes
var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq.messaging.svc.cluster.local";
var rabbitMqPort = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672");
var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
var rabbitMqPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
var rabbitMqVHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/";

// Configurar RabbitMQ
var factory = new ConnectionFactory()
{
    HostName = rabbitMqHost,
    Port = rabbitMqPort,
    UserName = rabbitMqUser,
    Password = rabbitMqPassword,
    VirtualHost = rabbitMqVHost,
    RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
    RequestedHeartbeat = TimeSpan.FromSeconds(60),
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

//Fila de cria��o
channel.QueueDeclare(queue: "contact_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Mensagem recebida: {message}");

    // Desserializar a mensagem para o objeto ContactDto
    var contactDto = System.Text.Json.JsonSerializer.Deserialize<ContactDto>(message);

    if (contactDto != null)
    {
        // Validar manualmente as Data Annotations
        var validationContext = new ValidationContext(contactDto);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(contactDto, validationContext, validationResults, true);

        if (!isValid)
        {
            foreach (var validationResult in validationResults)
            {
                Console.WriteLine($"Falha na valida��o: {validationResult.ErrorMessage}");
            }
            return; // Se falhar, n�o salvar no banco
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Mapear o DTO para o modelo Contact
        var contact = new Contact { Nome = contactDto.Nome, Telefone = contactDto.Telefone, Email = contactDto.Email };

        // Verificar duplica��o de e-mail ou telefone
        var existingContact = await dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Email == contact.Email || c.Telefone == contact.Telefone);

        if (existingContact != null)
        {
            Console.WriteLine($"Email {contact.Email} j� existente ou Telefone {contact.Telefone} em uso");
            return; // Se duplicado, n�o salvar
        }

        // Salvar o contato no banco de dados
        dbContext.Contacts.Add(contact);
        await dbContext.SaveChangesAsync();

        Console.WriteLine($" {contact.Nome} Contato armazenado com sucesso.");
    }
};

channel.BasicConsume(queue: "contact_queue",
                     autoAck: true,
                     consumer: consumer);

Console.WriteLine("Aguardando novas solicita��es de cria��o.");

// Consumir fila de exclus�o
channel.QueueDeclare(queue: "delete_contact_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var deleteConsumer = new EventingBasicConsumer(channel);
deleteConsumer.Received += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Mensagem recebida: {message}");

    if (int.TryParse(message, out var contactId))
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var contact = await dbContext.Contacts.FindAsync(contactId);
        if (contact != null)
        {
            dbContext.Contacts.Remove(contact);
            await dbContext.SaveChangesAsync();

            Console.WriteLine($"Contato de ID: {contactId} deletado da base de dados.");
        }
        else
        {
            Console.WriteLine($"Contato de ID: {contactId} N�o localizado.");
        }
    }
};

channel.BasicConsume(queue: "delete_contact_queue", autoAck: true, consumer: deleteConsumer);

Console.WriteLine("Aguardando mensagens na fila de dele��o.");

// Consumir fila de atualiza��o
channel.QueueDeclare(queue: "contact_update_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

var updateConsumer = new EventingBasicConsumer(channel);
updateConsumer.Received += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"Recebido mensagem de atualiza��o: {message}");

    // Desserializar a mensagem para o objeto ContactDto
    var contactDto = System.Text.Json.JsonSerializer.Deserialize<ContactDto>(message);

    if (contactDto != null)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Procurar o contato pelo nome no banco de dados
        var existingContact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Nome == contactDto.Nome);

        if (existingContact != null)
        {
            // Atualizar os dados do contato
            existingContact.Telefone = contactDto.Telefone;
            existingContact.Email = contactDto.Email;

            // Salvar as altera��es no banco de dados
            await dbContext.SaveChangesAsync();

            Console.WriteLine($"Contato de Nome {contactDto.Nome} atualizado com sucesso.");
        }
        else
        {
            Console.WriteLine($"Contato de Nome {contactDto.Nome} n�o localizado.");
        }
    }
};

channel.BasicConsume(queue: "contact_update_queue", autoAck: true, consumer: updateConsumer);

Console.WriteLine("Aguardando por Mensagens de Atualiza��o!.");

app.UseHttpMetrics();  // Coleta de m�tricas HTTP autom�ticas

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// Exponha o endpoint /metrics
app.MapMetrics();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.Run();

// DTO utilizado para desserializa��o
public class ContactDto
{
    [Required(ErrorMessage = "O campo Nome � obrigat�rio.")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "O campo Telefone � obrigat�rio.")]
    [Phone(ErrorMessage = "Formato de telefone inv�lido.")]
    public string Telefone { get; set; }

    [Required(ErrorMessage = "O campo Email � obrigat�rio.")]
    [EmailAddress(ErrorMessage = "Formato de email inv�lido.")]
    public string Email { get; set; }
}