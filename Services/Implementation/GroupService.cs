using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.ViewModels.Group;
using Sym_Connect__Capstone2_.ViewModels.Member;
using Microsoft.AspNetCore.Identity;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class GroupService : IGroupService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notificationService;


        public GroupService(AppDbContext context, UserManager<User> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        //GET ALL GROUPS
         public async Task<List<GroupListViewModel>> GetAllGroupsAsync()
      {
            var groups = await _context.Groups
                .AsNoTracking()
                .Where(g => !g.IsArchived)
                .Select(g => new GroupListViewModel
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    LeaderName = g.Leader!.FullName ?? "N/A",
                    LeaderEmail = g.Leader!.User.Email ?? "Unverified",
                    Status = g.Status,
                    Members = g.GroupDetails.Select(e => new GroupMemberViewModel
                    {
                        FullName = e.Member != null ? e.Member.FullName.Trim() : "N/A"
                    }).ToList(),
                }).ToListAsync();
            return groups;
      }

     

        //GET GROUP BY ID
        public async Task<GroupDetailsViewModel?> GetGroupByIdAsync(int id)
        {
            var group = await _context.Groups
                .AsNoTracking()
                .Where(g => g.GroupId == id & !g.IsArchived)
                .Select(g => new GroupDetailsViewModel
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    Description = g.Description,
                    Status = g.Status,
                    LeaderId = g.LeaderId,
                    PhotoUrl = g.Leader.User != null && !string.IsNullOrEmpty(g.Leader.User.ProfilePictureUrl)
                        ? g.Leader.User.ProfilePictureUrl
                        : "/images/default-avatar.png",
                    LeaderEmail = g.Leader != null ? g.Leader.User.Email : "Unverified",
                    LeaderName = g.Leader != null ? g.Leader.FirstName + " " + g.Leader.LastName : "No Leader Assigned",
                    LeaderContact = g.Leader != null ? g.Leader.ContactNumber : "N/A",

                    TotalPoints = g.GroupDetails
                .Where(gd => gd.MemberId != null)
                .Sum(gd => (int?)gd.Member!.EarnedPoints) ?? 0,


                    Members = g.GroupDetails.Where(m => m.MemberId != null)
                    .Select(gd => new GroupMemberViewModel
                    {
                        MemberId = gd.MemberId ?? 0,
                        FullName = gd.Member!.FullName,
                        Email = gd.Member.User.Email,
                        PhotoUrl = !string.IsNullOrEmpty(gd.Member.User.ProfilePictureUrl)
                        ? gd.Member.User.ProfilePictureUrl
                        : "/images/default-avatar.png",
                        ContactNumber = gd.Member.ContactNumber,
                        EarnedPoints = gd.Member.EarnedPoints,
                        Status = gd.Member.Status,
                        DateRegistered = gd.Member.DateRegistered
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (group?.Members?.Any() == true)
            {
                var topPoints = group.Members.Max(m => m.EarnedPoints);
                foreach (var m in group.Members.Where(m => m.EarnedPoints == topPoints))
                {
                    m.IsTopContributor = true;
                }
            }

            return group;
        }

        //PROMOTE FROM MEMBER TO LEADER
        public async Task<OperationalResult> PromoteMemberToLeaderAsync(int groupId, int newLeaderId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId && !g.IsArchived);
            if (group == null)
            {
                return new OperationalResult { Success = false, Message = "Group not found" };
            }

            var groupDetail = await _context.GroupDetails
                .FirstOrDefaultAsync(gd => gd.GroupId == groupId && gd.MemberId == newLeaderId);
            if (groupDetail == null)
            {
                return new OperationalResult { Success = false, Message = "Member is not part of this group" };
            }

            var newLeader = await _context.Members.FindAsync(newLeaderId);
            if (newLeader == null)
            {
                return new OperationalResult { Success = false, Message = "Member not found" };
            }

            int? oldLeaderId = group.LeaderId;

            // New leader steps up: remove from GroupDetails (leaders aren't tracked there), assign as LeaderId
            _context.GroupDetails.Remove(groupDetail);
            group.LeaderId = newLeaderId;

            // Old leader steps down into the member list
            if (oldLeaderId.HasValue && oldLeaderId != newLeaderId)
            {
                _context.GroupDetails.Add(new GroupDetails
                {
                    GroupId = groupId,
                    MemberId = oldLeaderId.Value
                });
            }

            await _context.SaveChangesAsync();

            await AddLeaderRoleAsync(newLeaderId);
            if (oldLeaderId.HasValue && oldLeaderId != newLeaderId)
            {
                await RemoveLeaderRoleIfNeededAsync(oldLeaderId.Value);
            }

            // Notify the newly promoted leader
            if (!string.IsNullOrEmpty(newLeader.UserId))
            {
                await _notificationService.NotifyUserAsync(
                    newLeader.UserId,
                    "You're now a group leader",
                    $"You have been promoted to leader of {group.GroupName}.",
                   $"Leader/Groups/Details/{group.GroupId}"
                );
            }

            return new OperationalResult { Success = true, Message = $"{newLeader.FullName} is now the leader of {group.GroupName}" };
        }



        //CREATE GROUP
        public async Task<OperationalResult> CreateGroupAsync(GroupCreateViewModel vm)
        {
            var newGroup = new Group
            {
                GroupName = vm.GroupName,
                Description = vm.Description,
                Status = vm.Status,
                LeaderId = vm.LeaderId,
            };
            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();

            if (vm.LeaderId.HasValue)
            {
                await AddLeaderRoleAsync(vm.LeaderId.Value);
            }



            return new OperationalResult { Success = true, Message = $"{newGroup.GroupName} created" };

        }

        //UPDATE GROUP
        public async Task<OperationalResult?> UpdateGroupAsync(GroupEditViewModel vm)
        {
            var ExistingGroup = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == vm.GroupId);
            if (ExistingGroup == null)
            {
                return new OperationalResult { Success = false, Message = "Group not existed" };
            }

            // Save the old leader before changing it
            int? oldLeaderId = ExistingGroup.LeaderId;

            // New leader from the edit form
            int? newLeaderId = vm.LeaderId;


            ExistingGroup.GroupName = vm.GroupName;
            ExistingGroup.Description = vm.Description;
            ExistingGroup.Status = vm.Status;
            ExistingGroup.LeaderId = vm.LeaderId;
            

            await _context.SaveChangesAsync();

            if (oldLeaderId.HasValue &&
             oldLeaderId != newLeaderId)
            {
                await RemoveLeaderRoleIfNeededAsync(
                    oldLeaderId.Value);
            }

            if (newLeaderId.HasValue)
            {
                await AddLeaderRoleAsync(
                    newLeaderId.Value);
            }


            return new OperationalResult { Success = true, Message = $"{ExistingGroup.GroupName} Updated" };

        }

        //DELETE GROUP
        public async Task<OperationalResult> DeleteGroupAsync(int id)
        {
            var getGroup = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == id);
            if (getGroup == null) return new OperationalResult { Success = false, Message = "Group not found" };

            int? oldLeaderId = getGroup.LeaderId;


            var groupMembers = await _context.GroupDetails.Where(gd => gd.GroupId == id).ToListAsync();
            _context.GroupDetails.RemoveRange(groupMembers);
            getGroup.LeaderId = null;
            getGroup.Leader = null;
            var announcements = await _context.Announcements
               .Where(a => a.GroupId == id)
               .ToListAsync();

            var members = await _context.Members.Where(e => e.PreferredGroupId == id).ToListAsync();
            foreach(var preferredGroup in members)
            {
                preferredGroup.PreferredGroupId = null;
            }

            foreach (var announcement in announcements)
            {
                announcement.GroupId = null;
            }

            _context.Groups.Remove(getGroup);
            await _context.SaveChangesAsync();

            if (oldLeaderId.HasValue)
            {
                await RemoveLeaderRoleIfNeededAsync(
                    oldLeaderId.Value);
            }
            return new OperationalResult { Success = true, Message = $"{getGroup.GroupName} deleted" };
        }

        //CHECK IF GROUP ALREADY EXIST
        public async Task<bool> GroupExists(int id)
        {
            return await _context.Groups.
                AnyAsync(g => g.GroupId == id);
        }

        //public async Task<SelectList> GetLeaderOptionsAsync(int? selectedId = null)
        //{
        //    var members = await _context.Members
        //                        .Where(e => e.Status == "Active").ToListAsync();
        //    return new SelectList(members, "MemberId", "FirstName", selectedId);
        //}


        // ======== CHOOSE LEADERS ========= 
        public async Task<List<MemberListViewModel>> GetAvailableLeadersAsync(string? search = null, int? currentGroupId = null ,int? excludeId = null)
        {
            var members = _context.Members.AsNoTracking()
                .Where(m => m.ApplicationStatus == MembershipApplicationStatus.Approved && m.Status == "Active");

            var alreadyLeadingIds = await _context.Groups
                .Where(g => !g.IsArchived && (currentGroupId == null || g.GroupId != currentGroupId))
                .Select(g => g.LeaderId)
                .Where(id => id != null)
                .ToListAsync();    

            var membersInOtherGroups = await _context.GroupDetails
                .Where(gd => currentGroupId == null || gd.GroupId != currentGroupId)
                .Where(gd => gd.MemberId != null)
                .Select(gd => gd.MemberId!.Value)
                .ToListAsync();


            members = members.Where(m =>
              !alreadyLeadingIds.Contains(m.MemberId) &&
              !membersInOtherGroups.Contains(m.MemberId));


            if (!string.IsNullOrWhiteSpace(search))
            {
                members = members
                    .Where(m => m.FirstName.Contains(search) ||
                    m.LastName.Contains(search) || m.User.Email.Contains(search));
            }

            return await members.OrderByDescending(m => m.EarnedPoints)
                .Select(m => new MemberListViewModel
                {
                    MemberId = m.MemberId,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Email = m.User != null ? (m.User.Email ?? "Unverified") : "Unverified",
                    Points = m.EarnedPoints,
                    PhotoUrl = m.User != null ? m.User.ProfilePictureUrl : ""
                }).ToListAsync();

        }





        //SELECTS A MEMBER NOT A LEADER OR EXISTING MEMBER IDS ========
        public async Task<List<MemberListViewModel>> GetAvailableMemberAsync(int groupId)
        {
            // Get IDs already in the group
            var existingIds = await _context.GroupDetails
                .Select(gd => gd.MemberId)
                .ToListAsync();

            var leaderId = await _context.Groups
                 .Where(g => g.GroupId == groupId)
                 .Select(g => g.LeaderId)
                 .FirstOrDefaultAsync();


            return await _context.Members
       .AsNoTracking()
       .Where(m => m.Status == "Active" &&
       !existingIds.Contains(m.MemberId) &&
       m.MemberId != leaderId && !_context.Groups.Any(g => g.LeaderId == m.MemberId))
     .OrderByDescending(m => m.EarnedPoints)
       .Select(m => new MemberListViewModel
       {
           MemberId = m.MemberId,
           FirstName = m.FirstName,
           LastName = m.LastName,
           Email = m.User.Email ?? "Unverified",
           Points = m.EarnedPoints,
       }).ToListAsync();
        }


        // ADD MEMBER TO GROUP =======
        public async Task<OperationalResult> AddMemberToGroupAsync(int groupId, int memberId)
        {
            var AlreadyExist = await _context.GroupDetails
                
                .AnyAsync(m => m.MemberId == memberId);
            if (AlreadyExist)
            {
                return new OperationalResult { Success = false, Message = $"Member already exist" };
            }

            var targetGroup = await _context.Groups.FindAsync(groupId);
            if(targetGroup == null)
            {
                return new OperationalResult { Success = false, Message = "Group not found" };
            }
            var group = new GroupDetails
            {
            GroupId = groupId,
            MemberId = memberId
            };

                _context.GroupDetails.Add(group);
                await _context.SaveChangesAsync();
                return new OperationalResult { Success = true, Message = $"Member added to this {targetGroup.GroupName}" };
            
        }
        //EXLUDE Id = skip this current ID edited and check all facility id except the current ONE 
        public async Task<bool> GroupNameExists(string name, int? excludeId = null)
        {
            return await _context.Groups.AnyAsync(e => e.GroupName == name && e.IsArchived == false && (excludeId == null || e.GroupId != excludeId.Value));
        }

        public async Task<OperationalResult> RemoveMemberFromGroupAsync(int groupId, int memberId)
        {
            
            var groupDetail = await _context.GroupDetails
                            .FirstOrDefaultAsync(g => g.GroupId == groupId && g.MemberId == memberId);
            if (groupDetail == null) return new OperationalResult { Success = false, Message = "Group not found" }; ;

            var targetMember = await _context.Members.FindAsync(memberId);
            if (targetMember == null)
            {
             return new OperationalResult { Success = false, Message = "Member not found" }; ;
            }

            _context.GroupDetails.Remove(groupDetail);
            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = $"{targetMember.FullName} has been removed from this group" };

        }

        //transfer A Member to existing group 
        public async Task<OperationalResult> TransferAMemberAsync(int memberId, int groupId)
        {
            var member = await _context.GroupDetails
                .Include(m => m.Member)
                     .FirstOrDefaultAsync(m => m.MemberId == memberId);
            if(member == null) return new OperationalResult { Success = false, Message = $"Member not found" };

            var Group =  await _context.Groups.FindAsync(groupId);

            if (Group == null) return new OperationalResult { Success = false, Message = "Group not found" };

            if (member.GroupId == groupId)
                return new OperationalResult { Success = false, Message = $"{member.Member.FullName} is already in this group" };
            member.GroupId = groupId; 
            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = $"{member.Member.FullName} has been removed from this group" };

        }

        public async Task<bool> IsMemberALeaderAsync(int? memberId)
        {
            return await _context.Groups
                .AnyAsync(g => g.LeaderId == memberId && !g.IsArchived);
        }

        public async Task<Group?> GetGroupLedByMemberAsync(int? memberId)
        {
            return await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.LeaderId == memberId && !g.IsArchived);
        }

        public async Task<Group?> GetGroupByLeaderIdAsync(int memberId)
        {
            return await _context.Groups
           .AsNoTracking()
           .FirstOrDefaultAsync(g =>
               g.LeaderId == memberId &&
               !g.IsArchived);
        }


        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Groups.Where(g => g.IsArchived == false).CountAsync();
        }



        private async Task AddLeaderRoleAsync(int memberId)
        {
            var member = await _context.Members
                .Include(m => m.User)
                .FirstOrDefaultAsync(m =>
                    m.MemberId == memberId);

            if (member == null || member.User == null)
            {
                return;
            }

            bool alreadyHasRole =
                await _userManager.IsInRoleAsync(
                    member.User,
                    "Leader");

            if (!alreadyHasRole)
            {
                await _userManager.AddToRoleAsync(
                    member.User,
                    "Leader");
            }
        }

        private async Task RemoveLeaderRoleIfNeededAsync(int memberId)
        {
            // Check if this person still leads another active group
            bool stillLeader =
                await _context.Groups
                    .AnyAsync(g =>
                        g.LeaderId == memberId &&
                        !g.IsArchived);

            // Still leading another group?
            // Keep the Leader role.
            if (stillLeader)
            {
                return;
            }

            var member = await _context.Members
                .Include(m => m.User)
                .FirstOrDefaultAsync(m =>
                    m.MemberId == memberId);

            if (member == null || member.User == null)
            {
                return;
            }

            bool hasLeaderRole =
                await _userManager.IsInRoleAsync(
                    member.User,
                    "Leader");

            if (hasLeaderRole)
            {
                await _userManager.RemoveFromRoleAsync(
                    member.User,
                    "Leader");
            }
        }

        // GET ARCHIVED GROUPS
        public async Task<GroupListViewModel?> GetArchivedModalGroups(int id)
        {
            return await _context.Groups
                .AsNoTracking()
                .Where(g => g.IsArchived == false && g.GroupId == id)
                .Select(g => new GroupListViewModel
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    LeaderName = g.Leader != null ? g.Leader.FullName : "",
                    LeaderEmail = g.Leader.User != null ? g.Leader.User.Email : "",
                    Status = g.Status,
                    Members = new List<GroupMemberViewModel>(),
                    MemberCount = _context.GroupDetails
                   .Count(gd => gd.GroupId == g.GroupId)
                })
                .FirstOrDefaultAsync();
        }

        // ARCHIVE GROUP POST
        public async Task<OperationalResult> ArchiveGroupAsync(int? id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return new OperationalResult { Success = false, Message = "Group does not exist" };
            }

            int? oldLeaderId = group.LeaderId;

            var groupMembers = await _context.GroupDetails
                .Where(gd => gd.GroupId == id)
                .ToListAsync();
            _context.GroupDetails.RemoveRange(groupMembers);

            group.Status = "Inactive";
            group.IsArchived = true;
            group.ArchivedDate = DateTime.Now;
            group.Leader = null;
            group.LeaderId = null;

            await _context.SaveChangesAsync();

            if (oldLeaderId.HasValue)
            {
                await RemoveLeaderRoleIfNeededAsync(oldLeaderId.Value);
            }

            return new OperationalResult { Success = true, Message = "Group archived" };
        }

        // GET ARCHIVED GROUPS
        public async Task<List<GroupListViewModel>> GetArchivedGroupsAsync()
        {
            return await _context.Groups
                .AsNoTracking()
                .Where(g => g.IsArchived)
                .OrderByDescending(g => g.ArchivedDate)
                .Select(g => new GroupListViewModel
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    LeaderName = "N/A", // always true post-archive
                    LeaderEmail = "N/A",
                    Status = g.Status,
                    Members = new List<GroupMemberViewModel>() 
                }).ToListAsync();
        }

        // RESTORE GROUP — leader is mandatory, not optional
        public async Task<OperationalResult> RestoreGroupAsync(int id, int leaderId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == id);
            if (group == null || !group.IsArchived)
            {
                return new OperationalResult { Success = false, Message = "Archived group not found" };
            }

            var leader = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == leaderId
                    && m.ApplicationStatus == MembershipApplicationStatus.Approved
                    && m.Status == "Active");
            if (leader == null)
            {
                return new OperationalResult { Success = false, Message = "Selected leader is not eligible" };
            }

            bool alreadyLeadsElsewhere = await _context.Groups
                .AnyAsync(g => g.LeaderId == leaderId && !g.IsArchived);
            bool alreadyInAnotherGroup = await _context.GroupDetails
                .AnyAsync(gd => gd.MemberId == leaderId);
            if (alreadyLeadsElsewhere || alreadyInAnotherGroup)
            {
                return new OperationalResult { Success = false, Message = "This member is already assigned to another group" };
            }
            group.IsArchived = false;
            group.ArchivedDate = null;
            group.Status = "Active";
            group.LeaderId = leaderId;
            await _context.SaveChangesAsync();
            await AddLeaderRoleAsync(leaderId);
            return new OperationalResult { Success = true, Message = $"{group.GroupName} restored" };
        }







        //== LEADERS VIEW ===== 
        // GET GROUP FOR LEADER — view-only, scoped to the group this member leads
        public async Task<LeaderGroupViewModel?> GetGroupForLeaderAsync(int leaderId, string? search = null, string? sort = "points_desc")
        {
            var query = _context.Groups
                .AsNoTracking()
                .Where(g => g.LeaderId == leaderId && !g.IsArchived);

            var group = await query
                .Select(g => new LeaderGroupViewModel
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    TotalMembers = g.GroupDetails.Count(gd => gd.MemberId != null),
                    Members = g.GroupDetails
                        .Where(gd => gd.MemberId != null)
                        .Select(gd => new LeaderGroupMemberViewModel
                        {
                            MemberId = gd.MemberId ?? 0,
                            LastName = gd.Member!.LastName,
                            FirstName = gd.Member.FirstName,
                            MiddleName = gd.Member.MiddleName,
                            Status = gd.Member.Status,
                            EarnedPoints = gd.Member.EarnedPoints,
                            Email = gd.Member.User.Email,
                            ContactNumber = gd.Member.ContactNumber,
                            PhotoUrl = !string.IsNullOrEmpty(gd.Member.User.ProfilePictureUrl)
                            ? gd.Member.User.ProfilePictureUrl
                            : "/images/default-avatar.png",
                            DateRegistered = gd.Member.DateRegistered
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (group == null) return null;

            // Search filter (name only — this is a view screen, keep it simple)
            if (!string.IsNullOrWhiteSpace(search))
            {
                group.Members = group.Members
                    .Where(m => $"{m.FirstName} {m.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sort
            group.Members = sort switch
            {
                "points_asc" => group.Members.OrderBy(m => m.EarnedPoints).ToList(),
                "name_asc" => group.Members.OrderBy(m => m.LastName).ToList(),
                "name_desc" => group.Members.OrderByDescending(m => m.LastName).ToList(),
                _ => group.Members.OrderByDescending(m => m.EarnedPoints).ToList() // "points_desc" default
            };

            // Row numbers for the "Id no." column — display order, not MemberId
            for (int i = 0; i < group.Members.Count; i++)
            {
                group.Members[i].RowNumber = i + 1;
            }

            return group;
        }


    }
}
