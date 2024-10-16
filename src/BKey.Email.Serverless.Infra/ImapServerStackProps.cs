using Amazon.CDK;

namespace BKey.Email.Serverless.Infra;
public class ImapServerStackProps : StackProps
{
    public string EnvironmentName { get; set; }
}
