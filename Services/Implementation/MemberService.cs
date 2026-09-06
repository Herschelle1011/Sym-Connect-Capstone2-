using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Group;
using Sym_Connect__Capstone2_.ViewModels.Member;
using Sym_Connect__Capstone2_.ViewModels.Membership;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class MemberService : IMemberService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinary;
        private readonly UserManager<User> _userManager;


        public MemberService(AppDbContext context, CloudinaryService cloudinary, UserManager<User> userManager)
        {
            _context = context;
            _cloudinary = cloudinary;
            _userManager = userManager;
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


        //======== CONFIRM EMAIL =========

        public async Task<OperationalResult> ConfirmEmailAsync(string userId, string Token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "User account was not found"
                };
            }

            //email confirmed = true
            var result = await _userManager.ConfirmEmailAsync(user, Token);
            if (!result.Succeeded)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Email confirmation failed."
                };
            }

            var passwordToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            return new OperationalResult
            {
                Success = true,
                Message = "Email verified successfully.",
                UserId = user.Id,
                PasswordToken = passwordToken
            };


        }

        // ===== SET PASSWORD ==== 
        public async Task<OperationalResult> SetPasswordAsync(string userId, string token, string password)
        {
            var user = await _context.Users.FindAsync(userId);
            if(user == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "User account was not found."
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(
                       " ",
                       result.Errors.Select(e => e.Description)
                   );

                return new OperationalResult
                {
                    Success = false,
                    Message = errors
                };
            }

            return new OperationalResult
            {
                Success = true,
                Message = "Password created successfully."
            };

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
                Email = m.User.Email,
                    PhotoUrl = string.IsNullOrEmpty(m.ProfilePictureUrl)
                      ? "/images/default-avatar.png"
                      : m.ProfilePictureUrl,
                    ContactNumber = m.ContactNumber,
                    Address = m.Address,
                    BirthDate = m.BirthDate,
                Gender = m.Gender,
                Status = m.Status,
                DateRegistered = m.DateRegistered,
                GroupDetailsVM = m.GroupDetails.Select(gd => new MemberGroupDetailsViewModel
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
            return await _context.Members
                .Include(gd => gd.GroupDetails)
                       .ThenInclude(g => g.Group)
                .Include(m => m.User)
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
                    Email = m.User != null && m.User.Email != null
                    ? m.User.Email
                        : "No Email",
                    IsEmailConfirmed = m.User != null
                   ? m.User.EmailConfirmed
                   : false,
                    ContactNumber = m.ContactNumber,
                    Address = m.Address,
                    PhotoUrl = string.IsNullOrEmpty(m.ProfilePictureUrl)
                      ? "/images/default-avatar.png"
                      : m.ProfilePictureUrl,              
                     EventAttendedCount = attendanceCheck,
                    groupDetails = m.GroupDetails
                    .Select(g => new GroupDetailsViewModel
                    {
                        GroupName = g.Group.GroupName,
                        DateJoined = g.DateJoined
                    })
                    .ToList()
                }).FirstOrDefaultAsync();
            return member;
        }


        public async Task<OperationalResult> CreateMember(MembersCreateViewModel vm, IFormFile? photo)
        {
            var existingUser = await _userManager.FindByEmailAsync(vm.Email);
            if (existingUser != null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "This email is already registered"
                };
            }

      
                var member = new Member
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    MiddleName = vm.MiddleName,
                    ContactNumber = vm.ContactNumber,
                    Province = vm.Province,
                    Barangay = vm.Barangay,
                    
                    City = vm.City,
                    Country = vm.Country,
                    Address = BuildAddress(vm.Barangay, vm.City, vm.Province, vm.Country),
                    DateRegistered = DateTime.Now,
                    BirthDate = vm.BirthDate,
                    Gender = vm.Gender,
                    Status = vm.Status
                };

                

                _context.Members.Add(member);
                await _context.SaveChangesAsync();


                //add user account fk to user table
                var user = new User
                {
                    UserName = vm.Email,
                    Email = vm.Email,
                    DisplayName = member.FirstName + " " + member.LastName,
                    EmailConfirmed = false,
                    MemberId = member.MemberId
                };



                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(" ", createResult.Errors.Select(e => e.Description));
                    return new OperationalResult { Success = false, Message = $"Could not create account: {errors}" };
                }

                member.UserId = user.Id;
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

                //generate email token waiting for member to confirm the email = pending email 0
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return new OperationalResult
                {
                    Success = true,
                    Message = "Member added successfully!",
                    UserId = user.Id,
                    EmailConfirmationToken = token

                };

            }
           
        

        public async Task UpdateMemberAsync(MembersEditViewModel vm, IFormFile? photo)
        {
            var member = await _context.Members.FindAsync(vm.memberId);
            if (member == null) return; // or throw, depending on how you want to handle a missing record

            member.FirstName = vm.FirstName;
            member.LastName = vm.LastName;
            member.MiddleName = vm.MiddleName;
            member.User?.Email = vm.Email;
            member.ContactNumber = vm.ContactNumber;
            member.Province = vm.Province;
            member.Barangay = vm.Barangay;
            member.City = vm.City;
            member.Country = vm.Country;
            member.Address = BuildAddress( vm.Barangay, vm.City, vm.Province, vm.Country);
            member.BirthDate = vm.BirthDate;
            member.Gender = vm.Gender;
            member.Status = vm.Status;
            member.User.EmailConfirmed = vm.IsEmailConfirmed;

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




        //====== ANNOUNCEMENTS =========
        public async Task<(int? GroupId, bool IsLeader)> GetAnnouncementContextAsync(int memberId)
        {
            var leaderGroup = await _context.Groups
                .AsNoTracking().FirstOrDefaultAsync(g => 
                g.LeaderId == memberId && g.IsArchived == false);

            if(leaderGroup != null)
            {
                return (leaderGroup.GroupId, true);
            }

            var memberGroupId = await _context.GroupDetails
                .AsNoTracking()
                .Where(e => e.MemberId == memberId)
                .Select(gd => (int?)gd.GroupId).FirstOrDefaultAsync();


            return (
                    memberGroupId,
                    false
                );
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Members.Where(m => m.Status != "Inactive").CountAsync();
        }

    }
}
