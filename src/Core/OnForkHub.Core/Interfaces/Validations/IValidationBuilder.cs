namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationBuilder<T>
{
    IValidationBuilder<T> WithField(string fieldName, object? value = null);
    IValidationBuilder<T> NotNull(string? message = null);
    IValidationBuilder<T> NotEmpty(string? message = null);
    IValidationBuilder<T> NotWhiteSpace(string? message = null);
    IValidationBuilder<T> MinLength(int length, string? message = null);
    IValidationBuilder<T> MaxLength(int length, string? message = null);
    IValidationBuilder<T> Length(int exactLength, string? message = null);
    IValidationBuilder<T> Range<TRange>(TRange min, TRange max, string? message = null)
        where TRange : IComparable<TRange>;
    IValidationBuilder<T> Matches(string pattern, string? message = null);
    IValidationBuilder<T> Custom(Func<object?, bool> validation, string message);
    Task<IValidationBuilder<T>> CustomAsync(Func<object?, Task<bool>> validation, string message);
    IValidationBuilder<T> WithMetadata<TMetadata>(string key, TMetadata value);
    IValidationBuilder<T> Ensure(Func<bool> predicate, string message);
    Task<IValidationBuilder<T>> EnsureAsync(Func<Task<bool>> predicate, string message);
    IValidationResult Validate();
}