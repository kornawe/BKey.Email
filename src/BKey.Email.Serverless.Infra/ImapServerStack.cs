using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using System.IO;

namespace BKey.Email.Serverless.Infra;
public class ImapServerStack : Stack
{
    public ImapServerStack(Construct scope, string id, ImapServerStackProps props) : base(scope, id, props)
    {
        string environmentName = props.EnvironmentName.ToLower();
        // Create individual Lambda functions for each IMAP command

        // LOGIN Lambda
        var loginLambda = new Function(this, $"IMAPLoginLambdaHandler-{environmentName}", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,
            Handler = "IMAPServerLambdas.Login::IMAPServerLambdas.LoginFunction::FunctionHandler",
            Code = Code.FromAsset(GetLambdaOutputPath()),
            Timeout = Duration.Seconds(30),
        });

        // SELECT Lambda
        var selectLambda = new Function(this, $"IMAPSelectLambdaHandler-{environmentName}", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,
            Handler = "IMAPServerLambdas.Select::IMAPServerLambdas.SelectFunction::FunctionHandler",
            Code = Code.FromAsset(GetLambdaOutputPath()),
            Timeout = Duration.Seconds(30),
        });

        // FETCH Lambda
        var fetchLambda = new Function(this, $"IMAPFetchLambdaHandler-{environmentName}", new FunctionProps
        {
            Runtime = Runtime.DOTNET_8,
            Handler = "IMAPServerLambdas.Fetch::IMAPServerLambdas.FetchFunction::FunctionHandler",
            Code = Code.FromAsset(GetLambdaOutputPath()),
            Timeout = Duration.Seconds(30),
        });

        // Create an API Gateway to expose IMAP-related endpoints
        var api = new RestApi(this, $"IMAPApiGateway-{environmentName}", new RestApiProps
        {
            RestApiName = "IMAP Service",
            Description = "API Gateway for IMAP server endpoints.",
        });

        // Create separate endpoints for each IMAP command

        // LOGIN endpoint
        var loginResource = api.Root.AddResource("login");
        loginResource.AddMethod("POST", new LambdaIntegration(loginLambda));

        // SELECT endpoint
        var selectResource = api.Root.AddResource("select");
        selectResource.AddMethod("POST", new LambdaIntegration(selectLambda));

        // FETCH endpoint
        var fetchResource = api.Root.AddResource("fetch");
        fetchResource.AddMethod("POST", new LambdaIntegration(fetchLambda));

        // Grant permissions for Lambda to write to logs
        foreach (var lambda in new[] { loginLambda, selectLambda, fetchLambda })
        {
            lambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] { "logs:CreateLogGroup", "logs:CreateLogStream", "logs:PutLogEvents" },
                Resources = new[] { "*" }
            }));
        }
    }

    private string GetLambdaOutputPath()
        {
            var directory = new DirectoryInfo("src/BKey.Email.Imap/bin/Debug/net8.0/");

            // Navigate to the Lambda project directory (assuming a structure like 'src/IMAPServerLambdas')
            var lambdaPath = directory.FullName;

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"Lambda binaries not found in: {lambdaPath}");
            }

            return lambdaPath;
        }
}