using MobileAPI.DTOs.Common;

namespace MobileAPI.Services.IService;

public interface INotificationService
{
    Task SendEmailNotification(EmailDto email);
}