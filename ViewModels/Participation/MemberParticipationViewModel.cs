using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Participation
{
    public class MemberParticipationViewModel
    {
        public int MemberId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }

        public int? GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public int EventsAssigned { get; set; }

        public int EventsPresent { get; set; }

        public int EventsAbsent { get; set; }

        public double ParticipationRate { get; set; }

        public int TotalPoints { get; set; }

        public MemberParticipationStatus Status { get; set; }
    }
}
