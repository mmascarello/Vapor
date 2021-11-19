﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AdministratorWebApi.Filters
{
    [ExcludeFromCodeCoverage]
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