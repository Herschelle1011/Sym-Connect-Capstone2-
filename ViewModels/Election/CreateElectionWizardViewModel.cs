using Sym_Connect__Capstone2_.ViewModels.Election.PartyList;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class CreateElectionWizardViewModel : IValidatableObject 
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

        // ---- Step 2 ----
        // Rendered on GET so the view can show clickable cards; not posted back.
        public List<PartyListsViewModel> AvailablePartyLists { get; set; } = new();

        // Posted back from Step 2 (checkbox/click selection, exactly 2)
        public List<int> SelectedPartyListIds { get; set; } = new();

        // ---- Step 3 ----
        // Built dynamically by JS to match SelectedPartyListIds (2 entries, 3 candidate slots each)
        public List<PartyListAssignmentsViewModel> PartyListAssignments { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate < StartDate)
                yield return new ValidationResult("Closing date must be on or after the opening date.", new[] { nameof(EndDate) });

            if (SelectedPartyListIds == null || SelectedPartyListIds.Count != 2)
                yield return new ValidationResult("Select exactly 2 party lists.", new[] { nameof(SelectedPartyListIds) });
        }

    }
}
