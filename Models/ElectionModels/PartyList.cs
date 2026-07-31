using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Sym_Connect__Capstone2_.Models.ElectionModels
{
    public class PartyList
    {
        [Key]
        public int PartyListId { get; set; }

        public string PartyName { get; set; } = string.Empty;

        public string PartyListDescription { get; set; } = string.Empty;
      
        public int? ElectionId { get; set; }


        public bool? isArchived { get; set; } = false;
        public DateTime? ArchivedDate { get; set; }


        //NAVIGATION PROPERTIES
        [ForeignKey(nameof(ElectionId))]
        public Election? Election { get; set; }
        public ICollection<Candidates> candidates { get; set; } = new List<Candidates>();
    }
}
