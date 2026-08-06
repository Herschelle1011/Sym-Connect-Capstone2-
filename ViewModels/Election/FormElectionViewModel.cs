using Sym_Connect__Capstone2_.ViewModels.PartyList;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class FormElectionViewModel 
    {
        [Required(ErrorMessage = "Election name is required.")]
        [StringLength(150)]
        public string ElectionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Opening date is required.")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Closing date is required.")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public List<PartyListsViewModel> AvailablePartyLists { get; set; } = new();


        public List<int> SelectedPartyListIds { get; set; } = new();

        public List<PartyListAssignmentsViewModel> PartyListAssignments { get; set; } = new();


    }
}
