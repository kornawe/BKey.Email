
using Amazon.CDK;

namespace BKey.Emai.Serverless.Infra;

public class ServerlessEmailStackProps : StackProps
{
    public string DomainName { get; set; }
    public string EnvironmentName { get; set; }
}