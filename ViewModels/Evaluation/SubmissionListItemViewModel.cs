using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class SubmissionListItemViewModel
    {

        public int SubmissionId { get; set; }
        public string MemberName { get; set; } = "";
        public DateTime? SubmittedAt { get; set; }
        public SubmissionStatus Status { get; set; }
    }
}
