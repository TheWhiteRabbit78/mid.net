using AbySalto.Mid.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace AbySalto.Mid.WebApi.Common
{
    /// <summary>
    /// Translates <see cref="ServiceResult"/> instances into appropriate <see cref="IActionResult"/> responses.
    /// Keeps controller actions thin and consistent.
    /// </summary>
    public static class ServiceResultExtensions
    {
        extension(ControllerBase controller)
        {
            /// <summary>
            /// Converts a <see cref="ServiceResult"/> into an <see cref="IActionResult"/>.
            /// On success returns 204 No Content. On failure delegates to <see cref="ControllerBase.Problem(string?, string?, int?, string?, string?)"/>.
            /// </summary>
            public IActionResult ToActionResult(ServiceResult result)
            {
                if (result.Success)
                {
                    return controller.NoContent();
                }

                return controller.Problem(statusCode: (int)result.Code, title: result.Message);
            }

            /// <summary>
            /// Converts a typed <see cref="ServiceResult{T}"/> into an <see cref="IActionResult"/>.
            /// On success returns the configured success status with the data payload.
            /// On failure delegates to <see cref="ControllerBase.Problem(string?, string?, int?, string?, string?)"/>.
            /// </summary>
            public IActionResult ToActionResult<T>(ServiceResult<T> result, int successStatusCode = StatusCodes.Status200OK)
            {
                if (result.Success)
                {
                    return controller.StatusCode(successStatusCode, result.Data);
                }

                return controller.Problem(statusCode: (int)result.Code, title: result.Message);
            }
        }
    }
}
