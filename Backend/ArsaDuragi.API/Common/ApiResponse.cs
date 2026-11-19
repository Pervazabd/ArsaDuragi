using System.Text.Json;

namespace ArsaDuragi.API.Common;

/// <summary>The one JSON envelope every endpoint returns: { success, message, data, errors }.</summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string message) => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors?.ToList() ?? new List<string>() };
}

public static class ApiResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Writes an error envelope from places that are outside MVC (middleware, JWT events).</summary>
    public static Task WriteAsync(HttpResponse response, int statusCode, string message, IEnumerable<string>? errors = null)
    {
        if (response.HasStarted) return Task.CompletedTask;

        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        return response.WriteAsync(JsonSerializer.Serialize(ApiResponse<object?>.Fail(message, errors), JsonOptions));
    }
}
