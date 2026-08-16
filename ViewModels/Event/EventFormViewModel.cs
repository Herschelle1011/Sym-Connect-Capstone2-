using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Event
{
    public class EventFormViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        public string EventTitle { get; set; } = string.Empty;


        [Required(ErrorMessage = "Description is required")]
        public string EventDescription { get; set; } = string.Empty;
        public bool IsAllDay { get; set; }
        [Required(ErrorMessage = "Start Date is required")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "End Date is required")]
        public DateTime? EndDate { get; set; }

        [Range(1, 100, ErrorMessage = "Must be below or equal to 100")]
        public int Points { get; set; } = 10;

        //time if its not ALLDAY
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        [Required(ErrorMessage = "Select facility for this event")]

        public List<int> SelectedGroupIds { get; set; } = new List<int>();

        public List<int> SelectedFacilityIds { get; set; } = new List<int>();
        
        [Required(ErrorMessage = "Open Time is required")]
        public TimeSpan? TimeInScannerOpenTime { get; set; }

        [Required(ErrorMessage = "Close Time is required")]
        public TimeSpan? TimeOutScannerOpenTime { get; set; }
    }


}
