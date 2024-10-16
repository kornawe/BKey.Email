using Amazon.Lambda.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BKey.Email.Imap.Select;
public class SelectFunction
{
    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    public async Task<string> FunctionHandler(Dictionary<string, string> input, ILambdaContext context)
    {
        // TODO: Implement SELECT command logic
        return "SELECT command received";
    }
}
