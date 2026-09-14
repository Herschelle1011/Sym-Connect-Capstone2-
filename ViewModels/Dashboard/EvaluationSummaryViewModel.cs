namespace Sym_Connect__Capstone2_.ViewModels.Dashboard
{
    public class EvaluationSummaryViewModel
    {
        public int FiveStar { get; set; }
        public int FourStar { get; set; }
        public int ThreeStar { get; set; }
        public int TwoStar { get; set; }
        public int OneStar { get; set; }
        public int TotalResponses => FiveStar + FourStar + ThreeStar + TwoStar + OneStar;

        public int FiveStarPercent => TotalResponses == 0 ? 0 : (int)Math.Round(FiveStar * 100.0 / TotalResponses);
        public int FourStarPercent => TotalResponses == 0 ? 0 : (int)Math.Round(FourStar * 100.0 / TotalResponses);
        public int ThreeStarPercent => TotalResponses == 0 ? 0 : (int)Math.Round(ThreeStar * 100.0 / TotalResponses);
        public int TwoStarPercent => TotalResponses == 0 ? 0 : (int)Math.Round(TwoStar * 100.0 / TotalResponses);
        public int OneStarPercent => TotalResponses == 0 ? 0 : (int)Math.Round(OneStar * 100.0 / TotalResponses);
    }
}
