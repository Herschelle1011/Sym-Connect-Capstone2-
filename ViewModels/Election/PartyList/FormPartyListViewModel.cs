using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Election.PartyList
{
    public class FormPartyListViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string PartyListName { get; set; } = string.Empty;
        public string PartyListDescription { get; set; } = string.Empty;
   

    }
}
