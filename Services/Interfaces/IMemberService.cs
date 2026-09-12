using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Member;
namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IMemberService
    {
        Task<List<MemberListViewModel>> GetAllMembersAsync();
         Task<Member?> GetMemberByIdAsync(int id);
        Task<OperationalResult> CreateMember(MembersCreateViewModel vm, IFormFile? photo);
        Task UpdateMemberAsync(MembersEditViewModel vm, IFormFile? photo);
         Task DeleteMemberAsync(int id);
        Task<bool> MemberExists(int id);

        Task<MemberDetailsViewModel?> GetMemberDetailsByIdAsync(int id);

        Task<string> GetOrCreateQrTokenAsync(int memberId);

        Task<OperationalResult> ConfirmEmailAsync(string userId, string Token);


        Task<OperationalResult> SetPasswordAsync(string userId, string token, string password);

        Task<(int? GroupId, bool IsLeader)> GetAnnouncementContextAsync(int memberId);
        Task<int> GetTotalCountAsync();

        Task<List<MembershipApplicantDetailsViewModel>> AllMemberApplicants();

        Task<MembershipApplicantDetailsViewModel?> GetApplicantInformationByIdAsync(int id);

        Task<OperationalResult> ApproveApplicantAsync(int id);
            
        Task<OperationalResult> RejectApplicantAsync(int id, string reason);

        Task<ApproveMembershipViewModel?> GetMemberForApprovalAsync(int memberId);

        Task<RejectMembershipViewModel?> GetMemberForRejectionAsync(int memberId);

        Task<ArchiveMemberViewModel> GetArchiveMemberAsync(int memberId);
        Task<OperationalResult> ArchiveMemberAsync(int memberId);
       
    }
}
