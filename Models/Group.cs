using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [Required]
        public string GroupName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";

        public int? LeaderId { get; set; }

        public bool IsArchived { get; set; } = false;

        public DateTime? ArchivedDate { get; set; }

        //NAVIGATION PROPERTIES
        public Member? Leader { get; set; }
        public ICollection<GroupDetails> GroupDetails { get; set; } = new List<GroupDetails>();



    }
}
