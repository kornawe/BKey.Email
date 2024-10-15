using Amazon.CDK;

namespace ServerlessEmailServer;

public class CognitoStackProps : StackProps
{
    public string DomainName { get; set; }
    public string UserPoolName { get; set; }

    public string EnvironmentName { get; set; }
}
