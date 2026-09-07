namespace Sym_Connect__Capstone2_.ViewModels.Member
{
    public class MembersPageViewModel
    {
        public List<MemberListViewModel> Members { get; set; } = new();
        public List<MembershipApplicantDetailsViewModel> Applicants { get; set; } = new();
    }
}
