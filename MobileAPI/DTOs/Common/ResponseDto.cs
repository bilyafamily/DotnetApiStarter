using System.Net;

namespace MobileAPI.DTOs.Common;

public class ResponseDto
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public object? Result { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string Message { get; set; } = "";
}