using Microsoft.AspNetCore.Mvc.Rendering;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.ViewModels.Group;
using Sym_Connect__Capstone2_.ViewModels.Member;
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
        Task<List<MemberListViewModel>> GetAvailableLeadersAsync(string? search = null, int? currentGroupId = null, int? excludeId = null);
       Task<List<MemberListViewModel>> GetAvailableMemberAsync(int groupId);
       Task<OperationalResult> AddMemberToGroupAsync(int groupId, int memberId);

        //CHECK IF A MEMEBR IS A LEADER OF A GROUP? 
        Task<bool> IsMemberALeaderAsync(int? memberId);

        Task<Group?> GetGroupLedByMemberAsync(int? memberId);

        Task<Group?> GetGroupByLeaderIdAsync(int memberId);

        Task<int> GetTotalCountAsync();

        //ARCHIVED
        Task<List<GroupListViewModel>> GetArchivedGroupsAsync();
        Task<OperationalResult> RestoreGroupAsync(int id, int leaderId);

        Task<GroupListViewModel?> GetArchivedModalGroups(int id);

        Task<OperationalResult> PromoteMemberToLeaderAsync(int groupId, int newLeaderId);

        Task<LeaderGroupViewModel?> GetGroupForLeaderAsync(int leaderId, string? search = null, string? sort = "points_desc");

        Task<ArchivedGroupDetailsViewModel?> GetArchivedDetailsAsync(int id);

        Task<int> GetMemberCountAsync(int groupId);

        Task<int> GetTotalEarnedPointsAsync(int groupId);


        Task<GroupDetailsViewModel?> GetGroupDetailsForMemberAsync(int memberId);


    }
}
