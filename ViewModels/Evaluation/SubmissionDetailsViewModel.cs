using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class SubmissionDetailsViewModel
    {
        public DateTime? SubmittedAt { get; set; }
        public List<AnswerReviewItem> Answers { get; set; } = new();


    }
}
