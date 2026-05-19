using Microsoft.AspNetCore.Mvc.Rendering;
using Sym_Connect__Capstone2_.Models;
namespace Sym_Connect__Capstone2_.Services
{
    public interface IMemberService
    {
        Task<List<Member>> GetAllMembersAsync();
        Task<Member?> GetMemberByIdAsync(int id);
        Task CreateMemberAsync(Member member);
        Task UpdateMemberAsync(Member member);
        Task DeleteMemberAsync(int id);
        bool MemberExists(int id);

        //LOAD ALL Members dropDown
        Task<List<Member>> GetMemberOptionsAsync(int groupId);
    }
}
