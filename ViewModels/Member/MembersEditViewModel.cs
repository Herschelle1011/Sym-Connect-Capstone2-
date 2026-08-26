using System.ComponentModel.DataAnnotations;
namespace Sym_Connect__Capstone2_.ViewModels.Member
{
    public class MembersEditViewModel : MemberFormViewModel
    {
        public int memberId { get; set; }

        public bool IsEmailConfirmed { get; set; }
    }
}
