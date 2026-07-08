
namespace Sym_Connect__Capstone2_.ViewModels.Member
{
    public class MemberDeleteViewModel
    {
        public int memberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? groupId { get; set; }
        public string? groupName { get; set; }
    }
}
