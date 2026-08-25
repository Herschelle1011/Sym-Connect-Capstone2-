using Sym_Connect__Capstone2_.Models.EventModels;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        //fk event
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        //fk members
        public int? MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }

        public string Status { get; set; } = string.Empty;

        public int Points { get; set; }

        //scanned by
        public string? ScannedByUserId { get; set; }
        public User? ScannedByUser { get; set; }

   
    }
}
