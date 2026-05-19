using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models
{
    public class User : IdentityUser
    {
        // Basic Identity
        public string? FirstName { get; set; } //allow null for now , we can make it required later if needed
        public string? LastName { get; set; } //allow null for now , we can make it required later if needed

        public string? MiddleName { get; set; }

        public string Status { get; set; } = "Active";

        public string? ProfilePictureUrl { get; set; } 

        public DateTime DateRegistered { get; set; } = DateTime.Now;    

        //public YouthMember? GroupMember { get; set; }
    }
}
