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
        Task<bool> GroupExists(int id);
        Task<bool> GroupNameExists(string name, int? excludeId = null);
        Task ArchiveGroupAsync(int? id);

        //LOAD ALL LEADERS
        Task<SelectList> GetLeaderOptionsAsync(int? selectedId = null);
        
        Task<List<Member>> GetAvailableMemberAsync(int groupId);
        Task AddMemberToGroupAsync(int groupId, int memberId);


    }
}
