using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace OrderProcessing.API.Controllers;

internal static class ValidationExtensions
{
    internal static ValidationProblemDetails ToValidationProblemDetails(this ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };
    }
}
