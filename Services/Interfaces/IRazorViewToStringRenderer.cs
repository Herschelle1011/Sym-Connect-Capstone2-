namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);

    }
}
