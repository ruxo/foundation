using FluentValidation;
using JetBrains.Annotations;

namespace RZ.Foundation.Blazor.Helpers;

[PublicAPI]
public static class Validator
{
    /// <summary>
    /// A helper function to convert a FluentValidation validator to a MudBlazor validator style. This will be moved to a specific library
    /// for MudBlazor in the future.
    /// </summary>
    /// <param name="validator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Func<object, string, Task<IEnumerable<string>>> ValueValidator<T>(this AbstractValidator<T> validator) =>
        async (model, propertyName) => {
            var result = await validator.ValidateAsync(
                             ValidationContext<T>.CreateWithOptions((T)model, x => x.IncludeProperties(propertyName)));
            return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
        };
}