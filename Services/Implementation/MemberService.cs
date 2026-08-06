using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Member;
using System.Collections.Immutable;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class MemberService : IMemberService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinary;

        public MemberService(AppDbContext context, CloudinaryService cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        //---- QR TOKEN PART ==== 


        public async Task<string> GetOrCreateQrTokenAsync(int memberId)
        {
            var member = await _context.Members.FindAsync(memberId);
            if(member == null) throw new InvalidOperationException("Member not found.");

            //IF QR token is empty
            //convert to GUID -----
            if (string.IsNullOrEmpty(member.QRToken))
            {
                member.QRToken = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();
            }

            //return if already have QRToken
            return member.QRToken;
        }


        public async Task<List<MemberListViewModel>> GetAllMembersAsync()
        {
            return await _context.Members.Select(m => new MemberListViewModel
            {
                MemberId = m.MemberId,
                FirstName = m.FirstName,
                MiddleName = m.MiddleName ?? "",
                LastName = m.LastName,
                Email = m.Email,
                ContactNumber = m.ContactNumber,
                Address = m.Address,
                BirthDate = m.BirthDate,
                Gender = m.Gender,
                Status = m.Status,
                DateRegistered = m.DateRegistered,
                GroupDetails = m.GroupDetails.Select(gd => new MemberGroupDetailsViewModel
                {
                    GroupDetailsId = gd.GroupDetailsId,
                    GroupId = gd.GroupId,
                    MemberId = gd.MemberId,
                    GroupName = gd.Group.GroupName ?? "",
                   
                }).ToList()
            }).ToListAsync();         
        }

        public async Task <Member?> GetMemberByIdAsync(int id)
        {
            return await _context.Members.Include(gd => gd.GroupDetails)
                              .ThenInclude(g => g.Group)
                                     .FirstOrDefaultAsync(m => m.MemberId == id);
        }

        public async Task<bool> CreateMember(MembersCreateViewModel vm, IFormFile? photo)
        {
            var member = new Member
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                MiddleName = vm.MiddleName,
                Email = vm.Email,
                ContactNumber = vm.ContactNumber,
                Address = vm.Address,
                BirthDate = vm.BirthDate,
                Gender = vm.Gender,
                Status = vm.Status
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            if (photo != null)
            {

                var url = await _cloudinary.UploadProfilePicture(photo, member.MemberId.ToString());
                if (url != null)
                {
                    member.ProfilePictureUrl = url;
                    await _context.SaveChangesAsync(); // second save just for the URL
                }
                else if (!string.IsNullOrWhiteSpace(url))
                {
                    member.ProfilePictureUrl = url.Trim();
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task UpdateMemberAsync(MembersEditViewModel vm, IFormFile? photo)
        {
            var member = await _context.Members.FindAsync(vm.memberId);
            if (member == null) return; // or throw, depending on how you want to handle a missing record

            member.FirstName = vm.FirstName;
            member.LastName = vm.LastName;
            member.MiddleName = vm.MiddleName;
            member.Email = vm.Email;
            member.ContactNumber = vm.ContactNumber;
            member.Address = vm.Address;
            member.BirthDate = vm.BirthDate;
            member.Gender = vm.Gender;
            member.Status = vm.Status;

            _context.Members.Update(member);
            await _context.SaveChangesAsync();

            if (photo != null)
            {
                var url = await _cloudinary.UploadProfilePicture(photo, member.MemberId.ToString());
                if (url != null)
                {
                    member.ProfilePictureUrl = url;
                    await _context.SaveChangesAsync(); // second save just for the URL
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.photoUrl))
            {
                member.ProfilePictureUrl = vm.photoUrl.Trim();
                await _context.SaveChangesAsync();
            }
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
