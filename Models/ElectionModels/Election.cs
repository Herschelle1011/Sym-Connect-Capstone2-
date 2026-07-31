using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Models.ElectionModels
{
    public class Election
    {
        [Key]
        public int ElectionId { get; set; }

        public string ElectionName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        //status
        [NotMapped]
        public string Status =>
            DateTime.Now < StartDate ? "Incoming" :
            DateTime.Now <= EndDate ? "Open" : "Closed";

        public ICollection<PartyList> partyLists { get; set; } = new List<PartyList>();



        //ARCHIVED SECTION
        public bool? IsArchived { get; set; } = false;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ArchivedDate { get; set; }

    }
}
