namespace BackgroundEmailSenderSample.Services {
    public interface IEmailSender
    {
        void Post(string subject, string body, string recipients, string sender);
    }
}