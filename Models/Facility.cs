using System.ComponentModel.DataAnnotations;
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Models
{
    public class Facility
    {
       
        [Key]
        public int FacilityId { get; set; }

        [Required]
        [StringLength(100)]
        public string FacilityName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FacilityDescription { get; set; } = string.Empty;

        [Required]
        public FacilityStatus Status { get; set; } = FacilityStatus.Active;

        public string Photos { get; set; } = string.Empty;

        [Required]
        public string FacilityTypes { get; set; } = string.Empty;

        public bool? IsArchived { get; set; } = false;
        public DateTime? ArchiveDate { get; set; }

        [Required]
        public int Capacity { get; set; }

        // Navigation Property
        public ICollection<FacilityPhoto> FacilityPhotos { get; set; }
            = new List<FacilityPhoto>();
    }
}
