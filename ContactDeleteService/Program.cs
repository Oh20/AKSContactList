using Prometheus;
using RabbitMQ.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDelete("/contatos/deleteID/{id}", (int id) =>
{
    var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "host.docker.internal";
    var rabbitMqPort = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672");

    // Configuração do RabbitMQ
    var factory = new ConnectionFactory() 
    {   
        HostName = rabbitMqHost,
        Port = rabbitMqPort
    };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    // Declaração da fila de exclusão de contatos
    channel.QueueDeclare(queue: "delete_contact_queue",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

    // Serializa o ID para enviar na fila
    var message = id.ToString();
    var body = Encoding.UTF8.GetBytes(message);

    // Publica a mensagem na fila de exclusão
    channel.BasicPublish(exchange: "",
                         routingKey: "delete_contact_queue",
                         basicProperties: null,
                         body: body);

    return Results.Ok($"Contato de ID: {id} Enviado para deleção!.");
});

app.UseHttpMetrics();  // Coleta de métricas HTTP automáticas

// Exponha o endpoint /metrics
app.MapMetrics();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
