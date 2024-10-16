using Amazon.Lambda.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BKey.Email.Imap.Fetch;
public class FetchFunction
{
    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
    public async Task<string> FunctionHandler(Dictionary<string, string> input, ILambdaContext context)
    {
        // TODO: Implement FETCH command logic
        return "FETCH command received";
    }
}