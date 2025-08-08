using Microsoft.AspNetCore.Mvc;
using Hangfire;
using Admin.Services;
using Entities.Presentation;
using Microsoft.AspNetCore.Authorization;

namespace Admin.Controllers
{
    
    [Route("api/jobs")]
    [ApiController]
    [Authorize] 
    public class JobsController : ControllerBase
    {
        private readonly RedisService _redisService;

        // RedisService DI’dan gelir (Program.cs'de AddSingleton<RedisService>() var)
        public JobsController(RedisService redisService)
        {
            _redisService = redisService;
        }

        // GET /api/jobs/recurring  → Tüm Recurring Job'ları döner
        [HttpGet("recurring")]
        public ActionResult<List<RecurringJobInfo>> GetRecurringJobs()
        {
            var jobs = _redisService.GetRecurringJobs();
            return Ok(jobs);
        }

        // POST /api/jobs/trigger/{id} → Job'ı anında tetikler
        [HttpPost("trigger/{id}")]
        public IActionResult Trigger(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("id gerekli");
            RecurringJob.TriggerJob(id);
            return NoContent(); // 204
        }

        // DELETE /api/jobs/{id} → Job'ı Hangfire'dan kaldırır
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("id gerekli");
            RecurringJob.RemoveIfExists(id); // void döner
            return NoContent(); // 204
        }
    }
}
