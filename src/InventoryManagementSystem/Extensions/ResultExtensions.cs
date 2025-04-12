using System;
using System.Threading.Tasks;
using InventoryManagementSystem.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Extensions
{
    /// <summary>
    /// Extension methods for the Result<T> class
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Maps the value of a Result<T> to a Result<TOut> using the provided mapping function
        /// </summary>
        /// <typeparam name="TIn">The input type</typeparam>
        /// <typeparam name="TOut">The output type</typeparam>
        /// <param name="result">The source result</param>
        /// <param name="mapper">The mapping function to apply to the value</param>
        /// <returns>A new Result<TOut> with the mapped value (if successful) or the original error details</returns>
        public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
        {
            if (result.IsSuccess && result.Value != null)
            {
                // Map the value and create a new success result
                try
                {
                    var mappedValue = mapper(result.Value);
                    return Result<TOut>.Success(mappedValue, result.Message);
                }
                catch (Exception ex)
                {
                    // If mapping fails, return a failure result
                    return Result<TOut>.Failure($"Error mapping result: {ex.Message}");
                }
            }
            else
            {
                // Preserve the error details from the original result
                return Result<TOut>.Failure(
                    message: result.Message,
                    errors: result.Errors,
                    statusCode: result.StatusCode
                );
            }
        }
        
        /// <summary>
        /// Converts a Result<T> to an appropriate ActionResult<T> based on the result status
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="result">The result to convert</param>
        /// <param name="controller">The controller instance (used for generating URLs)</param>
        /// <returns>An appropriate ActionResult based on the result status</returns>
        public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            if (result.IsSuccess)
            {
                return controller.Ok(result.Value);
            }
            
            return result.StatusCode switch
            {
                400 => controller.BadRequest(new { message = result.Message, errors = result.Errors }),
                401 => controller.Unauthorized(new { message = result.Message }),
                403 => controller.Forbid(),
                404 => controller.NotFound(new { message = result.Message }),
                409 => controller.Conflict(new { message = result.Message, errors = result.Errors }),
                _ => controller.StatusCode(result.StatusCode, new { message = result.Message, errors = result.Errors })
            };
        }
        
        /// <summary>
        /// Converts a Result<T> to an appropriate IActionResult based on the result status
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="result">The result to convert</param>
        /// <param name="controller">The controller instance (used for generating URLs)</param>
        /// <returns>An appropriate IActionResult based on the result status</returns>
        public static IActionResult ToIActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            if (result.IsSuccess)
            {
                return controller.Ok(result.Value);
            }
            
            return result.StatusCode switch
            {
                400 => controller.BadRequest(new { message = result.Message, errors = result.Errors }),
                401 => controller.Unauthorized(new { message = result.Message }),
                403 => controller.Forbid(),
                404 => controller.NotFound(new { message = result.Message }),
                409 => controller.Conflict(new { message = result.Message, errors = result.Errors }),
                _ => controller.StatusCode(result.StatusCode, new { message = result.Message, errors = result.Errors })
            };
        }
    }
} 