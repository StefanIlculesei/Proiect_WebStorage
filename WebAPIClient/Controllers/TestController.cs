using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoggingLayer;

namespace WebAPIClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint to trigger an error and send email notification
        /// </summary>
        [HttpGet("trigger-error")]
        [AllowAnonymous]
        public IActionResult TriggerError()
        {
            try
            {
                // Simulate some work
                var data = new { id = 123, name = "Test User" };
                
                // Deliberately throw an exception
                throw new InvalidOperationException("This is a test error to verify email notifications are working correctly!");
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(TriggerError), ex, $"Test error triggered at {DateTime.UtcNow}");
                return StatusCode(500, new { 
                    Message = "Error triggered successfully! Check your email.",
                    Timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Test endpoint to trigger a complex error with inner exception
        /// </summary>
        [HttpGet("trigger-complex-error")]
        [AllowAnonymous]
        public IActionResult TriggerComplexError()
        {
            try
            {
                try
                {
                    // Simulate inner exception
                    throw new ArgumentNullException("userId", "User ID cannot be null");
                }
                catch (Exception innerEx)
                {
                    // Wrap in outer exception
                    throw new InvalidOperationException("Failed to process user request", innerEx);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(nameof(TriggerComplexError), ex, "userId: null, operation: getUserProfile");
                return StatusCode(500, new { 
                    Message = "Complex error triggered successfully! Check your email for nested exception details.",
                    Timestamp = DateTime.UtcNow 
                });
            }
        }
    }
}
