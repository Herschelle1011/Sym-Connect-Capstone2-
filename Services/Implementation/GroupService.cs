using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Group;

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
         public async Task<List<Group>> GetAllGroupsAsync()
      {
          return await _context.Groups
              .Where(g => g.IsArchived == false)
              .Include(g => g.Leader)
                  .Include(gd => gd.GroupDetails)
                        .ThenInclude(m => m.Member)
                          .ToListAsync();
      }





        //GET GROUP BY ID
        public async Task<Group?> GetGroupByIdAsync(int id)
        {
            return await _context.Groups.
                      Where(g => g.IsArchived == false)
                        .Include(g => g.Leader). //load leader
                        Include(g => g.GroupDetails)
                            .ThenInclude(gd => gd.Member) //load each members
                      .FirstOrDefaultAsync(e => e.GroupId == id); //gives whole object of the group
        }

        //CREATE GROUP
        public async Task CreateGroupAsync(Group group)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
        }

        //UPDATE GROUP
        public async Task UpdateGroupAsync(Group group)
        {
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }

        //DELETE GROUP
        public async Task DeleteGroupAsync(int id)
        {
            var getGroup = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == id);
            if (getGroup == null) return;

            //remove connection in GroupDetails
            var groupMembers = await _context.GroupDetails.Where(gd => gd.GroupId == id).ToListAsync();
            //removed entities before group is deleted
            _context.GroupDetails.RemoveRange(groupMembers);
            //delete group
            _context.Groups.Remove(getGroup);

            await _context.SaveChangesAsync();

        }

        //CHECK IF GROUP ALREADY EXIST
        public async Task<bool> GroupExists(int id)
        {
            return await _context.Groups.
                AnyAsync(g => g.GroupId == id);
        }

        public async Task<SelectList> GetLeaderOptionsAsync(int? selectedId = null)
        {
            var members = await _context.Members
                                .Where(e => e.Status == "Active").ToListAsync();
            return new SelectList(members, "MemberId", "FirstName", selectedId);
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _context.Members.Where(m => m.Status == "Active").Include(g => g.GroupDetails).ToListAsync();
        }

        public async Task<List<Member>> GetAvailableMemberAsync(int groupId)
        {
            // Get IDs already in the group
            var existingIds = await _context.GroupDetails
                .Select(gd => gd.MemberId)
                .ToListAsync();


            // Return Active members NOT yet in the group
            return await _context.Members
                .Where(m => m.Status == "Active" && !existingIds.Contains(m.MemberId))
                .ToListAsync();
        }
        public async Task AddMemberToGroupAsync(int groupId, int memberId)
        {
            var AlreadyExist = await _context.GroupDetails.AnyAsync(m => m.MemberId == memberId);
            
            if (!AlreadyExist)
            {
                var group = new GroupDetails
                {
                    GroupId = groupId,
                    MemberId = memberId
                };

                _context.GroupDetails.Add(group);
                await _context.SaveChangesAsync();
            }
        }
        //EXLUDE Id = skip this current ID edited and check all facility id except the current ONE 
        public async Task<bool> GroupNameExists(string name, int? excludeId = null)
        {
            return await _context.Groups.AnyAsync(e => e.GroupName == name && e.IsArchived == false && (excludeId == null || e.GroupId != excludeId.Value));
        }

        public async Task ArchiveGroupAsync(int? id)
        {
            if (id == null) return;
            var group = await _context.Groups.FindAsync(id);
            if (group == null) return;
            group.Status = "Inactive";
            group.IsArchived = true;
            group.ArchivedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }


        public async Task RemoveMemberFromGroupAsync(int groupId, int memberId)
        {
            var groupDetail = await _context.GroupDetails
                            .FirstOrDefaultAsync(g => g.GroupId == groupId && g.MemberId == memberId);
            if (groupDetail == null) return;

            _context.GroupDetails.Remove(groupDetail);
            await _context.SaveChangesAsync();
            
        }

        //transfer A Member to existing group 
        public async Task TransferAMemberAsync(int memberId, int groupId)
        {
            var member = await _context.GroupDetails
                          .Include(gd => gd.Group)
                                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            var Group =  await _context.Groups.FindAsync(groupId);
            if (member == null || Group == null)
            {
                throw new Exception("Member or Group not found.");
            }
            //update from old group to a new  group
                member.GroupId = groupId; 
            await _context.SaveChangesAsync();
        }

    }
}
