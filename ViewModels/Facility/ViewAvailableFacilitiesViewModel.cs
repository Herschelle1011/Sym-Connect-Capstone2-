using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Facility
{
    public class ViewAvailableFacilitiesViewModel
    {
        public int FacilityId { get; set; }

        public string FacilityName { get; set; } = string.Empty;

        public string FacilityDescription { get; set; } = string.Empty;

        public List<string> FacilityType { get; set; } = new List<string>();

        public int Capacity { get; set; }

        public List<(DateTime Date, string Title)> UpcomingReservations { get; set; } = new(); public int ReservationCountThisMonth { get; set; }

        public string Status { get; set; } = string.Empty;

        public List<string> Photos { get; set; } = new();

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
