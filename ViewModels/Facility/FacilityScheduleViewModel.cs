namespace Sym_Connect__Capstone2_.ViewModels.Facility
{
    public class FacilityScheduleViewModel
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public List<FacilityScheduleEventViewModel> Events { get; set; } = new();
    }
}
