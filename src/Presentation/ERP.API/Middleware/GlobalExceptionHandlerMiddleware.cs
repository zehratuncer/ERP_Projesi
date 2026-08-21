using System.Net;
using System.Text.Json;
using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Models;
using FluentValidation;

namespace ERP.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Beklenmeyen bir hata oluştu: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationEx => CreateValidationResponse(context, validationEx),
            NotFoundException notFoundEx => CreateResponse(context, HttpStatusCode.NotFound, notFoundEx.Message),
            BusinessException businessEx => CreateResponse(context, HttpStatusCode.BadRequest, businessEx.Message),
            _ => CreateResponse(context, HttpStatusCode.InternalServerError, "Sunucu tarafında beklenmeyen bir hata meydana geldi.")
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private static ApiResponse<object> CreateResponse(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        return ApiResponse<object>.Failure(message);
    }

    private static ApiResponse<object> CreateValidationResponse(HttpContext context, ValidationException validationEx)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        var errors = validationEx.Errors.Select(e => e.ErrorMessage).ToList();
        return ApiResponse<object>.Failure("Doğrulama hatası oluştu.", errors);
    }
}
