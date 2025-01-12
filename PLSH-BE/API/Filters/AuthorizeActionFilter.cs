using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BU.Services.Implementation;
using BU.Services.Interface;
using Data.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace API.Filters
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _components;
        public IUnitOfWork UnitOfWork { get; private set; }

        public AuthorizeAttribute(string[] components)
        {
            _components = components;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var svc = context.HttpContext.RequestServices;
                var commonService = (CommonService)svc.GetService(typeof(ICommonService));

                
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                context.Result = new UnprocessableEntityResult();
            }
        }

        
    }
}
