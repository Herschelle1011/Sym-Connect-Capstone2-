using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models
{
    public class User : IdentityUser
    {
        public string? DisplayName { get; set; }

        public string? ProfilePictureUrl { get; set; }


        [ForeignKey(nameof(Member))]
        public int? MemberId { get; set; }

        public Member? Member { get; set; }

        public DateTime DateRegistered { get; set; } = DateTime.Now;    

    }
}
