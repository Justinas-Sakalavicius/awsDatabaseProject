using Amazon.Util;
using Microsoft.AspNetCore.Mvc;

namespace awsDatabase.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            return Ok(EC2InstanceMetadata.InstanceId.ToString() + " " + EC2InstanceMetadata.PrivateIpAddress.ToString());
        }

        [HttpGet("/Region")]
        public IActionResult Region()
        {
            return Ok(EC2InstanceMetadata.Region.ToString() + " " + EC2InstanceMetadata.AvailabilityZone.ToString());
        }
    }
}
