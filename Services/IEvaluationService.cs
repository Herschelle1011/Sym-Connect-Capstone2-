using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Services
{
    public interface IEvaluationService
    {
        Task<List<Evaluation>> GetAllEvaluationsAsync();
        Task<Evaluation?> GetEvaluationByIdAsync(int id);
        //evaluation from a specific member id STOP HERE SEARCH MORE --------
        Task CreateEvaluationAsync(Evaluation evaluation);
        Task UpdateEvaluationAsync(Evaluation evaluation);
        Task DeleteEvaluationAsync(int id);
        Task ArchiveEvaluationAsync(int id);

        Task<bool> EvaluationExist(int id);

    }
}
