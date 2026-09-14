using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Evaluation;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Evaluation;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IEvaluationService
    {
        Task<List<MemberEvaluationListItemViewModel>> GetAvailableEvaluationsAsync(int currentMemberId, bool isAdmin);        Task<Evaluation?> GetEvaluationByIdAsync(int id);
        Task <OperationalResult>CreateEvaluationAsync(CreateEvaluationViewModel vm, int currentMemberId, bool isAdmin);
        Task<EditQuestionViewModel> GetQuestionAsync(int id);
        Task CreateOrReuseQuestionAsync(int evaluationid, string questionText, AnswerType answertype);

        Task<OperationalResult> UpdateEvaluationAsync(UpdateEvaluationViewModel vm, int currentMemberId,
                                                                    bool isAdmin);

        Task<UpdateEvaluationViewModel?> GetUpdateEvaluationAsync(
               int id,
               int currentMemberId,
               bool isAdmin);
        Task<OperationalResult> DeleteEvaluationAsync(int id);
        Task<ArchiveEvaluationViewModel?> GetArchiveEvaluationConfirmation(int id, int currentMemberId, bool isAdmin);

        Task<List<EvaluationQuestion>> GetAllQuestionsAsync();
        Task<OperationalResult> ArchiveEvaluationAsync(int id,int currentMemberId,bool isAdmin);
            Task<bool> EvaluationExist(int id);

        Task EditQuestionAsync(EditQuestionViewModel vm);

        Task<List<SubmissionListItemViewModel>> GetSubmissionsForEvaluationAsync(int evaluationId);
        Task<SubmissionDetailsViewModel?> GetSubmissionDetailsAsync(int submissionId);

        Task<SummaryResponseReportViewModel> summaryReport(int? evaluationId);

        Task<string?> GetCurrentStatusAsync(int evaluationId);
        Task<List<ReusableQuestionViewModel>> GetFrequentlyUsedQuestionsAsync();

        Task<double?> GetEventFinalRatingAsync(int eventId);

        Task<int> GetEventResponseCountAsync(int eventId);

        Task<DetailsEvaluationViewModel?> GetEvaluationByIdAsync(
    int evaluationId,
    int currentMemberId,
    bool isAdmin);
    }
}