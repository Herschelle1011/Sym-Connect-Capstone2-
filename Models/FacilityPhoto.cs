using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models
{
    public class FacilityPhoto
    {
        [Key]
        public int FacilityPhotoId { get; set; }

        public string PhotoUrl { get; set; } = string.Empty;

        public int FacilityId { get; set; }

        //navigation 
        public Facility? Facility { get; set; }

    }
}
