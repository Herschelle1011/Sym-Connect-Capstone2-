using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }
        public string? QRToken { get; set; }

        //temporary only

        public string? UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{FirstName} {(string.IsNullOrWhiteSpace(MiddleName) ? "" : MiddleName + " ")}{LastName}";
            }
        }

        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ContactNumber { get; set; } = string.Empty;
        public string Barangay { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string Country { get; set; } = "Philippines"; public DateTime? BirthDate { get; set; }

        public string Address { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = "Active";

        public DateTime DateRegistered { get; set; } = DateTime.Now;

  
        public ICollection<GroupDetails> GroupDetails { get; set; } = new List<GroupDetails>();

        public string? ProfilePictureUrl { get; set; }

        public int EarnedPoints { get; set; }


    }
}
