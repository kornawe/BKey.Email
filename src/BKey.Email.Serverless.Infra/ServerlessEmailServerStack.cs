using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SES;
using Amazon.CDK.AWS.SES.Actions;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace BKey.Emai.Serverless.Infra
{
    public class ServerlessEmailServerStack : Stack
    {
        public ServerlessEmailServerStack(Construct scope, string id, ServerlessEmailStackProps props)
            : base(scope, id, props)
        {
            var envName = props.EnvironmentName.ToLowerInvariant();

            // Create an S3 bucket for storing emails
            var emailBucket = new Bucket(this, $"{envName}-EmailBucket", new BucketProps
            {
                BucketName = $"{envName}-email-storage-{props.DomainName}".Replace(".", "-"),
                Encryption = BucketEncryption.S3_MANAGED,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Create a DynamoDB table for storing email metadata
            var emailMetadataTable = new Table(this, $"{envName}EmailMetadataTable", new TableProps
            {
                TableName = $"{envName}-EmailMetadata-{props.DomainName}".Replace(".", "-"),
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "Id", Type = AttributeType.STRING },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Create an SNS topic for incoming emails
            var incomingEmailTopic = new Topic(this, $"{envName}IncomingEmailTopic", new TopicProps
            {
                TopicName = $"{envName}-incoming-email-topic-{props.DomainName}".Replace(".", "-")
            });

            // Create an SQS queue for processing emails
            var emailProcessingQueue = new Queue(this, $"{envName}EmailProcessingQueue", new QueueProps
            {
                QueueName = $"{envName}-email-processing-queue-{props.DomainName}".Replace(".", "-")
            });

            // Subscribe the SQS queue to the SNS topic
            incomingEmailTopic.AddSubscription(new SqsSubscription(emailProcessingQueue));

            // Create a Lambda function to process emails
            var emailProcessingFunction = new Function(this, $"{envName}EmailProcessingFunction", new FunctionProps
            {
                FunctionName = $"{envName}-email-processor-{props.DomainName}".Replace(".", "-"),
                Runtime = Runtime.DOTNET_8,
                Handler = "BKey.Email.Serverless::BKey.Email.Serverless.Function::FunctionHandler",
                Code = Code.FromAsset(GetLambdaOutputPath()),
                Environment = new Dictionary<string, string>
                {
                    { "QUEUE_URL", emailProcessingQueue.QueueUrl },
                    { "BUCKET_NAME", emailBucket.BucketName },
                    { "TABLE_NAME", emailMetadataTable.TableName },
                    { "ENVIRONMENT_NAME", envName }
                }
            });

            // Grant the Lambda function permissions
            emailProcessingQueue.GrantConsumeMessages(emailProcessingFunction);
            emailBucket.GrantReadWrite(emailProcessingFunction);
            emailMetadataTable.GrantReadWriteData(emailProcessingFunction);

            // Add the SQS queue as an event source for the Lambda function
            emailProcessingFunction.AddEventSource(new SqsEventSource(emailProcessingQueue));

            // Create an SES receipt rule set
            var ruleSet = new ReceiptRuleSet(this, $"{envName}EmailRuleSet", new ReceiptRuleSetProps
            {
                ReceiptRuleSetName = $"{envName}-incoming-email-rule-set-{props.DomainName}".Replace(".", "-")
            });

            // Create an SES receipt rule
            var rule = new ReceiptRule(this, $"{envName}EmailRule", new ReceiptRuleProps
            {
                RuleSet = ruleSet,
                Recipients = new[] { props.DomainName },
                Actions = new IReceiptRuleAction[]
                {
                    new AddHeader(new AddHeaderProps
                    {
                        Name = "X-Environment",
                        Value = envName
                    }),
                    new Sns(new SnsProps
                    {
                        Topic = incomingEmailTopic
                    })
                },
                ScanEnabled = true
            });

        }

        private string GetLambdaOutputPath()
        {
            var directory = new DirectoryInfo("src/BKey.Email.Serverless.EmailIngestion/bin/Debug/net8.0/");

            // Navigate to the Lambda project directory (assuming a structure like 'src/IMAPServerLambdas')
            var lambdaPath = directory.FullName;

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"Lambda binaries not found in: {lambdaPath}");
            }

            return lambdaPath;
        }
    }
}
