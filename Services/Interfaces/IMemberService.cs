using Microsoft.AspNetCore.Mvc.Rendering;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.ViewModels.Member;
namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IMemberService
    {
        Task<List<Member>> GetAllMembersAsync();
        Task<Member?> GetMemberByIdAsync(int id);
        Task<bool> CreateMember(MembersCreateViewModel vm, IFormFile? photo);
        Task UpdateMemberAsync(Member member);
        Task DeleteMemberAsync(int id);
        Task<bool> MemberExists(int id);

        //LOAD ALL Members dropDown
        Task<List<Member>> GetMemberOptionsAsync(int groupId);
    }
}
