using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models
{
    public class Facility
    {
        public enum FacilityStatus
        {
            Active,
            Inactive,
            UnderMaintenance
        }

        [Key]
        public int FacilityId { get; set; }

        [Required]
        [StringLength(100)]
        public string FacilityName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? FacilityDescription { get; set; }

        [Required]
        public FacilityStatus Status { get; set; } = FacilityStatus.Active;

        public ICollection<YouthActivity> Activities { get; set; } = new List<YouthActivity>();
    }
}
