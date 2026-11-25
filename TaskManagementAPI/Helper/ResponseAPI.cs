namespace TaskManagementAPI.Helper
{
    public class ResponseAPI
    {
        public ResponseAPI(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetMessageFromStatusCode(statusCode);
        }

        public int StatusCode { get; set; }
        public string? Message { get; set; }

        private static string? GetMessageFromStatusCode(int statusCode)
        {
            return statusCode switch
            {
                200 => "Done",
                201 => "Created successfully",
                400 => "Bad Request",
                401 => "Unauthorized",
                404 => "Resource not found",
                500 => "Server error",
                _ => null
            };
        }
    }
}
