namespace Sym_Connect__Capstone2_.ViewModels.Evaluation
{
    public class SummaryResponseReportViewModel
    {
        // Evaluation Information
        public int EvaluationId { get; set; }
        public string EvaluationTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Cards
        public int TotalResponses { get; set; }
        public double CompletionRate { get; set; }
        public double AverageRating { get; set; }
        public string Status { get; set; } = string.Empty;

        // Questions
        public List<QuestionSummaryViewModel> Questions { get; set; } = new();
    }

    public class QuestionSummaryViewModel
    {
        public int EvaluationDetailId { get; set; }
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = string.Empty;

        public double AverageRating { get; set; }

        public int TotalResponses =>
            VeryExcellent +
            Excellent +
            Neutral +
            Poor +
            VeryPoor;

        // Counts
        public int VeryExcellent { get; set; }
        public int Excellent { get; set; }
        public int Neutral { get; set; }
        public int Poor { get; set; }
        public int VeryPoor { get; set; }

        // Percentages (for progress bars)
        public double VeryExcellentPercent =>
            TotalResponses == 0 ? 0 : (double)VeryExcellent / TotalResponses * 100;

        public double ExcellentPercent =>
            TotalResponses == 0 ? 0 : (double)Excellent / TotalResponses * 100;

        public double NeutralPercent =>
            TotalResponses == 0 ? 0 : (double)Neutral / TotalResponses * 100;

        public double PoorPercent =>
            TotalResponses == 0 ? 0 : (double)Poor / TotalResponses * 100;

        public double VeryPoorPercent =>
            TotalResponses == 0 ? 0 : (double)VeryPoor / TotalResponses * 100;

    }
}
