using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class SubmissionListItemViewModel
    {

        public int SubmissionId { get; set; }
        public string Email { get; set; } = "";

        public string MaskedEmail
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@'))
                    return Email ?? string.Empty;

                var parts = Email.Split('@');
                var localPart = parts[0];
                var domain = parts[1];

                // Show up to the first 3 characters, mask the rest with fixed asterisks
                var visible = localPart.Length <= 3 ? localPart : localPart.Substring(0, 3);

                return $"{visible}****@{domain}";
            }
        }

        public DateTime? SubmittedAt { get; set; }
        public SubmissionStatus Status { get; set; }
    }
}
