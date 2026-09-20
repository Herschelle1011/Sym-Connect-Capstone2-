namespace Sym_Connect__Capstone2_.ViewModels.Facility
{
    public class ArchivedFacilityDetailsViewModel
    {
        public int FacilityId { get; set; }

        public string FacilityName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Capacity { get; set; }

        public string Status { get; set; } = string.Empty;

        public List<string> FacilityTypes { get; set; } = new();

        public List<string> PhotoUrls { get; set; } = new();

        public DateTime? ArchiveDate { get; set; }

        public List<string> FacilityPhotos { get; set; } = new();

    }
}
