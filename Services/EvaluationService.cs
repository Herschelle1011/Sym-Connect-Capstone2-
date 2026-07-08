using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.ViewModels.Evaluation;

namespace Sym_Connect__Capstone2_.Services
{
    public class EvaluationService : IEvaluationService
    {
        private readonly AppDbContext _context;
        public EvaluationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Evaluation>> GetAllEvaluationsAsync()
        {
            return await _context.Evaluations
                    .Include(d => d.EvaluationDetails)
                     .ThenInclude(q => q.EvaluationQuestion)
                      .Where(e => e.IsArchived == false)
                            .ToListAsync();
        }
        public async Task<List<EvaluationQuestion>> GetAllQuestionsAsync()
        {
            return await _context.EvaluationQuestions.ToListAsync();
        }

        public async Task<Evaluation?> GetEvaluationByIdAsync(int id)
        {
            return await _context.Evaluations
                .Include(d => d.EvaluationDetails)
                    .ThenInclude(q => q.EvaluationQuestion)
                        .FirstOrDefaultAsync(e => e.EvaluationId == id && e.IsArchived == false);
        }
        
        //------PROCEED TO THE OTHER METHODS NEEDED -------//
        // SEARCH FOR INCLUDE, .SELECT , ENTITY FRAMEWORKDS //
                
        public async Task CreateOrReuseQuestionAsync(int evaluationid, string questionText, AnswerType answertype)
        {   
            //first find/search question along with its answertype using first to get first question && answertype
            var question = await _context.EvaluationQuestions.FirstOrDefaultAsync(e => e.QuestionText == questionText && e.AnswerType == answertype);
            if (question == null) 
            {
                //if question doesnt exist yet create a new question 
                // REUSE the same variable after the search and fill it up if question if empty create New
                question = new EvaluationQuestion
                {
                    QuestionText = questionText,
                    AnswerType = answertype,             
                };
                _context.EvaluationQuestions.Add(question);
                await _context.SaveChangesAsync();
            }
            //Avoid duplicate remove BRIDGE if it is already linked like evaluation == questionId already 
            var AlreadyExist = await _context.EvaluationDetail.AnyAsync(e => e.EvaluationId == evaluationid && e.QuestionId == question.QuestionId);
            if (AlreadyExist) return; //IF true STOP !!

            // no search was invented here so create a New variable
            var evaluationDetail = new EvaluationDetails
                {
                    QuestionId = question.QuestionId,
                    EvaluationId = evaluationid,
                   
                };
                _context.EvaluationDetail.Add(evaluationDetail); //this continue to add bridges 
                await _context.SaveChangesAsync();
        }
        public async Task CreateEvaluationAsync(CreateEvaluationViewModel vm)
        {
            //follow this method and POLISH everything ------STUCK HERE RESEARCH 7/6/2026
            var evaluation = new Evaluation
            {
                EvaluationTitle = vm.EvaluationTitle,
                Description = vm.Description,
                OpenAt = vm.OpenAt,
                ClosedAt = vm.ClosedAt,
                DateCreated = DateTime.Now,
                IsArchived = false,
                ArchivedDate = null,
            };
            //add evaluation
            _context.Evaluations.Add(evaluation);
            await _context.SaveChangesAsync();
            //add question to evaluationdetails
            foreach(var question in vm.Questions)
            {
                await CreateOrReuseQuestionAsync(
                    evaluation.EvaluationId,
                    question.QuestionText,
                    question.AnswerType
                    );
            }      
        }

        public async Task<UpdateEvaluationViewModel?> GetUpdateEvaluationAsync(int id)
        {
            var evaluation = await _context.Evaluations.
                Include(ed => ed.EvaluationDetails)
                .ThenInclude(q => q.EvaluationQuestion).
                FirstOrDefaultAsync(e => e.EvaluationId == id);

            if (evaluation == null) return null;
            return new UpdateEvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                EvaluationTitle = evaluation.EvaluationTitle,
                Description = evaluation.Description,
                ClosedAt = evaluation.ClosedAt,
                OpenAt = evaluation.OpenAt,
                Questions = evaluation.EvaluationDetails.Select(e => new QuestionRowViewModel
                {
                    QuestionId = e.QuestionId,
                    QuestionText = e.EvaluationQuestion!.QuestionText,
                    AnswerType = e.EvaluationQuestion.AnswerType,           
                }).ToList()
            };

        }


        public async Task UpdateEvaluationAsync(UpdateEvaluationViewModel vm)
        {
            var Exist = await _context.Evaluations.FindAsync(vm.EvaluationId);
            if (Exist == null) return;

            Exist.EvaluationTitle = vm.EvaluationTitle;
            Exist.Description = vm.Description;
            Exist.OpenAt = vm.OpenAt;
            Exist.ClosedAt = vm.ClosedAt;
            Exist.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            //Remove the Bridge which is EvaluationDetail where evaluationid == evaluationId(EditedbyUser) then convert to LIST();
            var ExistingQuestions = _context.EvaluationDetail.Where(e => e.EvaluationId == vm.EvaluationId).ToList();
            _context.EvaluationDetail.RemoveRange(ExistingQuestions);
          
            await _context.SaveChangesAsync();

            foreach (var question in vm.Questions)
            {
                await CreateOrReuseQuestionAsync(
                    vm.EvaluationId,
                    question.QuestionText,
                    question.AnswerType
                   );
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEvaluationAsync(int id)
        {
            var getEvaluation = await _context.Evaluations.FindAsync(id);
            if (getEvaluation == null) return;
            _context.Evaluations.Remove(getEvaluation);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EvaluationExist(int id)
        {
            return await _context.Evaluations.AnyAsync(e => e.EvaluationId == id);
        }

        public async Task<ArchiveEvaluationViewModel?> GetArchiveEvaluationConfirmation(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null) return null;

            return new ArchiveEvaluationViewModel
            {
                EvaluationId = evaluation.EvaluationId,
                EvaluationTitle = evaluation.EvaluationTitle,
                Description = evaluation.Description,
            };     
        }
        public async Task ArchiveEvaluation(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null) return;
            evaluation.IsArchived = true;
            evaluation.ArchivedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
