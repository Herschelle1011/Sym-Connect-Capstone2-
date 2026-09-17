using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Points;
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



        public async Task<MemberPointDetailsViewModel?> GetMemberPointDetailsAsync(
    int attendanceId,
    int memberId)
        {
            // =========================================
            // 1. GET ATTENDANCE + EVENT INFORMATION
            // =========================================

            var attendance = await _context.Attendances
                .AsNoTracking()
                .Where(a =>
                    a.AttendanceId == attendanceId &&
                    a.MemberId == memberId)
                .Select(a => new
                {
                    a.AttendanceId,
                    a.EventId,
                    a.Status,
                    a.Points,
                    a.TimeIn,
                    a.TimeOut,
                    a.ScannedByUserId,

                    EventTitle = a.Event.EventTitle,

                    EventDate = a.Event.StartDate,

                    FacilityNames = a.Event.ExpectedFacilities
                        .Select(ef => ef.Facility.FacilityName)
                        .ToList()
                })
                .FirstOrDefaultAsync();


            if (attendance == null)
            {
                return null;
            }


            // =========================================
            // 2. GET THE PERSON WHO SCANNED THE MEMBER
            // =========================================

            string scannedByName = "Not available";
            string scannedByRole = "";


            if (!string.IsNullOrWhiteSpace(attendance.ScannedByUserId))
            {
                var scanner = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == attendance.ScannedByUserId)
                    .Select(u => new
                    {
                        u.Id,
                        u.DisplayName
                    })
                    .FirstOrDefaultAsync();


                if (scanner != null)
                {
                    scannedByName = scanner.DisplayName ?? "Unknown";
                }
            }


            // =========================================
            // 3. CREATE REMARK
            // =========================================

            string remarks;

            if (attendance.Status == "Present")
            {
                remarks = "Attended on time.";
            }
            else if (attendance.Status == "Late")
            {
                remarks = "Attended late.";
            }
            else if (attendance.Status == "Absent")
            {
                remarks = "Did not attend this event.";
            }
            else
            {
                remarks = "No attendance remarks.";
            }


            // =========================================
            // 4. CREATE THE VIEWMODEL
            // =========================================

            var viewModel = new MemberPointDetailsViewModel
            {
                AttendanceId = attendance.AttendanceId,

                EventId = attendance.EventId,

                EventTitle = attendance.EventTitle,

                EventDate = attendance.EventDate,

                FacilityNames = attendance.FacilityNames,

                Status = attendance.Status,

                Points = attendance.Points,

                TimeIn = attendance.TimeIn,

                TimeOut = attendance.TimeOut,

                ScannedByName = scannedByName,

                ScannedByRole = scannedByRole,

                Remarks = remarks
            };


            return viewModel;
        }

    }
}
