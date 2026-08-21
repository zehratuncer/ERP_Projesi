namespace ERP.Application.Common.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"'{name}' ({key}) bulunamadı.")
    {
    }
}
