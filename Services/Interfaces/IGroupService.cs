using Microsoft.AspNetCore.Mvc.Rendering;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Group;
namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IGroupService
    {
        Task<List<GroupListViewModel>> GetAllGroupsAsync();
        Task<GroupDetailsViewModel?> GetGroupByIdAsync(int id);
        Task<OperationalResult> CreateGroupAsync(GroupCreateViewModel vm);
        Task<OperationalResult?> UpdateGroupAsync(GroupEditViewModel vm);
        Task<OperationalResult> DeleteGroupAsync(int id);
        Task<bool> GroupExists(int id);
        Task<bool> GroupNameExists(string name, int? excludeId = null);
        Task<OperationalResult> ArchiveGroupAsync(int? id);

        Task<OperationalResult> RemoveMemberFromGroupAsync(int groupId, int memberId);

        Task<OperationalResult> TransferAMemberAsync(int memberId, int groupId);

        //LOAD ALL LEADERS
        Task<SelectList> GetLeaderOptionsAsync(int? selectedId = null);
        
        Task<List<Member>> GetAvailableMemberAsync(int groupId);
        Task<OperationalResult> AddMemberToGroupAsync(int groupId, int memberId);

    }
}
