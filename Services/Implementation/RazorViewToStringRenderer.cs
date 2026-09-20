using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            await using var sw = new StringWriter();

            var viewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);

            if (!viewResult.Success)
            {
                // Fallback: try resolving by full relative path (e.g. "/Views/Emails/PasswordResetEmail.cshtml")
                viewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            }

            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"Couldn't find view '{viewName}'. Searched locations: {string.Join(", ", viewResult.SearchedLocations)}");
            }

            var viewDictionary = new ViewDataDictionary<TModel>(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            return sw.ToString();
        }
    }
}

