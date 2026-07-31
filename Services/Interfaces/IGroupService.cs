using Microsoft.AspNetCore.Mvc.Rendering;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.ViewModels.Group;
namespace Sym_Connect__Capstone2_.Services.Interfaces
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

        Task RemoveMemberFromGroupAsync(int groupId, int memberId);

        Task TransferAMemberAsync(int memberId, int groupId);

        //LOAD ALL LEADERS
        Task<SelectList> GetLeaderOptionsAsync(int? selectedId = null);
        
        Task<List<Member>> GetAvailableMemberAsync(int groupId);
        Task AddMemberToGroupAsync(int groupId, int memberId);


    }
}
