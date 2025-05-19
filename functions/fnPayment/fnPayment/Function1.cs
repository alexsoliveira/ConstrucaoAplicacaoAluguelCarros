using Azure.Messaging.ServiceBus;
using fnPayment.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace fnPayment;

public class Payment
{
    private readonly ILogger<Payment> _logger;
    private readonly IConfiguration _configuration;
    private readonly string[] StatusList = { "Aprovado", "Reprovado", "Em análise" };
    private readonly Random random = new Random();

    public Payment(ILogger<Payment> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function(nameof(Payment))]
    [CosmosDBOutput("%CosmosDB%", "%CosmosContaineer%", Connection = "CosmosDBConnection", CreateIfNotExists = true)]
    public async Task<object?> Run(
        [ServiceBusTrigger("payment-queue", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

        PaymantModel paymant = null;

        try
        {
            paymant = JsonSerializer.Deserialize<PaymantModel>(message.Body.ToString(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if(paymant == null)
            {
                await messageActions.DeadLetterMessageAsync(message, null, "The message could not be deserialized.");
            }
            int index = random.Next(StatusList.Length);
            string status = StatusList[index];
            paymant.status = status;

            if(status == "Aprovado")
            {
                paymant.DataAprovacao = DateTime.UtcNow;
                await SentToNotificationQueue(paymant);
            }
            return paymant;
        }
        catch (Exception ex)
        {

            await messageActions.DeadLetterMessageAsync(message, null, $"Erro:{ex.Message}");
            return null;
        }
        finally
        {
            await messageActions.CompleteMessageAsync(message);
        }                

    }    

    private async Task SentToNotificationQueue(PaymantModel paymant)
    {
        var connectionString = _configuration.GetSection("ServiceBusConnection").Value.ToString();
        var queueName = _configuration.GetSection("NotificationQueue").Value.ToString();

        var serviceBusClient = new ServiceBusClient(connectionString);
        var sender = serviceBusClient.CreateSender(queueName);
        var message = new ServiceBusMessage(JsonSerializer.Serialize(paymant))
        {
            ContentType = "application/json",            
        };

        message.ApplicationProperties["IdPayment"] = paymant.IdPayment;
        message.ApplicationProperties["type"] = "notification";
        message.ApplicationProperties["message"] = "Pagamento Aprovado com sucesso.";

        try
        {
            await sender.SendMessageAsync(message);
            _logger.LogInformation("Message sent to notification queue: {id}", paymant.IdPayment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to notification queue");
        }
        finally
        {
            await sender.DisposeAsync();
            await serviceBusClient.DisposeAsync();
        }
    }
}