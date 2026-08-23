using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Sym_Connect__Capstone2_.Services.Interfaces;


namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class RazorViewRenderer : IRazorViewRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewRenderer( IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }


        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext
                {
                    RequestServices = _serviceProvider
                },
                new RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            );

            using var writer = new StringWriter();

            var viewPath = $"/Views/{viewName}.cshtml";

            var viewResult = _viewEngine.GetView(
                executingFilePath: null,
                viewPath: viewPath,
                isMainPage: false
            );

            if (!viewResult.Success)
            {
                throw new InvalidOperationException(
                    $"View '{viewPath}' was not found."
                );
            }


            var viewData = new ViewDataDictionary<TModel>(
               new EmptyModelMetadataProvider(),
               new ModelStateDictionary()
           )
            {
                Model = model
            };

            var tempData = new TempDataDictionary(
                actionContext.HttpContext,
                _tempDataProvider
            );

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewData,
                tempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);

            return writer.ToString();
        }
    }
}
