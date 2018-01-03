using System.ComponentModel.DataAnnotations;

namespace BackgroundEmailSenderSample.Models
{
    public class Contact
    {
        //Mandatory, 30 chars maximum
        [Required, StringLength(30), Display(Name = "Your first and last name")]
        public string Name { get; set; }

        //Mandatory, must be a valid e-mail address
        [Required, EmailAddress, Display(Name = "E-mail address")]
        public string Email { get; set; }

        [Required, StringLength(100), Display(Name = "How did you hear about us?")]
        public string Source { get; set; }

        //Mandatory, 1000 chars maximum
        [Required, StringLength(1000), Display(Name = "Your message to us")]
        public string Request { get; set; }
    }
}