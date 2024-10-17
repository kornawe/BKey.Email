using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BKey.Email.Serverless.EmailIngestion;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _bucketName;
    private readonly string _tableName;

    public Function()
    {
        _s3Client = new AmazonS3Client();
        _dynamoDbClient = new AmazonDynamoDBClient();
        _bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");
        _tableName = Environment.GetEnvironmentVariable("TABLE_NAME");
    }

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        // Parse the SNS message from the SQS event
        var snsMessage = JsonSerializer.Deserialize<SNSMessage>(message.Body);
        var sesMessage = JsonSerializer.Deserialize<SESMessage>(snsMessage.Message);

        // Generate a unique ID for the email
        string emailId = Guid.NewGuid().ToString();

        // Save email content to S3
        await _s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = _bucketName,
            Key = $"emails/{emailId}",
            ContentBody = sesMessage.Content
        });

        // Save metadata to DynamoDB
        var table = Table.LoadTable(_dynamoDbClient, _tableName);
        var item = new Document
        {
            ["Id"] = emailId,
            ["Subject"] = sesMessage.Subject,
            ["From"] = sesMessage.From,
            ["To"] = sesMessage.To,
            ["Timestamp"] = DateTime.UtcNow.ToString("o")
        };
        await table.PutItemAsync(item);

        context.Logger.LogLine($"Processed message {message.MessageId}");
    }
}

// You'll need to define these classes based on the actual structure of your messages
public class SNSMessage
{
    public string Message { get; set; }
}

public class SESMessage
{
    public string Subject { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public string Content { get; set; }
}