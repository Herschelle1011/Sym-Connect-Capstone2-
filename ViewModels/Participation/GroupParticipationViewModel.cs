using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Participation
{
    public class GroupParticipationViewModel
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public int TotalMembers { get; set; }

        public int EventsAssigned { get; set; }

        public int FullyPresentEvents { get; set; }

        public int PartiallyPresentEvents { get; set; }

        public int AbsentEvents { get; set; }

        public double ParticipationRate { get; set; }

        public int EventsAssignedCount { get; set; } // events assigned
        public int EventsAttendedCount { get; set; } // how many events did this group attended? count

        public int TotalPossiblePoints { get; set; }   // sum of Points across all assigned events
        public int EarnedPoints { get; set; }          // sum of Points for events actually attended




        public GroupAttendanceStatus Status { get; set; }

        public int AttendedEvents => FullyPresentEvents + PartiallyPresentEvents;

    }
}
