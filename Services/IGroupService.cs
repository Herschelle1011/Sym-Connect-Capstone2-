using Microsoft.AspNetCore.Mvc.Rendering;
using Sym_Connect__Capstone2_.Models;
namespace Sym_Connect__Capstone2_.Services
{
    public interface IGroupService
    {
        Task<List<Group>> GetAllGroupsAsync();
        Task<List<Member>> GetAllMembersAsync();
        Task<Group?> GetGroupByIdAsync(int id);
        Task CreateGroupAsync(Group group);
        Task UpdateGroupAsync(Group group);
        Task DeleteGroupAsync(int id);
        bool GroupExists(int id, string name);

        //LOAD ALL LEADERS
        Task<SelectList> GetLeaderOptionsAsync(int? selectedId = null);
        
        Task<List<Member>> GetAvailableMemberAsync(int groupId);
        Task AddMemberToGroupAsync(int groupId, int memberId);


    }
}
