using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using MobileAPI.DTOs.Common;
using MobileAPI.Services.IService;

namespace MobileAPI.Services;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;
    
    public NotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SendEmailNotification(EmailDto email)
    {
            
        var bodyText = email.HtmlContent;
        var body = new PowerAutomateEmailDto
        {
            subject = email.Subject,
            receipient = email.To.Address,
            cc = email.CC,
            body = bodyText,
        };
            
        var serializeContent = JsonSerializer.Serialize(body);
        var content = new StringContent(serializeContent, Encoding.UTF8, "application/json");

        var baseUrl = _configuration.GetSection("PowerAutomate:BaseUrl").Value ?? "";
        var trigger = _configuration.GetSection("PowerAutomate:Trigger").Value ?? "";
        
        var client = new HttpClient();
        client.BaseAddress = new Uri(baseUrl);
        try
        { 
            await client.PostAsync(trigger, content);
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
        }
    }

}