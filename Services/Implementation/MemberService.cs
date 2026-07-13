using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using System.Collections.Immutable;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class MemberService : IMemberService
    {
        private readonly AppDbContext _context;

        public MemberService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _context.Members
                .Include(gd => gd.GroupDetails)             
                .ThenInclude(g => g.Group).ToListAsync();
        }

        public async Task <Member?> GetMemberByIdAsync(int id)
        {
            return await _context.Members.Include(gd => gd.GroupDetails)
                              .ThenInclude(g => g.Group)
                                     .FirstOrDefaultAsync(m => m.MemberId == id);
        }

        public async Task CreateMemberAsync(Member member)
        {
              _context.Members.Add(member);
                      await _context.SaveChangesAsync();
        }
        public async Task UpdateMemberAsync(Member member)
        {
            _context.Members.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMemberAsync(int id)
        {
           var getMember = await _context.Members.FindAsync(id);
            if (getMember == null) { return; }

            var groupMember = await _context.Groups.Where(m => m.LeaderId == id).ToListAsync();

            foreach(var leader in groupMember)
            {
                leader.LeaderId = null;
            }

            if (getMember != null)
            {
                _context.Members.Remove(getMember);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> MemberExists(int id)
        {
           return await _context.Members.AnyAsync(m => m.MemberId == id);
        }

        public async Task<List<Member>> GetMemberOptionsAsync(int groupId)
        {
            var alreadyInGroup = await _context.GroupDetails.Where(gd => gd.GroupId == groupId).Select(gd => gd.MemberId).ToListAsync();

            return await _context.Members.Where(e => e.Status == "Active" && !alreadyInGroup.Contains(e.MemberId)).ToListAsync();
                                            
        }
    }
}
