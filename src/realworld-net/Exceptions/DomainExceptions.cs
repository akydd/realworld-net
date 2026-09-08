namespace realworld_net.Exceptions;

public abstract class AppException : Exception
{
    public string Field { get; set; }
    public string Error { get; set; }

    protected AppException(string field, string error) : base(field + error)
    {
        Field = field;
        Error = error;
    }
}

public class NotFoundException(string field) : AppException(field, "not found");
public class UnauthorizedException(string field = "token", string error = "is missing") : AppException(field, error);
public class ForbiddenException(string field) : AppException(field, "forbidden");
public class ConflictException(string field) : AppException(field, "has already been taken");
