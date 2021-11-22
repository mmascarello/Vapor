using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace AdministratorWebApi.Filters
{
public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            try
            {
                throw context.Exception;
            }
            catch (ArgumentException e)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 400,
                    Content = e.Message
                };
            }catch (Exception)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 500,
                    Content = "Internal server error"
                };
            }
        }
    }
}