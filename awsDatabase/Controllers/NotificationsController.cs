using awsDatabase.Services;
using Microsoft.AspNetCore.Mvc;

namespace awsDatabase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("/subscriptions/{email}")]
        public async Task<IActionResult> SubscribeEmail(string email)
        {
            try
            {
                await _notificationService.SubscribeEmail(email);
                return Ok($"Successfully send subscription confimation to email: {email}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("/subscriptions/{email}")]
        public async Task<IActionResult> UnsubscribeEmail(string email)
        {
            try
            {
                await _notificationService.UnsubscribeEmail(email);
                return Ok($"Successfully removed email subscription from email: {email}");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
