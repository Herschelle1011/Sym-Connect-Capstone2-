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
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        //scanned by
        public string? ScannedByUserId { get; set; }
        public User? ScannedByUser { get; set; }

        //date
        public DateTime DateScanned { get; set; }
    }
}
