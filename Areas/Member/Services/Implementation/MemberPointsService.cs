using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Points;
using Sym_Connect__Capstone2_.Data;
namespace Sym_Connect__Capstone2_.Areas.Member.Services.Implementation
{
    public class MemberPointsService : IMemberPointsService
    {
        private readonly AppDbContext _context;

        public MemberPointsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MemberPointsViewModel> GetMemberPointsAsync(int memberId)
        {
            // Get the member
            var member = await _context.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null)
            {
                return new MemberPointsViewModel();
            }


            // Get this member's attendance history
            var activities = await _context.Attendances
                .AsNoTracking()
                .Where(a => a.MemberId == memberId)
                .OrderByDescending(a => a.Event.StartDate)
                .Select(a => new MemberPointActivityViewModel
                {
                    AttendanceId = a.AttendanceId,
                    EventId = a.EventId,

                    EventTitle = a.Event.EventTitle,

                    EventDate = a.Event.StartDate,

                    FacilityName = a.Event.ExpectedFacilities
                        .Select(f => f.Facility.FacilityName)
                        .FirstOrDefault() ?? "No facility",

                    Status = a.Status,

                    Points = a.Points
                })
                .ToListAsync();


            // Build the whole page
            var viewModel = new MemberPointsViewModel
            {
                TotalPoints = member.EarnedPoints,

                AttendanceCount = activities.Count(a =>
                    a.Status == "Present" ||
                    a.Status == "Late"),

                AbsentCount = activities.Count(a =>
                    a.Status == "Absent"),

                Activities = activities
            };


            return viewModel;
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
                else
                {
                    scannedByName = "Automatically recorded";
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

