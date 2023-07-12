using Amazon.Lambda;
using Amazon.Lambda.Model;
using Microsoft.AspNetCore.Mvc;

namespace awsDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LambdaController : ControllerBase
    {
        private readonly IAmazonLambda _lambdaClient;

        public LambdaController(IAmazonLambda lambdaClient)
        {
            _lambdaClient = lambdaClient;
        }

        [HttpGet("triggerLambda")]
        public async Task<IActionResult> TriggerLambda()
        {
            var request = new InvokeRequest
            {
                FunctionName = "AWS-Uploads-Batch-Notifier",
                InvocationType = InvocationType.RequestResponse,
                Payload = "{\"detail-type\":\"AWS Application\"}"
            };
            ///new KeyValuePair<string, string>("detail-type", "AWS Application").ToString()
            /// "{\"detail-type\":\"AWS Application\"}"

            var response = await _lambdaClient.InvokeAsync(request);
            var responseReader = new StreamReader(response.Payload);
            string responsePayload = responseReader.ReadToEnd();

            return Ok(responsePayload);
        }
    }
}
