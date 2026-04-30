using System.Net;

namespace AbySalto.Mid.Application.Common.Models
{
    /// <summary>
    /// Generic service result for operations.
    /// </summary>
    public class ServiceResult
    {
        public bool Success { get; set; }
        public HttpStatusCode Code { get; set; } = HttpStatusCode.BadRequest;
        public Exception? Exception { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ServiceResult Ok(string? message = null) => new() { Success = true, Code = HttpStatusCode.OK, Message = message };

        public static ServiceResult Fail(HttpStatusCode code) => new() { Success = false, Code = code };

        public static ServiceResult Fail(HttpStatusCode code, string? message) => new() { Success = false, Code = code, Message = message };

        public static ServiceResult Fail(HttpStatusCode code, Exception exception) => new() { Success = false, Code = code, Exception = exception, Message = exception.Message };

        public static ServiceResult Fail(List<string> errors) => new() { Success = false, Errors = errors };
    }

    /// <summary>
    /// Service result carrying typed data.
    /// </summary>
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string? message = null) => new() { Success = true, Code = HttpStatusCode.OK, Data = data, Message = message };

        public new static ServiceResult<T> Fail(HttpStatusCode code) => new() { Success = false, Code = code };

        public new static ServiceResult<T> Fail(HttpStatusCode code, string? message) => new() { Success = false, Code = code, Message = message };

        public new static ServiceResult<T> Fail(HttpStatusCode code, Exception exception) => new() { Success = false, Code = code, Exception = exception, Message = exception.Message };
    }
}
