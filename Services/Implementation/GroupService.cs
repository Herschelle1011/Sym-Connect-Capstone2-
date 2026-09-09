using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.ViewModels.Group;
using Sym_Connect__Capstone2_.ViewModels.Member;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class GroupService : IGroupService
    {
        private readonly AppDbContext _context;

        public GroupService(AppDbContext context)
        {
            _context = context;
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
                    LeaderEmail = g.Leader != null ? g.Leader.User.Email : "Unverified",
                    LeaderName = g.Leader != null ? g.Leader.FirstName + " " + g.Leader.LastName : "No Leader Assigned",
                    LeaderContact = g.Leader != null ? g.Leader.ContactNumber : "N/A",

                    Members = g.GroupDetails.Where(m => m.MemberId != null)
                    .Select(gd => new GroupMemberViewModel
                    {
                        MemberId = gd.MemberId ?? 0,
                        FullName = gd.Member!.FullName,
                        Email = gd.Member.User.Email,
                        ContactNumber = gd.Member.ContactNumber,
                        Status = gd.Member.Status,
                        DateRegistered = gd.Member.DateRegistered
                    }).ToList()
                }).FirstOrDefaultAsync();

            return group;
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
            ExistingGroup.GroupName = vm.GroupName;
            ExistingGroup.Description = vm.Description;
            ExistingGroup.Status = vm.Status;
            ExistingGroup.LeaderId = vm.LeaderId;

            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = $"{ExistingGroup.GroupName} Updated" };

        }

        //DELETE GROUP
        public async Task<OperationalResult> DeleteGroupAsync(int id)
        {
            var getGroup = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == id);
            if (getGroup == null) return new OperationalResult { Success = false, Message = "Group not found" };

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
                .Where(m => m.Status == "Active");

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
                    PhotoUrl = m.ProfilePictureUrl
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

        public async Task<OperationalResult> ArchiveGroupAsync(int? id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return new OperationalResult { Success = false, Message = "Group not existed" };
            group.Status = "Inactive";
            group.IsArchived = true;
            group.ArchivedDate = DateTime.Now;
            group.Leader = null;
            group.LeaderId = null;
            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = "Group saved to archived" };
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
    }
}
