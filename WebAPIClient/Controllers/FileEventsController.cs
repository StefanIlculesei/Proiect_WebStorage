using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Accessors;
using System.Security.Claims;

namespace WebAPIClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileEventsController : ControllerBase
    {
        private readonly FileEventAccessor _fileEventAccessor;
        private readonly ILogger<FileEventsController> _logger;

        public FileEventsController(
            FileEventAccessor fileEventAccessor,
            ILogger<FileEventsController> logger)
        {
            _fileEventAccessor = fileEventAccessor;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFileEvents()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var events = await _fileEventAccessor.GetByUserIdAsync(userId);
            return Ok(events);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentEvents([FromQuery] int count = 10)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var events = await _fileEventAccessor.GetRecentEventsAsync(userId, count);
            return Ok(events);
        }

        [HttpGet("file/{fileId}")]
        public async Task<IActionResult> GetFileEvents(int fileId)
        {
            var events = await _fileEventAccessor.GetByFileIdAsync(fileId);
            return Ok(events);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFileEvent(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var fileEvent = await _fileEventAccessor.GetByIdAsync(id);

            if (fileEvent == null)
            {
                return NotFound(new { Message = "File event not found" });
            }

            if (fileEvent.UserId != userId)
            {
                return Forbid();
            }
            
            await _fileEventAccessor.DeleteAsync(fileEvent);
            await _fileEventAccessor.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDeleteFileEvents([FromBody] List<int> eventIds)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var deletedCount = 0;

            foreach (var eventId in eventIds)
            {
                var fileEvent = await _fileEventAccessor.GetByIdAsync(eventId);
                if (fileEvent != null && fileEvent.UserId == userId)
                {
                    await _fileEventAccessor.DeleteAsync(fileEvent);
                    deletedCount++;
                }
            }

            await _fileEventAccessor.SaveChangesAsync();

            return Ok(new { Message = $"{deletedCount} file events deleted", DeletedCount = deletedCount });
        }
    }
}
