using Microsoft.AspNetCore.Mvc.Rendering;
using Sym_Connect__Capstone2_.Models;
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
        ////LOAD ALL Members dropDown
        //Task<List<Member>> GetMemberOptionsAsync(int groupId);
    }
}
