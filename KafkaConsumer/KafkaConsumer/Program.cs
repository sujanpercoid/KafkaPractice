using System;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

// Consumer table
public class ConsumerEntity
{
    public int Id { get; set; }
    public string Content { get; set; }
}

// DbContext for database
public class MyDbContext : DbContext
{
    public DbSet<ConsumerEntity> CMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=DESKTOP-SUJAN\\PERCOIDDB;Initial Catalog=KafkaConsumer;Integrated Security=True;TrustServerCertificate=True;");
    }
}

public class KafkaConsumerrr
{
    public static void Main()
    {
        ReadMessage();
    }

    public static void ReadMessage()
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            ClientId = "my-app",
            GroupId = "my-group",
            BrokerAddressFamily = BrokerAddressFamily.V4,
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        //Topics to consume
        consumer.Subscribe(new List<string> { "my-topic", "new_topic_testing" });



        try
        {
            while (true)
            {
                var consumeResult = consumer.Consume();
                Console.WriteLine($"Message received from {consumeResult.TopicPartitionOffset}: {consumeResult.Message.Value}");

                // Try to send data to the database
                SendDataToDatabase(consumeResult.Message.Value);
            }
        }
        catch (OperationCanceledException)
        {
            // The consumer was stopped via cancellation token.
        }
        finally
        {
            consumer.Close();
        }
    }

    public static void SendDataToDatabase(string messgaeContent)
    {
        using var dbContext = new MyDbContext();
        var consumerEntity = new ConsumerEntity { Content = messgaeContent };
        dbContext.CMessages.Add(consumerEntity);
        dbContext.SaveChanges();

    }
}