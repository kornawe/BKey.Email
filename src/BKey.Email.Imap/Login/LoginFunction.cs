using Amazon.Lambda.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BKey.Email.Imap.Login;
public class LoginFunction
{
    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    public async Task<string> FunctionHandler(Dictionary<string, string> input, ILambdaContext context)
    {
        // TODO: Implement LOGIN command logic
        return "LOGIN command received";
    }
}
