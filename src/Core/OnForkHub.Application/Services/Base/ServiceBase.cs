namespace OnForkHub.Application.Services.Base;

public abstract class ServiceBase
{
#pragma warning disable CA1051

    protected readonly IValidationService _validationService;

#pragma warning restore CA1051

    protected ServiceBase(IValidationService validationService)
    {
        _validationService = validationService;
    }

    public static object ValidateEntity<T>(object value, Func<T, CustomValidationResult> validateTestEntity)
    {
        throw new NotImplementedException();
    }

    protected static async Task<RequestResult<T>> ExecuteAsync<T>(Func<Task<RequestResult<T>>> operacao)
    {
        try
        {
            return await operacao();
        }
        catch (CustomResultException customResultException)
        {
            return RequestResult<T>.WithError(customResultException.CustomResult.Message);
        }
        catch (Exception exception)
        {
            return RequestResult<T>.WithError($"Error processing operation: {exception.Message}");
        }
    }

    protected static async Task<RequestResult<T>> ExecuteAsync<T>(
        T entity,
        Func<T, Task<RequestResult<T>>> operacao,
        Func<T, CustomValidationResult> validationFunc
    )
        where T : class
    {
        var validationResult = ValidateEntity(entity, validationFunc);
        if (!validationResult.IsValid)
        {
            var result = new RequestResult<T>();
            foreach (var error in validationResult.Errors)
            {
                result.Status = EResultStatus.EntityHasError;
                result.AddError(error.Message);
            }

            return result;
        }

        return await ExecuteAsync(() => operacao(entity));
    }

    protected static CustomValidationResult ValidateEntity<T>(T entity, Func<T, CustomValidationResult> validationFunc)
        where T : class
    {
        var validationResult = new CustomValidationResult();

        if (entity == null)
        {
            validationResult.AddError("Entity cannot be null");
            return validationResult;
        }

        return validationFunc(entity);
    }
}
