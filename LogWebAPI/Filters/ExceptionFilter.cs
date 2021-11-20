using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;


namespace LogWebAPI.Filters
{
public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            try
            {
                throw context.Exception;
            }catch (Exception)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 404,
                    Content = ""
                };
            }
        }
    }
}