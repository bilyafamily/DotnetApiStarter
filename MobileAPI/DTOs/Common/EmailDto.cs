using System.Net.Mail;

namespace MobileAPI.DTOs.Common;

public class EmailDto
{
    public MailAddress? From { get; set; }
    public MailAddress To { get; set; }
    public string CC { get; set; } = "";
    public string Subject { get; set; }
    public string PlainTextContent { get; set; }
    public string HtmlContent { get; set; }
    public string AttachementBase64 { get; set; } = "";
    public string AttachmentName { get; set; } = "";
}