using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Services
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
                .Include(g => g.Leader)
                    .Include(gd => gd.GroupDetails)
                          .ThenInclude(m => m.Member)
                .ToListAsync();
        }

        //GET GROUP BY ID
        public async Task<Group?> GetGroupByIdAsync (int id)
        {
            return await _context.Groups.
                        Include(g => g.Leader). //load leader
                        Include(g => g.GroupDetails) 
                            .ThenInclude(gd => gd.Member) //load each members
                      .FirstOrDefaultAsync(e => e.GroupId == id);
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
        public async Task DeleteGroupAsync (int id)
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
        public bool GroupExists (int id, string name)
        {
            return _context.Groups.             
                Any(g => g.GroupId == id && g.GroupName == name);
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
                .Where(gd => gd.GroupId == groupId)
                .Select(gd => gd.MemberId)
                .ToListAsync();

            // Return Active members NOT yet in the group
            return await _context.Members
                .Where(m => m.Status == "Active" && !existingIds.Contains(m.MemberId))
                .ToListAsync(); // ToListAsync first — so FullName ([NotMapped]) works
        }
        public async Task AddMemberToGroupAsync(int groupId, int memberId)
        {
            var AlreadyExist = await _context.GroupDetails.AnyAsync(m => m.MemberId == memberId && groupId == m.GroupId);
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


    }
}
