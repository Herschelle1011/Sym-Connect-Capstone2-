using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Participation;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class ParticipationTrackingService : IParticipationTrackingService
    {
        private readonly AppDbContext _context;

        public ParticipationTrackingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ParticipationTrackingViewModel> GetParticipationTrackingAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var groupParticipation = await GetGroupAsync(fromDate, toDate);

            var vm = new ParticipationTrackingViewModel
            {
                Groups = groupParticipation,
            };

            vm.Summary = new ParticipationSummaryViewModel
            {
                TotalGroups = groupParticipation.Count,

                FullyPresentGroups =
                    groupParticipation.Count(g =>
                        g.Status ==
                        GroupAttendanceStatus.FullyPresent),

                PartiallyPresentGroups =
                    groupParticipation.Count(g =>
                        g.Status ==
                        GroupAttendanceStatus.PartiallyPresent),

                AbsentGroups =
                    groupParticipation.Count(g =>
                        g.Status ==
                        GroupAttendanceStatus.Absent),

                TotalMembers = await _context.Members.CountAsync(m => m.Status == "Active")
            };

            return vm;
        }


        private async Task<List<GroupParticipationViewModel>> GetGroupAsync(DateTime? fromDate, DateTime? toDate)
        {
            //update this later
            var ExistingGroup = await _context.Groups.AsNoTracking()
                      .Where(g => g.IsArchived == false && g.Status == "Active")
                     .Include(g => g.GroupDetails)
                        .ToListAsync();


            var result = new List<GroupParticipationViewModel>();


            foreach (var group in ExistingGroup)
            {
                var memberIds = group.GroupDetails
                    .Select(gd => gd.MemberId)
                    .ToList();


                var assignedEvents = await _context.Events
                    .AsNoTracking()
                    .Where(e =>
                        !e.IsArchived &&
                        e.ExpectedGroups.Any(eg =>
                            eg.GroupId == group.GroupId) &&
                        (!fromDate.HasValue || e.StartDate.Date >= fromDate.Value.Date) &&
                        (!toDate.HasValue || e.StartDate.Date <= toDate.Value.Date))
                    .ToListAsync();


                int fullyPresent = 0;
                int partiallyPresent = 0;
                int absent = 0;

                // NEW — points tracking
                int totalPossiblePoints = 0;
                int earnedPoints = 0;


                foreach (var eventItem in assignedEvents)
                {
                    var presentMemberIds =
                        await _context.Attendances
                            .AsNoTracking()
                            .Where(a =>
                                a.EventId == eventItem.EventId &&
                                memberIds.Contains(a.MemberId) &&
                                (
                                    a.Status == "Present" ||
                                    a.Status == "Late"
                                ))
                            .Select(a => a.MemberId)
                            .Distinct()
                            .ToListAsync();


                    totalPossiblePoints += eventItem.Points;

                    if (presentMemberIds.Count == 0)
                    {
                        absent++;
                    }
                    else if (
                        presentMemberIds.Count ==
                        memberIds.Count)
                    {
                        fullyPresent++;
                        earnedPoints += eventItem.Points;
                    }
                    else
                    {
                        partiallyPresent++;
            
                        earnedPoints += eventItem.Points;
                    }
                }


                int attendedEvents =
                    fullyPresent + partiallyPresent;

                double rate =
                    assignedEvents.Count == 0
                        ? 0
                        : (double)attendedEvents /
                            assignedEvents.Count * 100;


                GroupAttendanceStatus status;


                if (assignedEvents.Count > 0 &&
                    fullyPresent == assignedEvents.Count)
                {
                    status =
                        GroupAttendanceStatus.FullyPresent;
                }
                else if (attendedEvents > 0)
                {
                    status =
                        GroupAttendanceStatus.PartiallyPresent;
                }
                else
                {
                    status =
                        GroupAttendanceStatus.Absent;
                }


                result.Add(new GroupParticipationViewModel
                {
                    GroupId = group.GroupId,

                    GroupName = group.GroupName,

                    TotalMembers = memberIds.Count,

                    EventsAssigned =
                            assignedEvents.Count,

                    FullyPresentEvents =
                            fullyPresent,

                    PartiallyPresentEvents =
                            partiallyPresent,

                    AbsentEvents =
                            absent,

                    ParticipationRate =
                            Math.Round(rate, 1),

                    Status = status,

                    // NEW
                    TotalPossiblePoints = totalPossiblePoints,
                    EarnedPoints = earnedPoints
                });
            }


            return result;
        }


        public async Task<List<MemberParticipationViewModel>> GetMembersAsync(DateTime? fromDate, DateTime? toDate, int groupId)
        {
            var members = await _context.Members
                .AsNoTracking()
                .Where(m => m.Status == "Active" && m.GroupDetails.Any(gd => gd.GroupId == groupId))
                .Select(m => new
                {
                    m.MemberId,
                    m.FirstName,
                    m.LastName,
                    m.User.ProfilePictureUrl,
                    m.EarnedPoints,
                    GroupId = groupId,


                    GroupName = m.GroupDetails.Where(gd => gd.GroupId == groupId).Select(gd => gd.Group.GroupName).FirstOrDefault()
                })
                .ToListAsync();

            if (!members.Any())
                return new List<MemberParticipationViewModel>();

            var assignedEvents = await _context.Events
                .AsNoTracking()
                .Where(e =>
                    !e.IsArchived &&
                    e.ExpectedGroups.Any(eg => eg.GroupId == groupId) &&
                    (!fromDate.HasValue || e.StartDate.Date >= fromDate.Value.Date) &&
                    (!toDate.HasValue || e.StartDate.Date <= toDate.Value.Date))
                .Select(e => new { e.EventId, e.Points })
                .ToListAsync();

            var assignedEventIds = assignedEvents.Select(e => e.EventId).ToList();
            int totalAssignedCount = assignedEvents.Count;
            int totalPossiblePoints = assignedEvents.Sum(e => e.Points);

            // 3. Fetch all attendance records for these members and events in one single go
            var memberIds = members.Select(m => m.MemberId).ToList();
            var attendanceLookup = (await _context.Attendances
                .AsNoTracking()
                .Where(a => a.MemberId.HasValue &&
                    memberIds.Contains(a.MemberId.Value) &&
                    assignedEventIds.Contains(a.EventId) &&
                    (a.Status == "Present" || a.Status == "Late"))
                .Select(a => new { a.MemberId, a.EventId })
                .ToListAsync())
                .GroupBy(a => a.MemberId)
                .ToDictionary(g => g.Key, g => g.Select(a => a.EventId).ToHashSet());

            var result = new List<MemberParticipationViewModel>();

            foreach (var member in members)
            {
                attendanceLookup.TryGetValue(member.MemberId, out var attendedSet);
                attendedSet ??= new HashSet<int>();

                int present = attendedSet.Count;
                int absent = totalAssignedCount - present;

                double rate = totalAssignedCount == 0
                    ? 0
                    : (double)present / totalAssignedCount * 100;

                int earnedEventPoints = assignedEvents
                    .Where(e => attendedSet.Contains(e.EventId))
                    .Sum(e => e.Points);

                MemberParticipationStatus status = MemberParticipationStatus.Absent;
                if (totalAssignedCount > 0 && present == totalAssignedCount)
                {
                    status = MemberParticipationStatus.FullyPresent;
                }
                else if (present > 0)
                {
                    status = MemberParticipationStatus.PartiallyPresent;
                }

                result.Add(new MemberParticipationViewModel
                {
                    MemberId = member.MemberId,
                    FullName = $"{member.FirstName} {member.LastName}",
                    PhotoUrl = member.ProfilePictureUrl,
                    GroupId = member.GroupId,
                    GroupName = member.GroupName,
                    EventsAssigned = totalAssignedCount,
                    EventsPresent = present,
                    EventsAbsent = absent,
                    ParticipationRate = Math.Round(rate, 1),
                    TotalPoints = member.EarnedPoints,
                    TotalPossiblePoints = totalPossiblePoints,
                    EarnedEventPoints = earnedEventPoints,
                    Status = status
                });
            }
            var sorted = result
                .OrderByDescending(m => m.EarnedEventPoints)
                .ToList();


            // Mark the highest scorer as top performer
            if (sorted.Any() && sorted[0].EarnedEventPoints > 0)
            {
                sorted[0].IsTopPerformer = true;
            }


            // Return the sorted list
            return sorted;
        }



    }
}