
using Amazon.CDK;

namespace ServerlessEmailServer;

public class ServerlessEmailStackProps : StackProps
{
    public string DomainName { get; set; }
    public string EnvironmentName { get; set; }
}