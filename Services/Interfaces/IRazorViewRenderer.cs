namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IRazorViewRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(
        string viewName,
        TModel model);
    }
}
