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

        public async Task<ParticipationTrackingViewModel> GetParticipationTrackingAsync()
        {
            var groupParticipation = await GetGroupAsync();

            var memberParticipation = await GetMembersAsync();
            var vm = new ParticipationTrackingViewModel
            {
                Groups = groupParticipation,
                Members = memberParticipation
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

                TotalMembers =
                    memberParticipation.Count
            };

            return vm;
        }

        
        private async Task <List<GroupParticipationViewModel>> GetGroupAsync()
        {
            //update this later
            var ExistingGroup = await _context.Groups.AsNoTracking()
                      .Where(g => g.IsArchived == false && g.Status == "Active")
                     .Include(g => g.GroupDetails)
                        .ToListAsync();


            var result =
       new List<GroupParticipationViewModel>();


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
                            eg.GroupId == group.GroupId))
                    .ToListAsync();


                int fullyPresent = 0;
                int partiallyPresent = 0;
                int absent = 0;


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


                    if (presentMemberIds.Count == 0)
                    {
                        absent++;
                    }
                    else if (
                        presentMemberIds.Count ==
                        memberIds.Count)
                    {
                        fullyPresent++;
                    }
                    else
                    {
                        partiallyPresent++;
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


                result.Add(
                    new GroupParticipationViewModel
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

                        Status = status
                    });
            }


            return result;
        }


        private async Task<List<MemberParticipationViewModel>>
    GetMembersAsync()
        {
            var members = await _context.Members
                .AsNoTracking()
                .Include(m => m.GroupDetails)
                    .ThenInclude(gd => gd.Group)
                .Where(m => m.Status == "Active")
                .ToListAsync();


            var result =
                new List<MemberParticipationViewModel>();


            foreach (var member in members)
            {
                var group =
                    member.GroupDetails
                        .Select(gd => gd.Group)
                        .FirstOrDefault();


                if (group == null)
                    continue;


                var assignedEventIds =
                    await _context.Events
                        .AsNoTracking()
                        .Where(e =>
                            !e.IsArchived &&
                            e.ExpectedGroups.Any(eg =>
                                eg.GroupId == group.GroupId))
                        .Select(e => e.EventId)
                        .ToListAsync();


                var attendedEventIds =
                    await _context.Attendances
                        .AsNoTracking()
                        .Where(a =>
                            a.MemberId == member.MemberId &&
                            assignedEventIds.Contains(a.EventId) &&
                            (
                                a.Status == "Present" ||
                                a.Status == "Late"
                            ))
                        .Select(a => a.EventId)
                        .Distinct()
                        .ToListAsync();


                int assigned =
                    assignedEventIds.Count;

                int present =
                    attendedEventIds.Count;

                int absent =
                    assigned - present;


                double rate =
                    assigned == 0
                        ? 0
                        : (double)present /
                            assigned * 100;


                MemberParticipationStatus status;


                if (assigned > 0 &&
                    present == assigned)
                {
                    status =
                        MemberParticipationStatus.FullyPresent;
                }
                else if (present > 0)
                {
                    status =
                        MemberParticipationStatus.PartiallyPresent;
                }
                else
                {
                    status =
                        MemberParticipationStatus.Absent;
                }


                result.Add(
                    new MemberParticipationViewModel
                    {
                        MemberId =
                            member.MemberId,

                        FullName =
                            member.FirstName + " " +
                            member.LastName,

                        PhotoUrl =
                            member.ProfilePictureUrl,

                        GroupId =
                            group.GroupId,

                        GroupName =
                            group.GroupName,

                        EventsAssigned =
                            assigned,

                        EventsPresent =
                            present,

                        EventsAbsent =
                            absent,

                        ParticipationRate =
                            Math.Round(rate, 1),

                        TotalPoints =
                            member.EarnedPoints,

                        Status =
                            status
                    });
            }


            return result;
        }
    }
}
