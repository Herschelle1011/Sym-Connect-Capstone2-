using Sym_Connect__Capstone2_.Models.FacilityModels;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models.EventModels
{
    public class EventFacility
    {
        [Key]
        public int EventId { get; set; }
        public Event Event { get; set; }

        public int FacilityId { get; set; }
        public Facility Facility { get; set; }
    }
}
