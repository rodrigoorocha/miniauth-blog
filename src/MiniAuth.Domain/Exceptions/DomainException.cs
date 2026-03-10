namespace MiniAuth.Domain.Exceptions;

public class DomainException : Exception
{
    public List<string> Errors { get; }

    public DomainException(string message) : base(message)
    {
        Errors = new List<string> { message };
    }

    public DomainException(List<string> errors) : base(errors.First())
    {
        Errors = errors;
    }
}
