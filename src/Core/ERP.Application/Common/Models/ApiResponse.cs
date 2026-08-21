namespace ERP.Application.Common.Models;

/// <summary>
/// Tüm API isteklerine standart formatta yanıt dönmek için kullanılan sarmalayıcı (envelope) model.
/// </summary>
/// <typeparam name="T">Dönecek verinin tipi</typeparam>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Success(T data, string message = "İşlem başarıyla tamamlandı.")
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> Failure(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> Failure(string message, string error)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            Message = message,
            Data = default,
            Errors = new List<string> { error }
        };
    }
}
