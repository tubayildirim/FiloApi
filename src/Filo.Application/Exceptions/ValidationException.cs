namespace Filo.Application.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors) : base("Bir veya birden fazla doğrulama hatası oluştu.")
    {
        Errors = errors;
    }

    public ValidationException(string error) : base("Doğrulama hatası oluştu.")
    {
        Errors = new List<string> { error };
    }
}
