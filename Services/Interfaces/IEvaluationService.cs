using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.ViewModels.Evaluation;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IEvaluationService
    {
        Task<List<Evaluation>> GetAllEvaluationsAsync();
        Task<Evaluation?> GetEvaluationByIdAsync(int id);
        //evaluation from a specific member id STOP HERE SEARCH MORE --------
        Task CreateEvaluationAsync(CreateEvaluationViewModel vm);
        Task<EditQuestionViewModel> GetQuestionAsync(int id);
        Task CreateOrReuseQuestionAsync(int evaluationid, string questionText, AnswerType answertype);

        Task UpdateEvaluationAsync(UpdateEvaluationViewModel vm);
        Task<UpdateEvaluationViewModel?> GetUpdateEvaluationAsync(int id);
        Task DeleteEvaluationAsync(int id);
        Task<ArchiveEvaluationViewModel?> GetArchiveEvaluationConfirmation(int id);
        Task <List<EvaluationQuestion>> GetAllQuestionsAsync();
        Task ArchiveEvaluation(int id);
        Task<bool> EvaluationExist(int id);

        Task EditQuestionAsync(EditQuestionViewModel vm);

        Task<List<SubmissionListItemViewModel>> GetSubmissionsForEvaluationAsync(int evaluationId);
        Task<SubmissionDetailsViewModel?> GetSubmissionDetailsAsync(int submissionId);
    }
}
