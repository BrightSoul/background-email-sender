using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackgroundEmailSenderSample.Models;
using Microsoft.Extensions.Configuration;
using BackgroundEmailSenderSample.Services;

namespace BackgroundEmailSenderSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public IActionResult SendMail(
            [FromServices] IEmailSender emailSender,
            [FromServices] IConfiguration configuration,
            Contact contact) 
        {
            emailSender.Post(
                subject: "Contact request", 
                body: $"{contact.Name} ({contact.Email}) has sent you this message: {contact.Request}. He knew about us from: {contact.Source}",
                sender: contact.Email,
                recipients: configuration["AdminContact"]);
            return RedirectToAction(nameof(ThankYou));
        }

        public IActionResult ThankYou() {
            return View();
        }

    }
}
