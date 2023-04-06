namespace Insky.Rfid;

public class RfidResult
{
    public RfidResult()
    {
    }

    public RfidResult(ResponseCode code, string message = null)
    {
        Code = code;
        Message = message;
    }

    public ResponseCode Code { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
}