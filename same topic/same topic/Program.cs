using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

// entity model for  database
public class MessageEntity
{
    public int Id { get; set; }
    public string Content { get; set; }
}

//  DbContext class for  database
public class MyDbContext : DbContext
{
    public DbSet<MessageEntity> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=DESKTOP-SUJAN\\PERCOIDDB;Initial Catalog=KafkaTesting;Integrated Security=True;TrustServerCertificate=True;");
    }
}

public class KafkaProducerss
{
    public static async Task Main(string[] args)
    {
        while (true)
        {
            await CreateMessage();

            Console.WriteLine("Do you want to send another message? (y/n)");
            var response = Console.ReadLine();

            if (response?.ToLower() != "y")
            {
                break;
            }
        }
    }

    public static async Task CreateMessage()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            ClientId = "my-app",
            BrokerAddressFamily = BrokerAddressFamily.V4,
        };
        using var producer = new ProducerBuilder<Null, string>(config).Build();

        Console.WriteLine("Please enter the message you want to send");
        var input = Console.ReadLine();
        var message = new Message<Null, string>
        {
            Value = input
        };
        var deliveryReport = await producer.ProduceAsync("my-topic", message);  //topic in whic message is send to(needs to be created before hand)
        Console.WriteLine($"Message delivered to {deliveryReport.TopicPartitionOffset}");

        // Save the message to the local database
        SaveMessageToDatabase(input);
    }

    private static void SaveMessageToDatabase(string messageContent)
    {
        using var dbContext = new MyDbContext();
        var messageEntity = new MessageEntity { Content = messageContent };
        dbContext.Messages.Add(messageEntity);
        dbContext.SaveChanges();
    }
}
