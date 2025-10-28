namespace SampleProject.Domain.Responses
{
    public class ErrorResponseModel
    {
        public ErrorResponseModel(Exception ex, int code, bool includeStack = true)
        {
            Success = false;
            Code = code;
            var ErrorList = new List<object>();
            ErrorList.Add($"MESSAGE: {ex.Message}");
            if (includeStack)
            {
                ErrorList.Add($"STACK: {ex.StackTrace}");
                ErrorList.Add($"INNER EXCEPTION: {ex.InnerException}");
            }
            Errors = ErrorList.ToArray();
        }

        public ErrorResponseModel()
        {
            Success = false;
            Code = 400;
            Errors = Array.Empty<object>();
        }

        public int Code { get; set; }
        public int StatusCode { get; set; }
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
        public string? Message { get; set; }
        public object? Details { get; set; }
        public object[] Errors { get; set; }
        public bool Success { get; set; }
    }

    public class ErrorResponseSimplySign
    {
        [JsonPropertyName("error")]
        public required string Error { get; set; }

        [JsonPropertyName("error_description")]
        public required string ErrorDescription { get; set; }

        public string GetString()
        {
            return $"{Error} - {ErrorDescription}";
        }
    }
}