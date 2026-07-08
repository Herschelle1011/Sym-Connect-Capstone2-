using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Facility
{
    public class FacilityDetailsViewModel
    {
        public int FacilityId { get; set; }

        public string FacilityName { get; set; } = string.Empty;

        public string FacilityDescription { get; set; } = string.Empty;

        public string FacilityType { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public FacilityStatus Status { get; set; }

        public List<string> Photos { get; set; } = new();
    }
}
