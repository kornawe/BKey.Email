using Amazon.CDK;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.CertificateManager;

namespace ServerlessEmailServer;

public class CognitoStack : Stack
{
    internal CognitoStack(Construct scope, string id, CognitoStackProps props) : base(scope, id, props)
    {
        string environmentName = props.EnvironmentName.ToLower();

        // Create Cognito User Pool
        var userPool = new UserPool(this, $"{environmentName}-UserPool", new UserPoolProps
        {
            UserPoolName = $"{props.UserPoolName}-{environmentName}",
            SignInAliases = new SignInAliases
            {
                Username = true,
                Email = true
            },
            SelfSignUpEnabled = true,
            PasswordPolicy = new PasswordPolicy
            {
                MinLength = 8,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireDigits = true,
                RequireSymbols = true
            },
            AccountRecovery = AccountRecovery.EMAIL_ONLY
        });

        // Create an App Client with OAuth Settings
        var appClient = new UserPoolClient(this, $"{environmentName}-AppClient", new UserPoolClientProps
        {
            UserPool = userPool,
            UserPoolClientName = $"{props.UserPoolName}-AppClient-{environmentName}",
            GenerateSecret = false,
            AuthFlows = new AuthFlow
            {
                UserPassword = true,
                UserSrp = true
            },
            SupportedIdentityProviders = new[]
            {
                UserPoolClientIdentityProvider.COGNITO
            },
            OAuth = new OAuthSettings
            {
                Flows = new OAuthFlows
                {
                    AuthorizationCodeGrant = true
                },
                Scopes = new[]
                {
                    OAuthScope.EMAIL,
                    OAuthScope.OPENID,
                    OAuthScope.PROFILE
                },
                CallbackUrls = new[] { $"https://{props.DomainName}/login" },
                LogoutUrls = new[] { $"https://{props.DomainName}/logout" }
            }
        });

        // Create a Custom Domain for Cognito hosted UI
        var customDomain = userPool.AddDomain($"{environmentName}-CustomDomain", new UserPoolDomainOptions
        {
            CustomDomain = new CustomDomainOptions
            {
                DomainName = $"{props.DomainName}",
                Certificate = new Certificate(this, $"{environmentName}-Certificate", new CertificateProps
                {
                    DomainName = props.DomainName,
                    Validation = CertificateValidation.FromDns()
                })
            }
        });

        // Get the existing hosted zone from Route 53
        var hostedZone = HostedZone.FromLookup(this, $"{environmentName}-HostedZone", new HostedZoneProviderProps
        {
            DomainName = props.DomainName.Replace("auth.", "") // Hosted zone without the "auth" subdomain
        });

        // Create a CNAME record in Route 53 for domain verification
        var cname = new CnameRecord(this, $"{environmentName}-CognitoCnameRecord", new CnameRecordProps
        {
            Zone = hostedZone,
            RecordName = "auth", // The subdomain part of the domain name
            DomainName = customDomain.CloudFrontDomainName // CNAME provided by Cognito
        });
    }
}