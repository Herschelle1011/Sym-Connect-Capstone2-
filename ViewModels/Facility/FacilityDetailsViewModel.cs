using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Facility
{
    public class FacilityDetailsViewModel
    {
        public int FacilityId { get; set; }

        public string FacilityName { get; set; } = string.Empty;

        public string FacilityDescription { get; set; } = string.Empty;

        public List<string> FacilityType { get; set; } = new List<string>();

        public int Capacity { get; set; }

        public FacilityStatus Status { get; set; }
        public string StatusLabel { get; set; } = "";
        public string StatusClass { get; set; } = "";

        public List<ExistingPhotoViewModel> Photos { get; set; } = new();


        public List<FacilityReservationViewModel> UpcomingReservations { get; set; } = new List<FacilityReservationViewModel>();

    }
}
