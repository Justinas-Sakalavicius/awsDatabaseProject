using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace awsDatabase.Services
{
    public interface INotificationService
    {
        Task SubscribeEmail(string email);

        Task UnsubscribeEmail(string email);

        Task SendMessageToQueue(string message);

        Task SendMessageToTopic(string message);

        Task<List<Message>> ReadMessages();
    }

    public class NotificationService : INotificationService
    {
        private readonly string SNSProtocol = "email";
        private readonly IAmazonSQS _sqs;
        private readonly IAmazonSimpleNotificationService _sns;
        private readonly string _queueUrl = "https://sqs.eu-north-1.amazonaws.com/530375214676/aws-task9-uploads-notification-queue";
        private readonly string _topicArn = "arn:aws:sns:eu-north-1:530375214676:aws-task9-uploads-notification-topic";

        public NotificationService(
            IAmazonSQS sqs,
            IAmazonSimpleNotificationService sns)
        {
            _sqs = sqs;
            _sns = sns;
        }

        public async Task SubscribeEmail(string email)
        {
            try
            {
                var subscribeRequest = new SubscribeRequest
                {
                    TopicArn = _topicArn,
                    Protocol = SNSProtocol,
                    Endpoint = email
                };
                await _sns.SubscribeAsync(subscribeRequest);
            }
            catch (AmazonSimpleNotificationServiceException asex)
            {
                Console.WriteLine($"Amazon notification service error subscribing email: {asex.Message}");
                throw;
            }
            catch (AmazonServiceException asex)
            {
                Console.WriteLine($"Amazon service error subscribing email: {asex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error subscribing email: {ex.Message}");
                throw;
            }
        }

        public async Task UnsubscribeEmail(string email)
        {
            try
            {
                var subscriptions = await _sns.ListSubscriptionsByTopicAsync(_topicArn);
                foreach (var subscription in subscriptions.Subscriptions)
                {
                    if (subscription.Endpoint == email)
                    {
                        await _sns.UnsubscribeAsync(subscription.SubscriptionArn);
                        break;
                    }
                }
            }
            catch (AmazonSimpleNotificationServiceException asex)
            {
                Console.WriteLine($"Amazon notification service error unsubscribing email: {asex.Message}");
                throw;
            }
            catch (AmazonServiceException asex)
            {
                Console.WriteLine($"Amazon service error unsubscribing email: {asex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error unsubscribing email: {ex.Message}");
                throw;
            }
        }

        public async Task SendMessageToQueue(string message)
        {
            try
            {
                var sendRequest = new SendMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MessageBody = message,
                    DelaySeconds = 5
                };
                await _sqs.SendMessageAsync(sendRequest);
            }
            catch (AmazonSQSException ex)
            {
                Console.WriteLine($"Amazon queue service error while sending the message to queue: {ex.Message}");
            }
            catch (AmazonServiceException asex)
            {
                Console.WriteLine($"Amazon service error while sending the message to queue: {asex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error while sending the message to queue: {ex.Message}");
                throw;
            }
        }

        public async Task SendMessageToTopic(string message)
        {
            try
            {
                var publishRequest = new PublishRequest
                {
                    TopicArn = _topicArn,
                    Message = message
                };
                await _sns.PublishAsync(publishRequest);
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                Console.WriteLine($"Amazon notification service error while sending the message to topic: {ex.Message}");
            }
            catch (AmazonServiceException asex)
            {
                Console.WriteLine($"Amazon service error while sending the message to topic: {asex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error while sending the message to topic: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Message>> ReadMessages()
        {
            var receiveRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 10
            };

            var messages = new List<Message>();
            try
            {
                var response = await _sqs.ReceiveMessageAsync(receiveRequest);
                foreach (var message in response.Messages)
                {
                    await _sqs.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
                }
                return response.Messages;
            }
            catch (AmazonSQSException ex)
            {
                Console.WriteLine($"Amazon queue service error while reading messages: {ex.Message}");
            }
            catch (AmazonServiceException asex)
            {
                Console.WriteLine($"Amazon service error reading messages: {asex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error reading messages: {ex.Message}");
                throw;
            }

            return messages;
        }
    }
}
