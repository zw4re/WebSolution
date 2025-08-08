using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Hangfire;

namespace Admin.Controllers
{
    // Controller sadece MVC (Razor View) üzerinden gelen istekleri işler.
    [Authorize]
    [Route("jobs")] 
    public class JobsMvcController : Controller
    {
        [HttpPost("trigger/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Trigger(string id)
        {
            // ID boş veya geçersizse kullanıcıya hata mesajı dön
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["JobMsg"] = "Geçersiz job ID.";
                TempData["JobMsgType"] = "danger";
                return Redirect(Url.Action("Index", "Dashboard") + "#jobsSection");
            }
            // Hangfire üzerinden ilgili job'u hemen çalıştır
            RecurringJob.TriggerJob(id);
            // Kullanıcıya bilgi mesajı göster
            TempData["JobMsg"] = $"Job tetiklendi: {id}";
            TempData["JobMsgType"] = "success";
            return Redirect(Url.Action("Index", "Dashboard") + "#jobsSection");
        }
        // Belirtilen job ID'sini Hangfire'dan kaldırır.
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["JobMsg"] = "Geçersiz job ID.";
                TempData["JobMsgType"] = "danger";
                return Redirect(Url.Action("Index", "Dashboard") + "#jobsSection");
            }

            // Hangfire'dan ilgili job'u kaldır (void döner)
            RecurringJob.RemoveIfExists(id);

            // Kullanıcıya işlem sonucu hakkında bilgi ver
            TempData["JobMsg"] = $"Job silindi: {id}";
            TempData["JobMsgType"] = "success";

            return Redirect(Url.Action("Index", "Dashboard") + "#jobsSection");
        }

    }
}
