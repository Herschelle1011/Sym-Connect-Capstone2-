using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Profile;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Implementation
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly IMemberService _memberService;


        public ProfileService(AppDbContext context, IMemberService memberService)
        {
            _context = context;
            _memberService = memberService;
        }


        public async Task<ProfileViewModel?> GetProfileAsync(int? memberId)
        {
            if (memberId == null) return null;

           var profile = await _context.Members
                .Where(m => m.MemberId == memberId)
                .Select(m => new ProfileViewModel
                {
                    memberId = m.MemberId,
                    FirstName = m.FirstName ?? "N/A",
                    MiddleName = m.MiddleName ?? "N/A",
                    LastName = m.LastName ?? "N/A",
                    Country = m.Country,
                    Province = m.Province,
                    City = m.City,
                    Barangay = m.Barangay,
                    ContactNumber = m.ContactNumber,
                    PhotoUrl = m.User.ProfilePictureUrl,
                    Email = m.User.Email,
                    Birthdate = m.BirthDate,
                    GroupName = m.GroupDetails.Select(
                        gd=> gd.Group.GroupName).FirstOrDefault() ?? "N/A",
                    MemberSince = m.DateRegistered,

                }).FirstOrDefaultAsync();

            if (profile == null) return null;

            profile.MyQrToken = await _memberService.GetOrCreateQrTokenAsync(memberId.Value);
            profile.RecentActivities = await GetRecentActivitiesAsync(memberId.Value);


            return profile;

        }


        private async Task<List<ActivityLogViewModel>> GetRecentActivitiesAsync(int memberId, int take = 5)
        {
            // Check-ins
            var checkIns = await _context.Attendances
                .AsNoTracking()
                .Where(a => a.MemberId == memberId && a.TimeIn != null)
                .Include(a => a.Event)
                .OrderByDescending(a => a.TimeIn)
                .Take(take)
                .Select(a => new ActivityLogViewModel
                {
                    Type = ActivityType.CheckIn,
                    Description = $"Check-in at {string.Join(", ", a.Event.ExpectedFacilities.Select(ef => ef.Facility.FacilityName))} - {a.Event.EventTitle}",                    
                    Timestamp = a.TimeIn!.Value
                }).ToListAsync();

            // Group join
            var groupJoins = await _context.GroupDetails
                .Where(gd => gd.MemberId == memberId)
                .Include(gd => gd.Group)
                .OrderByDescending(gd => gd.DateJoined)
                .Select(gd => new ActivityLogViewModel
                {
                    Type = ActivityType.GroupJoin,
                    Description = $"Joined the {gd.Group.GroupName} Group",
                    Timestamp = gd.DateJoined ?? DateTime.Now
                }).ToListAsync();

            return checkIns
           
                .Concat(groupJoins)
                .OrderByDescending(a => a.Timestamp)
                .Take(take)
                .ToList();
        }
    
}
}
