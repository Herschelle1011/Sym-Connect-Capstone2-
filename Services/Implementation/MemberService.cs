using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Group;
using Sym_Connect__Capstone2_.ViewModels;
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

        //========= GET ALL MEMBERS PART ==== 

        public async Task<List<MemberListViewModel>> GetAllMembersAsync()
        {
            return await _context.Members
                .AsNoTracking()
                .Select(m => new MemberListViewModel
            {
                MemberId = m.MemberId,
                FirstName = m.FirstName,
                MiddleName = m.MiddleName ?? "",
                LastName = m.LastName,
                Email = m.Email,
                    PhotoUrl = string.IsNullOrEmpty(m.ProfilePictureUrl)
                      ? "/images/default-avatar.png"
                      : m.ProfilePictureUrl,
                    ContactNumber = m.ContactNumber,
                    Address = BuildAddress( m.Barangay, m.City, m.Province, m.Country),
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

        public async Task<MemberDetailsViewModel?> GetMemberDetailsByIdAsync(int id)
        {
            var attendanceCheck = await _context.Attendances
                .CountAsync(m => m.MemberId == id && m.Status == "Present");   

            var member = await _context.Members
                .AsNoTracking()
                .Where(m => m.MemberId == id)
                .Select(m => new MemberDetailsViewModel
                {
                    FullName = m.FullName,
                    BirthDate = m.BirthDate,
                    Gender = m.Gender,
                    Status = m.Status,
                    MemberSince = m.DateRegistered,
                    Email = m.Email,
                    ContactNumber = m.ContactNumber,
                    Address = m.Address,
                    PhotoUrl = string.IsNullOrEmpty(m.ProfilePictureUrl)
                      ? "/images/default-avatar.png"
                      : m.ProfilePictureUrl,              
                     EventAttendedCount = attendanceCheck,
                    groupDetails = m.GroupDetails.Select(g => new GroupDetailsViewModel
                    {
                        GroupName = g.Group.GroupName,
                        DateJoined = g.DateJoined
                    }).ToList()
                }).FirstOrDefaultAsync();
            return member;
        }


        public async Task<OperationalResult> CreateMember(MembersCreateViewModel vm, IFormFile? photo)
        {
            var member = new Member
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                MiddleName = vm.MiddleName,
                Email = vm.Email,
                ContactNumber = vm.ContactNumber,
                Address = BuildAddress( vm.Barangay, vm.City, vm.Province, vm.Country),
                DateRegistered = DateTime.Now,
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
                    await _context.SaveChangesAsync();
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.PhotoUrl))
            {
                member.ProfilePictureUrl = vm.PhotoUrl.Trim();
                await _context.SaveChangesAsync();
            }
            return new OperationalResult { Success = true, Message = "Member added successfully!" };
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
            member.Address = BuildAddress( vm.Barangay, vm.City, vm.Province, vm.Country);
            member.BirthDate = vm.BirthDate;
            member.Gender = vm.Gender;
            member.Status = vm.Status;

            await _context.SaveChangesAsync();

            if (photo != null)
            {
                var url = await _cloudinary.UploadProfilePicture(photo, member.MemberId.ToString());
                if (url != null)
                {
                    member.ProfilePictureUrl = url;
                    await _context.SaveChangesAsync(); 
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.PhotoUrl))
            {
                member.ProfilePictureUrl = vm.PhotoUrl.Trim();
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
            return await _context.Members
                .Where(e => e.Status == "Active" &&
                            !_context.GroupDetails.Any(gd => gd.GroupId == groupId && gd.MemberId == e.MemberId))
                .ToListAsync();
        }

        private static string BuildAddress( string barangay, string city, string province, string country)
        {
            var parts = new[] { barangay, city, province, country }
                .Where(p => !string.IsNullOrWhiteSpace(p));

            return string.Join(", ", parts);
        }

    }
}
