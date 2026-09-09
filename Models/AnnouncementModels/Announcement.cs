using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sym_Connect__Capstone2_.Models.Enums.AnnouncementEnums;


namespace Sym_Connect__Capstone2_.Models.AnnouncementModels
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public TargetAudience TargetAudience { get; set; }


        //==== SPECIFIC GROUP PART === 
        [ForeignKey(nameof(Group))]
        public int? GroupId { get; set; }
        public Group Group { get; set; }

        //==== PUBLISH PART === 
        public PublishOption PublishOption { get; set; }
        public DateTime? PublishDate { get; set; }

        //==== EXPIRES AT === 
        public ExpirationOption ExpirationOption { get; set; }
        public DateTime? ExpirationDate { get; set; }
        
        public DateTime CreatedDate { get; set; }

        public bool IsPinned { get; set; } = false;



        //=== AUTH MIGRATION SWAP TO AspNetUsersId once Path A bridge exists
        [ForeignKey(nameof(User))]
        public string? CreatedByUserId { get; set; }

        public User? User { get; set; }

        //==== Archive Area
        public bool IsArchived { get; set; }
        public DateTime? ArchiveDate { get; set; }

        public AnnouncementPinScope PinScope { get; set; }


    }
}
