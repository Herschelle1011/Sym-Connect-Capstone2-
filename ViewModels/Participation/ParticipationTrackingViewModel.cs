namespace Sym_Connect__Capstone2_.ViewModels.Participation
{
    public class ParticipationTrackingViewModel
    {
        public ParticipationSummaryViewModel Summary { get; set; } 
            = new ParticipationSummaryViewModel();

        public List<GroupParticipationViewModel> Groups { get; set; }
            = new List<GroupParticipationViewModel>();

        public List<MemberParticipationViewModel> Members { get; set; }
            = new List<MemberParticipationViewModel>();

    }
}
