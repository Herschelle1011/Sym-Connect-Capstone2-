using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class FormElectionViewModel
    {
        [Required(ErrorMessage = "Election Name is required")]
        public string ElectionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Starting Date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Close Date is required")]
        public DateTime EndDate { get; set; }


        [Required(ErrorMessage = "Starting Time is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Close Time is required")]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Please select at least one party-list for this election.")]
        public List<int> SelectedPartyListIds { get; set; } = new();

        // Populated via your service method inside the Controller's Get method
        public List<PartyListsViewModel> AvailablePartyLists { get; set; } = new();

    }
}
