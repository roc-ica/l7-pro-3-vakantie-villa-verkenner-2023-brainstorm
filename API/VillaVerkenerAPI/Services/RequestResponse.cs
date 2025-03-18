namespace VillaVerkenerAPI.Services;

public struct RequestResponse
{
    public bool Success = false;
    public string Message { get; set; } = "";
    public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();

    public RequestResponse(bool success, string message, Dictionary<string, string> data)
    {
        Success = success;
        Message = message;
        Data = data;
    }
    public static RequestResponse Successfull(string message = "Success", Dictionary<string, string> data = null)
    {
        return new RequestResponse(true, message, data ?? new Dictionary<string, string>());
    }
    public static RequestResponse Failed(string message = "Failed", Dictionary<string, string> data = null)
    {
        return new RequestResponse(false, message, data ?? new Dictionary<string, string>());
    }
}