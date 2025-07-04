using RabbitMQ.Client;
using System.Text;

public class RabbitMqChannel
{
    private readonly IModel _channel;

    public RabbitMqChannel(IModel channel)
    {
        _channel = channel;
    }

    public void Publish(string queueName, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }

    public void DeclareQueue(string queueName)
    {
        _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
    }
}
