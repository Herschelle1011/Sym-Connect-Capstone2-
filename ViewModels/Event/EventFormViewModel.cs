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


              [Required(ErrorMessage = "Please select a start date.")]
              public DateTime? StartDate { get; set; }

              [Required(ErrorMessage = "Please select an end date.")]
               public DateTime? EndDate { get; set; }


        [Range(1, 100, ErrorMessage = "Points must be between 1 and 100")]
            public int Points { get; set; } = 10;


            public TimeSpan? StartTime { get; set; }

            public TimeSpan? EndTime { get; set; }


            public List<int> SelectedGroupIds { get; set; } = new();

            public List<int> SelectedFacilityIds { get; set; } = new();


            // Scanner modal fills these later
            public TimeSpan? TimeInScannerOpenTime { get; set; }
           public TimeSpan? TimeInScannerClosesTime { get; set; }

            public TimeSpan? TimeOutScannerOpenTime { get; set; }
            public TimeSpan? TimeOutScannerClosesTime { get; set; }

    }
}



