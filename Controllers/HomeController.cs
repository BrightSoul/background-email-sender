using System.Threading.Tasks;
using BackgroundEmailSenderSample.Models.InputModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BackgroundEmailSenderSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SendMail(ContactInputModel inputModel, [FromServices] IEmailSender emailSender) 
        {
            await emailSender.SendEmailAsync(inputModel.Email, "Request from our website", inputModel.ToHtmlMessage());
            return RedirectToAction(nameof(ThankYou));
        }
        
        public IActionResult ThankYou() {
            return View();
        }

    }
}
