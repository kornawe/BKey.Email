using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SES;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace BKey.Emai.Serverless.Infra;

public class ServerlessEmailServerStack : Stack
{
    internal ServerlessEmailServerStack(Construct scope, string id, ServerlessEmailStackProps props) : base(scope, id, props)
    {
        string environmentName = props.EnvironmentName.ToLower();
        string domainName = props.DomainName;

        // Create an S3 bucket to hold emails
        var emailBucket = new Bucket(this, $"{environmentName}-EmailBucket", new BucketProps
        {
            BucketName = $"{environmentName}-emails-bucket",
            Versioned = true,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // Create a DynamoDB table to hold metadata information
        var emailMetadataTable = new Table(this, $"{environmentName}-EmailMetadataTable", new TableProps
        {
            TableName = $"{environmentName}-email-metadata",
            PartitionKey = new Attribute
            {
                Name = "MessageId",
                Type = AttributeType.STRING
            },
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // Set up SES to ingest emails
        var emailIdentity = new CfnEmailIdentity(this, $"{environmentName}-EmailIdentity", new CfnEmailIdentityProps
        {
            EmailIdentity = domainName
        });

        // Create an IAM role for the Lambda function
        var lambdaRole = new Role(this, $"{environmentName}-LambdaRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });
        lambdaRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Actions = new[] { "s3:PutObject", "s3:GetObject" },
            Resources = new[] { emailBucket.BucketArn + "/*" }
        }));
        lambdaRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Actions = new[] { "dynamodb:PutItem", "dynamodb:GetItem" },
            Resources = new[] { emailMetadataTable.TableArn }
        }));

        // Create a Lambda function to ingest emails
        var emailIngestLambda = new Function(this, $"{environmentName}-EmailIngestLambda", new FunctionProps
        {
            FunctionName = $"{environmentName}-email-ingest",
            Runtime = Runtime.NODEJS_14_X,
            Handler = "index.handler",
            Code = Code.FromAsset("lambda"), // Placeholder for Lambda code directory
            Role = lambdaRole
            
        });
    }
}