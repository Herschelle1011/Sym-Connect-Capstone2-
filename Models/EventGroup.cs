using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models
{
    public class EventGroup
    {
        [Key]
        public int EventGroupId { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;
    }
}
