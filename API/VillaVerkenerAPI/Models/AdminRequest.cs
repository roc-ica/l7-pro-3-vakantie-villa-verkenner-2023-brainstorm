using VillaVerkenerAPI.Models.DB;

namespace VillaVerkenerAPI.Models
{
    public class AdminRequest
    {
        public string Message { get; set; }
        public int RequestID { get; set; }
        public string Email { get; set; }
        public string RequestMessage { get; set; }
   
        public AdminRequest(Request request)
        {
            Message = "Success";
            RequestID = request.RequestId;
            Email = request.Email;
            RequestMessage = request.Message;
        }
        public static AdminRequest From(Request request)
        {
            return new AdminRequest(request);
        }
    }
}
