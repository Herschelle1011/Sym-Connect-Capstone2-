using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.EventModels;
using Sym_Connect__Capstone2_.Models.FacilityModels;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Attendance;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.ViewModels.Facility;
using Sym_Connect__Capstone2_.ViewModels.Group;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetPendingRequestCountAsync()
        {
            var pendingCount = await _context.Events
                .CountAsync(e => e.ApprovalStatus == ApprovalStatus.Pending);

            return pendingCount;
        }




        public async Task<Event?> GetEventByIdAsync(int eventId)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        }


        public async Task<List<EventCalendarViewModel>> GetAllEventAsync(int year, int month)
        {
            return await _context.Events
                                .Where(e => !e.IsArchived &&
                                e.StartDate.Year == year && e.StartDate.Month == month && e.ApprovalStatus == ApprovalStatus.Approved)
                                .Select(e => new EventCalendarViewModel
                                {
                                    EventId = e.EventId,
                                    EventTitle = e.EventTitle,
                                    IsAllDay = e.IsAllDay,
                                    StartDate = e.StartDate,
                                    StartTime = e.StartTime,
                                    Status = e.Status,
                                }).ToListAsync();
        }

        //FOR LEADERS VIEW (THE EVENTS THEY CREATED FOR THEIR GROUP ONLY)
        public async Task<List<EventListItemViewModel>> GetAllEventForLeaderAsync(int memberId)
        {
            return await _context.Events
                .Where(e => e.CreatedByUserId == memberId && !e.IsArchived)
                .OrderByDescending(e => e.DateCreated)
                .Select(e => new EventListItemViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    StartDate = e.StartDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    ApprovalStatus = e.ApprovalStatus,
                    DateCreated = e.DateCreated,
                    ApprovedDate = e.ApprovedDate,
                    RejectionReason = e.RejectionReason,
                    ApproveByName = e.ApprovedByUser != null ? e.ApprovedByUser.FirstName + " " + e.ApprovedByUser.LastName : null,
                    SubmittedByName = e.CreatedByUser != null ? e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName : null,
                    FacilityNames = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                    GroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList()
                }).ToListAsync();



        }

        //FOR ADMINS VIEW === > Get all pending, approve, rejected events < ==== 
        public async Task<SummaryEventsViewModel> GetEventsSummaryForAdminAsync()
        {
            var events = await _context.Events
                 .AsNoTracking()
                 .Where(e => e.CreatedByUserId != null && !e.IsArchived)
                 .OrderByDescending(e => e.DateCreated)
                 .Select(e => new EventListItemViewModel
                 {
                     EventId = e.EventId,
                     EventTitle = e.EventTitle,
                     Description = e.EventDescription ?? "No description",
                     StartDate = e.StartDate,
                     EndDate = e.EndDate,
                     StartTime = e.StartTime,
                     EndTime = e.EndTime,
                     ApprovalStatus = e.ApprovalStatus,
                     DateCreated = e.DateCreated,
                     ApprovedDate = e.ApprovedDate,
                     RejectionReason = e.RejectionReason,
                     ApproveByName = e.ApprovedByUser != null ?
                     e.ApprovedByUser.FirstName + " " + e.ApprovedByUser.LastName
                        : null,
                     SubmittedByName = e.CreatedByUser != null
                ? e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName
                : "Admin",
                     FacilityNames = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                     GroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList()
                 }).ToListAsync();

            return new SummaryEventsViewModel
            {
                TotalRequests = events.Count(),
                PendingCount = events.Count(e => e.ApprovalStatus == ApprovalStatus.Pending),
                ApprovedCount = events.Count(e => e.ApprovalStatus == ApprovalStatus.Approved),
                RejectedCount = events.Count(e => e.ApprovalStatus == ApprovalStatus.Rejected),
                Events = events
            };
        }


        public async Task<DetailsEventViewModel?> GetDetailsByAsync(int id)
        {
            var existingEvent = await _context.Events
                .Where(e => e.EventId == id && e.IsArchived == false)
                .Select(e => new DetailsEventViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    EventDescription = e.EventDescription,
                    Status = e.Status,
                    IsAllDay = e.IsAllDay,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Points = e.Points,
                    StartTime = e.StartTime,
                    //Scanner open time
                    TimeInScannerClosesTime = e.TimeInScannerClosesOffset,
                    TimeOutScannerClosesTime = e.TimeOutScannerClosesOffset,
                    TimeOutScannerOpenTime = e.TimeOutScannerOpensOffset,
                    TimeInScannerOpenTime = e.TimeInScannerOpensOffset,

                    EndTime = e.EndTime,
                    DateCreated = e.DateCreated,
                    CreatedBy = e.CreatedByUser != null
                ? e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName
                : "Admin",
                    //LIST OF FACILITIES
                    Facilities = e.ExpectedFacilities.Select(f => new ExpectedFacilityCardVM
                    {
                        FacilityId = f.FacilityId,
                        FacilityCapacity = f.Facility.Capacity,
                        FacilityName = f.Facility.FacilityName
                    }).ToList(),
                    //LIST OF GROUPS 
                    ExpectedGroups = e.ExpectedGroups.Select(g => new ExpectedGroupCardVM
                    {
                        GroupId = g.GroupId,
                        GroupName = g.Group.GroupName,
                        MemberCount = g.Group.GroupDetails.Count()
                    }).ToList(),
                    ExpectedGroupCount = e.ExpectedGroups.Count(),
                    ScheduleTimeline = new List<ScheduleTimelineVM>(),

                    //Additional for isAdmin or a Leader
                    ApprovalStatus = e.ApprovalStatus,
                    RejectionReason = e.RejectionReason,
                    ApprovedBy = e.ApprovedByUser != null ? e.ApprovedByUser.FirstName + " " + e.ApprovedByUser.LastName : null,
                    ApprovedDate = e.ApprovedDate,
                    RequestedByGroupName = e.CreatedByUser != null ? e.CreatedByUser.GroupDetails.Select(g => g.Group.GroupName).FirstOrDefault() : null,

                }).FirstOrDefaultAsync();
            if (existingEvent == null) return null;

            var selectedGroup =
                await _context.EventGroups.Where(e => e.EventId == id).Select(e => e.GroupId).ToListAsync();
            foreach (var groupIds in selectedGroup)
            {
                await MarkAsAbsentAsync(existingEvent.EventId, groupIds);
            }


            existingEvent.AttendanceRecords = await GetGroupAttendanceOverviewAsync(id);

            if (existingEvent.StartTime.HasValue && existingEvent.EndTime.HasValue)
            {
                var eventStart = existingEvent.StartTime.Value;
                var eventEnd = existingEvent.EndTime.Value;

                var timeInOpens = existingEvent.TimeInScannerOpenTime
                    ?? eventStart.Add(TimeSpan.FromMinutes(-30));

                var timeInCloses = existingEvent.TimeInScannerClosesTime
                    ?? eventStart;

                var timeOutOpens = existingEvent.TimeOutScannerOpenTime
                    ?? eventEnd.Add(TimeSpan.FromMinutes(-30));

                var timeOutCloses = existingEvent.TimeOutScannerClosesTime
                    ?? eventEnd.Add(TimeSpan.FromMinutes(30));

                var timelineEntries = new List<(TimeSpan Time, string Description)>
                 {
                    (timeInOpens, "Time-In Scanner Opens"),
                    (timeInCloses, "Time-In Scanner Closes"),
                    (eventStart, "Event Starts"),
                    (timeOutOpens, "Time-Out Scanner Opens"),
                    (eventEnd, "Event Ends"),
                    (timeOutCloses, "Time-Out Scanner Closes"),
                   };

                foreach (var entry in timelineEntries.OrderBy(e => e.Time))
                {
                    existingEvent.ScheduleTimeline.Add(new ScheduleTimelineVM
                    {
                        Time = DateTime.Today.Add(entry.Time).ToString("h:mm tt"),
                        Description = entry.Description
                    });
                }
            }

            var startDate = existingEvent.StartDate.Date + (existingEvent.StartTime ?? TimeSpan.Zero);
            var endDate = existingEvent.EndDate?.Date + (existingEvent.EndTime ?? TimeSpan.Zero);

            var now = DateTime.Now;
            EventStatus status;

            if (now < startDate)
            {
                status = EventStatus.Upcoming;
            }
            else if (now <= endDate)
            {
                status = EventStatus.Ongoing;
            }
            else
            {
                status = EventStatus.Completed;
            }

            existingEvent.Status = status;

            return existingEvent;
        }


        // TODO: [AUTH MIGRATION] createdByUserId currently comes from =========== ALERT ! ====
        // Session.GetInt32("CurrentMemberId") — an int, Members.MemberId.

        // Once Identity is implemented, this parameter should likely become a Guid
        // (AspNetUsers.Id) and the CreatedByUserId model type needs to change too.


        public async Task<EventResult> CreateEventAsync(CreateEventViewModel vm, bool isAdminCreated, int? createByUserId)
        {
            var maxLeadTime = TimeSpan.FromHours(5);
            if (vm.StartTime.HasValue && vm.TimeInScannerOpenTime.HasValue)
            {
                // Event starts: 10:00 AM
                //Scanner opens: 8:00 AM
                //              ----------
                //Difference:   2 hours

                var lead = vm.StartTime.Value - vm.TimeInScannerOpenTime.Value;
                if (lead > maxLeadTime)
                {
                    return EventResult.Fail("Scanner cannot open more than 5 hours before the event start time.");
                }
            }

            //VALIDATION FOR START DATE AND END DATE
            if (vm.StartDate == default(DateTime) || vm.EndDate == default(DateTime))
            {
                return EventResult.Fail("Please provide valid start and end dates.");
            }
            if (vm.EndDate < vm.StartDate)
            {
                return EventResult.Fail("End date must be higher than start date.");
            }

            if (vm.StartDate < DateTime.Today)
            {
                return EventResult.Fail("You cannot create an event in the past.");
            }




            var hasConflict = await HasFacilityConflictAsync(vm.SelectedFacilityIds, vm.IsAllDay, vm.StartDate.Value, vm.EndDate, vm.StartTime, vm.EndTime);
            if (hasConflict != null)
            {
                return new EventResult
                {

                    Success = false,
                    ErrorMessage = $"This facility is already booked for \"{hasConflict.EventTitle}\" during that time.",
                    ConflictingEventTitle = hasConflict.EventTitle
                };
            }
            if (!vm.IsAllDay && (vm.StartDate == null || vm.EndDate == null))
            {
                return EventResult.Fail("Start time and end time is required.");
            }


            if (vm.IsAllDay && (vm.StartTime == null || vm.EndTime == null))
            {
                vm.StartTime = null;
                vm.EndTime = null;
                vm.TimeInScannerOpenTime = null;
                vm.TimeInScannerClosesTime = null;
                vm.TimeOutScannerOpenTime = null;
                vm.TimeOutScannerClosesTime = null;
            }



            //facility validation
            var selectedFacility = await _context.Facilities
                .Where(f => vm.SelectedFacilityIds.Contains(f.FacilityId)).ToListAsync();

            foreach (var facility in selectedFacility)
            {
                if (facility.Status == FacilityStatus.UnderMaintenance
                    || facility.Status == FacilityStatus.Inactive)
                {
                    return EventResult.Fail($"Selected {facility.FacilityName} is Undermaintenance");
                }
            }


            if (!vm.IsAllDay)
            {
                // TIME-IN OPEN cannot be after event starts
                if (vm.TimeInScannerOpenTime.HasValue &&
                    vm.StartTime.HasValue &&
                    vm.TimeInScannerOpenTime.Value > vm.StartTime.Value)
                {
                    return EventResult.Fail(
                        "Time-In scanner cannot open after the event starts.");
                }

                // TIME-IN CLOSE cannot be after Time-Out scanner opens
                if (vm.TimeInScannerClosesTime.HasValue &&
                    vm.TimeOutScannerOpenTime.HasValue &&
                    vm.TimeInScannerClosesTime.Value > vm.TimeOutScannerOpenTime.Value)
                {
                    return EventResult.Fail(
                        "Time-In scanner must close before the Time-Out scanner opens.");
                }

                // TIME-IN CLOSE must be after TIME-IN OPEN
                if (vm.TimeInScannerOpenTime.HasValue &&
                    vm.TimeInScannerClosesTime.HasValue &&
                    vm.TimeInScannerOpenTime.Value >= vm.TimeInScannerClosesTime.Value)
                {
                    return EventResult.Fail(
                        "Time-In scanner closing time must be after its opening time.");
                }
            }


            var newEvent = new Event
            {
                EventTitle = vm.EventTitle,
                Points = vm.Points,
                EventDescription = vm.EventDescription,
                IsAllDay = vm.IsAllDay,
                DateCreated = DateTime.UtcNow,
                StartDate = vm.StartDate.Value,

                //TimeInOpen and TimeOutClose
                TimeInScannerOpensOffset = vm.TimeInScannerOpenTime,
                TimeInScannerClosesOffset = vm.TimeInScannerClosesTime,

                TimeOutScannerOpensOffset = vm.TimeOutScannerOpenTime,
                TimeOutScannerClosesOffset = vm.TimeOutScannerClosesTime,

                EndDate = vm.EndDate.Value,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                IsArchived = false,

                //Leaders Part
                CreatedByUserId = createByUserId,
                ApprovalStatus = isAdminCreated ? ApprovalStatus.Approved : ApprovalStatus.Pending,
                ApprovedByUserId = isAdminCreated ? createByUserId : null,
                ApprovedDate = isAdminCreated ? DateTime.Now : null,

            };

            //foreach the list selectedIds, and add
            foreach (var facilityId in vm.SelectedFacilityIds)
            {
                newEvent.ExpectedFacilities.Add(new EventFacility
                {
                    FacilityId = facilityId

                });
            }
            //foreach the list selectedGroupIds, and add
            foreach (var groupId in vm.SelectedGroupIds)
            {
                newEvent.ExpectedGroups.Add(new EventGroup
                {
                    GroupId = groupId
                });
            }
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return EventResult.Ok();
        }

        public async Task<EventResult> UpdateEventAsync(int? gId, EditEventViewModel vm)
        {
            var maxLeadTime = TimeSpan.FromHours(5);
            if (vm.StartTime.HasValue && vm.TimeInScannerOpenTime.HasValue)
            {
                // Event starts: 10:00 AM
                //Scanner opens: 8:00 AM
                //              ----------
                //Difference:   2 hours

                var lead = vm.StartTime.Value - vm.TimeInScannerOpenTime.Value;
                if (lead > maxLeadTime)
                {
                    return EventResult.Fail("Scanner cannot open more than 5 hours before the event start time.");
                }
            }


            var selectedFacility = await _context.Facilities
                       .Where(f => vm.SelectedFacilityIds.Contains(f.FacilityId)).ToListAsync();

            foreach (var facility in selectedFacility)
            {
                if (facility.Status == FacilityStatus.UnderMaintenance
                    || facility.Status == FacilityStatus.Inactive)
                {
                    return EventResult.Fail($"Selected {facility.FacilityName} is Undermaintenance");
                }
            }





            //find the ID first from the EVENT
            var existingEvent = await _context.Events.
                            Include(eg => eg.ExpectedGroups)
                              .Include(e => e.ExpectedFacilities)
                            .FirstOrDefaultAsync(e => e.EventId == gId);

            if (existingEvent == null) return EventResult.Fail("Event doesn't exist");

            var hasConflict = await HasFacilityConflictAsync(vm.SelectedFacilityIds, vm.IsAllDay, vm.StartDate.Value, vm.EndDate, vm.StartTime, vm.EndTime, excludeEventId: gId);
            if (hasConflict != null)
            {
                return EventResult.Fail($"This facility is already booked for \"{hasConflict.EventTitle}\" during that time.");
            }
            if (vm.IsAllDay && (vm.StartTime == null || vm.EndTime == null))
            {
                vm.StartTime = null;
                vm.EndTime = null;
                vm.TimeInScannerOpenTime = null;
                vm.TimeInScannerClosesTime = null;
                vm.TimeOutScannerOpenTime = null;
                vm.TimeOutScannerClosesTime = null;
            }

            if (!vm.IsAllDay)
            {
                // TIME-IN OPEN cannot be after event starts
                if (vm.TimeInScannerOpenTime.HasValue &&
                    vm.StartTime.HasValue &&
                    vm.TimeInScannerOpenTime.Value > vm.StartTime.Value)
                {
                    return EventResult.Fail(
                        "Time-In scanner cannot open after the event starts.");
                }

                // TIME-IN CLOSE cannot be after event starts
                if (vm.TimeInScannerClosesTime.HasValue &&
            vm.TimeOutScannerOpenTime.HasValue &&
            vm.TimeInScannerClosesTime.Value > vm.TimeOutScannerOpenTime.Value)
                {
                    return EventResult.Fail(
                        "Time-In scanner must close before the Time-Out scanner opens.");
                }

                // TIME-IN CLOSE must be after TIME-IN OPEN
                if (vm.TimeInScannerOpenTime.HasValue &&
                    vm.TimeInScannerClosesTime.HasValue &&
                    vm.TimeInScannerOpenTime.Value >= vm.TimeInScannerClosesTime.Value)
                {
                    return EventResult.Fail(
                        "Time-In scanner closing time must be after its opening time.");
                }
            }

            existingEvent.EventTitle = vm.EventTitle;
            existingEvent.EventDescription = vm.EventDescription;
            existingEvent.StartDate = vm.StartDate.Value;
            existingEvent.EndDate = vm.EndDate.Value;
            existingEvent.Points = vm.Points;
            existingEvent.StartTime = vm.StartTime;
            existingEvent.TimeOutScannerClosesOffset = vm.TimeOutScannerClosesTime;
            existingEvent.TimeInScannerClosesOffset = vm.TimeInScannerClosesTime;
            existingEvent.IsAllDay = vm.IsAllDay;
            existingEvent.EndTime = vm.EndTime;
            existingEvent.TimeInScannerOpensOffset = vm.TimeInScannerOpenTime;
            existingEvent.TimeOutScannerOpensOffset = vm.TimeOutScannerOpenTime;



            var currentFacilityIds = existingEvent.ExpectedFacilities.Select(ef => ef.FacilityId).ToList();

            var facilitiesToRemove = existingEvent.ExpectedFacilities
                .Where(ef => !vm.SelectedFacilityIds.Contains(ef.FacilityId)).ToList();

            var facilityIdsToAdd = vm.SelectedFacilityIds
                .Where(selectedId => !currentFacilityIds.Contains(selectedId));

            foreach (var remove in facilitiesToRemove)
                existingEvent.ExpectedFacilities.Remove(remove);

            foreach (var facilityIdToAdd in facilityIdsToAdd)
                existingEvent.ExpectedFacilities.Add(new EventFacility { FacilityId = facilityIdToAdd });



            //show all currentGroup in the selected event
            var currentGroupIds = existingEvent.ExpectedGroups.Select(eg => eg.GroupId).ToList();

            //only remove those group that were selected by the admin KEEP those unselected groups
            var groupRemove = existingEvent.ExpectedGroups
                                .Where(eg => !vm.SelectedGroupIds.Contains(eg.GroupId)).ToList();
            //does selectedgroups selected by admin contains any groupId already in the list? 

            // ---  DATE : 7/12/2026 ------ NAG LIBOG DIRI NGA PART BALIK DIRI ---- 
            //things to keep in mind : vm.SelectedGroupIds list of INTS selected by admin -------
            //ExpectedGroups is the object of <EventGroup> to loads object of groupId or EventId CHECK MODEL


            //select group that were not existed yet in selectedGroupIds ONLY SHOW notCurrentGroupIds
            var groupIdsToAdd = vm.SelectedGroupIds
                            .Where(selectedId => !currentGroupIds.Contains(selectedId));

            foreach (var remove in groupRemove)
                existingEvent.ExpectedGroups.Remove(remove);

            foreach (var groupIdToAdd in groupIdsToAdd)
            {
                //ExpectedGroups expects an EventGroup objects create one using New EventGroup fill it up with ViewModel
                //groupIdsToAdd is only a list of groups that it isnt in the Event(id) yet
                existingEvent.ExpectedGroups.Add(new EventGroup {
                    GroupId = groupIdToAdd
                });
            }
            await _context.SaveChangesAsync();
            return EventResult.Ok();
        }

        public async Task<EditEventViewModel?> GetEventForEditAsync(int? id)
        {
            var existingEvent = await _context.Events.
                    Include(eg => eg.ExpectedGroups)
                    .Include(f => f.ExpectedFacilities)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (existingEvent == null) return null;

            return new EditEventViewModel
            {

                EventId = existingEvent.EventId,
                EventTitle = existingEvent.EventTitle,
                EventDescription = existingEvent.EventDescription ?? "N/A",
                IsAllDay = existingEvent.IsAllDay,
                StartDate = existingEvent.StartDate,
                EndDate = existingEvent.EndDate,
                Points = existingEvent.Points,
                TimeInScannerClosesTime = existingEvent.TimeInScannerClosesOffset,
                TimeOutScannerClosesTime = existingEvent.TimeOutScannerClosesOffset,
                TimeInScannerOpenTime = existingEvent.TimeInScannerOpensOffset,
                TimeOutScannerOpenTime = existingEvent.TimeOutScannerOpensOffset,



                StartTime = existingEvent.StartTime,
                EndTime = existingEvent.EndTime,
                SelectedFacilityIds = existingEvent.ExpectedFacilities.Select(f => f.FacilityId).ToList(),
                SelectedGroupIds = existingEvent.ExpectedGroups.Select(eg => eg.GroupId).ToList()
            };
        }

        public async Task<List<ExpectedFacilityCardVM>> GetFacilitiesForModalAsync(int? id)
        {
            if (id == null) return new List<ExpectedFacilityCardVM>();

            var existingEvent = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == id.Value)
                .SelectMany(e => e.ExpectedFacilities)
                .Select(ef => new ExpectedFacilityCardVM
                {
                    FacilityId = ef.FacilityId,
                    FacilityName = ef.Facility.FacilityName,
                    FacilityCapacity = ef.Facility.Capacity
                }).ToListAsync();


            return existingEvent;
        }

        public async Task<List<ExpectedGroupCardVM>> GetGroupsForModalAsync(int? id)
        {
            if (id == null) return new List<ExpectedGroupCardVM>();

            var existingEvent = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == id.Value)
                .SelectMany(e => e.ExpectedGroups)
                .Select(eg => new ExpectedGroupCardVM
                {
                    GroupId = eg.GroupId,
                    GroupName = eg.Group.GroupName,
                    MemberCount = eg.Group.GroupDetails.Count()
                }).ToListAsync();

            return existingEvent;
        }




        public async Task<ArchiveViewModel?> ConfirmationArchiveAsync(int? eventId)
        {
            if (eventId == null) return null;
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (existingEvent == null) return null;
            return new ArchiveViewModel
            {
                EventId = existingEvent.EventId,
                EventTitle = existingEvent.EventTitle,
                Description = existingEvent.EventDescription ?? "N/A",
                StartDate = existingEvent.StartDate,
                FacilityId = existingEvent.ExpectedFacilities.Select(f => f.FacilityId).ToList(),
            };
        }

        //POST
        public async Task ArchiveEventAsync(int id)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
            if (existingEvent == null) return;
            existingEvent.IsArchived = true;
            existingEvent.ArchiveDate = DateTime.Now;
            await _context.SaveChangesAsync();

        }

        private (DateTime StartDate, DateTime EndDate) EventRange(bool IsAllDay, DateTime startDate, DateTime? endDate, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (IsAllDay)
            {
                return (startDate.Date, endDate.Value.Date.AddDays(1));
            }
            return (startDate.Date + (startTime ?? TimeSpan.Zero), endDate.Value.Date + (endTime ?? TimeSpan.Zero));
        }

        //understand this part >>>> to be check....
        public async Task<Event?> HasFacilityConflictAsync(List<int>? facilityId, bool isAllDay, DateTime startDate,
                                        DateTime? endDate, TimeSpan? startTime, TimeSpan? endTime, int? excludeEventId = null)
        {
            var (newStart, newEnd) = EventRange(isAllDay, startDate, endDate, startTime, endTime);
            //list of those events in the same DATE
            var possibleEventConflicts = await _context.Events.Where(e =>
                                        e.ExpectedFacilities.Any(ef => facilityId.Contains(ef.FacilityId))
                                        && e.StartDate.Date <= endDate.Value.Date
                                        && e.EndDate.Date >= startDate.Date
                                        && (excludeEventId == null || e.EventId != excludeEventId)).ToListAsync();
            //checks the list 
            foreach (var existing in possibleEventConflicts)
            {
                var (existingStart, existingEnd) = EventRange
                    (existing.IsAllDay, existing.StartDate, existing.EndDate, existing.StartTime, existing.EndTime);

                if (newStart < existingEnd && existingStart < newEnd) return existing;
            }
            return null;
        }



        //============ QR SCANNER PART CODE ==============


        public async Task<ScannerViewModel> GetScannerStateAsync(int eventId)
        {

            var scannerState = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == eventId)
                .Select(e => new
                {
                    e.EventTitle,
                    TotalMembersCount = e.ExpectedGroups
                        .SelectMany(eg => eg.Group.GroupDetails.Select(gd => gd.MemberId))
                        .Distinct()
                        .Count()
                })
                .FirstOrDefaultAsync();

            if (scannerState == null) return null;

            var participants = await _context.Attendances
                .AsNoTracking()
                .Where(a => a.EventId == eventId)
                .OrderByDescending(a => a.TimeIn)
                .Select(a => new ParticipantCheckInViewModel
                {
                    MemberId = a.MemberId.Value,
                    FullName = a.Member.FullName,
                    PhotoUrl = a.Member.ProfilePictureUrl,
                    GroupName = a.Member.GroupDetails
                         .Where(gd => _context.Events
                            .Where(e => e.EventId == eventId)
                            .SelectMany(e => e.ExpectedGroups)
                            .Any(eg => eg.GroupId == gd.GroupId))
                        .Select(gd => gd.Group.GroupName)
                        .FirstOrDefault(),
                    CheckedInAt = a.TimeIn,
                    CheckedOutAt = a.TimeOut
                })
                .ToListAsync();

            return new ScannerViewModel
            {
                EventId = eventId,
                EventName = scannerState.EventTitle,
                TotalMembersCount = scannerState.TotalMembersCount,
                PresentCount = participants.Where(p => p.CheckedOutAt.HasValue)
                .Select(p => p.MemberId).Distinct().Count(),
                Participants = participants
            };
        }









        public async Task<OperationalResult> RecordTimeInAsync(string qrToken, int eventId)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.QRToken == qrToken);
            if (member == null)
            {
                return new OperationalResult { Success = false, Message = "QR code not recognized." };
            }

            var EventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!EventExists)
            {
                return new OperationalResult { Success = false, Message = "Event not found." };
            }

            var ExistingRecord = await _context.Attendances.FirstOrDefaultAsync(m => m.MemberId == member.MemberId
            && m.EventId == eventId);


            if (ExistingRecord != null)
            {
                return new OperationalResult { Success = false, Message = $"{member.FirstName} {member.LastName} already Checked In" };
            }

            var expectedGroupIds = await _context.Events
          .AsNoTracking()
          .Where(e => e.EventId == eventId)
          .SelectMany(e => e.ExpectedGroups)
          .Select(eg => eg.GroupId)
          .ToListAsync();

            var eventEntity = await _context.Events
            .AsNoTracking()
            .Where(e => e.EventId == eventId)
            .Select(e => new { e.Points })
            .FirstOrDefaultAsync();


            var memberGroupIds = await _context.GroupDetails
            .Where(gd => gd.MemberId == member.MemberId)
            .Select(gd => gd.GroupId)
            .ToListAsync();

            if (memberGroupIds == null) return new OperationalResult { Success = false, Message = "Event not found." };

            bool isExpected = expectedGroupIds.Any(id => memberGroupIds.Contains(id));

            if (!isExpected)
                return new OperationalResult { Success = false, Message = $"{member.FirstName} is not part of the expected groups for this event." };


            _context.Attendances.Add(new Models.Attendance
            {
                MemberId = member.MemberId,
                EventId = eventId,
                TimeIn = DateTime.Now,
                TimeOut = null,
                Status = "Pending"
            });

            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = $"{member.FirstName} Checked in Successfully." };
        }


        public async Task MarkAsAbsentAsync(int eventId, int groupId)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (existingEvent == null) return;
            if (!existingEvent.EndTime.HasValue)
            {
                return;
            }

            var timeOutCloses = existingEvent.TimeOutScannerClosesOffset ??
                existingEvent.EndTime.Value.Add(TimeSpan.FromMinutes(30));
            var scannerCloseDateTime = existingEvent.EndDate.Date + timeOutCloses;

            if (DateTime.Now <= scannerCloseDateTime) return;


            var groupsBelongstoEvent = await _context.Events.AnyAsync(e => e.EventId == eventId &&
            e.ExpectedGroups.Any(g => g.GroupId == groupId));

            if (!groupsBelongstoEvent)
            {
                return;
            }

            //fetch all members belong to this specific group
            var groupMembers = await _context.Groups
                .Where(g => g.GroupId == groupId)
                .SelectMany(g => g.GroupDetails)
                .Select(m => m.MemberId).ToListAsync();


            //fetch attendance records for these members assigned from this event
            var existingAttendance = await
              _context.Attendances.Where(a => a.EventId == eventId && groupMembers.Contains(a.MemberId))
              .ToListAsync();

            var now = DateTime.Now;

            foreach (var memberId in groupMembers)
            {
                var attendance = existingAttendance.FirstOrDefault(a => a.MemberId == memberId);
                if (attendance != null)
                {
                    if (attendance.Status == "Pending" && !attendance.TimeIn.HasValue)
                        attendance.Status = "Absent";

                }
                else
                {
                    _context.Attendances.Add(new Attendance
                    {
                        EventId = eventId,
                        MemberId = memberId,
                        Status = "Absent"
                    });
                }
            }

            await _context.SaveChangesAsync();

        }


        public async Task<OperationalResult> RecordTimeOutAsync(string qrToken, int eventId)
        {
            var member = await _context.Members
              .FirstOrDefaultAsync(m => m.QRToken == qrToken);
            if (member == null)
            {
                return new OperationalResult { Success = false, Message = "QR code not recognized." };
            }

            var eventExists = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (eventExists == null)
                return new OperationalResult { Success = false, Message = "Event not found." };


            var MemberhasTimeIn = await _context.Attendances.FirstOrDefaultAsync(m => m.MemberId == member.MemberId && m.EventId == eventId);

            if (MemberhasTimeIn == null)
            {
                return new OperationalResult { Success = false, Message = $"{member.FirstName} never checked in — cannot record time-out." };
            }

            if (MemberhasTimeIn.TimeOut.HasValue)
                return new OperationalResult { Success = false, Message = $"{member.FirstName} already timed out." };

            var latePeriod = TimeSpan.FromMinutes(15);

            if (eventExists.StartTime.HasValue && MemberhasTimeIn.TimeIn.HasValue)
            {
                var EventStartDateTime = eventExists.StartDate.Date + eventExists.StartTime.Value;
                var late = EventStartDateTime + latePeriod;

                if (MemberhasTimeIn.TimeIn.Value > late)
                {
                    MemberhasTimeIn.Points = (eventExists.Points) - 2;
                    member.EarnedPoints += MemberhasTimeIn.Points;
                    MemberhasTimeIn.Status = "Late";
                }
                else
                {

                    MemberhasTimeIn.Points = eventExists.Points;
                    member.EarnedPoints += MemberhasTimeIn.Points;
                    MemberhasTimeIn.Status = "Present";
                }
            }

            MemberhasTimeIn.TimeOut = DateTime.Now;
            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = $"{member.FirstName} Timed Out Successfully." };

        }

        //===================== ATTENDANCE PART ================== 

        public async Task<List<AttendanceRowViewModel>> ViewAttendanceForEventAsync(int eventId)
        {
            var eventTiming = await _context.Events.AsNoTracking()
                            .Where(e => e.EventId == eventId)
                            .Select(e => new { e.StartDate, e.StartTime })
                            .FirstOrDefaultAsync();

            if (eventTiming == null) return new List<AttendanceRowViewModel>();

            var rows = await _context.Attendances
                .AsNoTracking()
                .Where(a => a.EventId == eventId)
                .Select(a => new AttendanceRowViewModel
                {
                    MemberId = a.MemberId,
                    FullName = a.Member.FullName,
                    PhotoUrl = a.Member.ProfilePictureUrl,
                    Points = a.Points,
                    GroupName = a.Member.GroupDetails.Select(gd => gd.Group.GroupName).FirstOrDefault() ?? "-",
                    TimeIn = a.TimeIn,
                    TimeOut = a.TimeOut,
                    Status = a.Status
                })
                .ToListAsync();

            return rows;
        }

        //==== SEARCH PANEL PART ==== 
        public async Task<List<EventSearchResultViewModel>> SearchPanelAsync(string search)
        {
            var existingEvents = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventTitle.Contains(search))
                .Select(e => new EventSearchResultViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    IsAllDay = e.IsAllDay,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Facilities = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                }).ToListAsync();

            return existingEvents;
        }


        //==== VIEW AVAILABLE FACILITIES ==== 
        public async Task<List<ViewAvailableFacilitiesViewModel>> GetAvailableFacilitiesAsync()
        {
            DateTime today = DateTime.Today;
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            DateTime startOfNextMonth = startOfMonth.AddMonths(1);

            var facilities = await _context.Facilities
                .AsNoTracking()
                     .Include(f => f.FacilityPhotos)
                .Where(f =>
                    f.Status != FacilityStatus.UnderMaintenance &&
                    f.Status != FacilityStatus.Inactive &&
                    f.IsArchived != true)
                .ToListAsync();

            var result = new List<ViewAvailableFacilitiesViewModel>();



            foreach (var facility in facilities)
            {
                var upcoming = await _context.Events
                   .AsNoTracking()
                   .Where(e =>
                       e.ExpectedFacilities.Any(f => f.FacilityId == facility.FacilityId) &&
                       e.StartDate.Date >= today)
                   .OrderBy(e => e.StartDate)
                   .Take(3)
                   .Select(e => new { e.StartDate, e.EventTitle })
                   .ToListAsync();

                var upcomingList = upcoming
                 .Select(e => (Date: e.StartDate, Title: e.EventTitle))
                 .ToList();

                bool hasEventToday = await _context.Events
                    .AsNoTracking()
                    .AnyAsync(e =>
                        e.ExpectedFacilities.Any(f => f.FacilityId == facility.FacilityId) &&
                        e.StartDate.Date <= today &&
                        e.EndDate >= today
                    );

                int reservationsThisMonth = await _context.Events
                  .AsNoTracking()
                  .CountAsync(e =>
                      e.ExpectedFacilities.Any(f => f.FacilityId == facility.FacilityId) &&
                      e.StartDate.Date >= today &&
                      e.StartDate < startOfNextMonth
                  );

                result.Add(new ViewAvailableFacilitiesViewModel
                {
                    FacilityId = facility.FacilityId,
                    FacilityName = facility.FacilityName,
                    FacilityDescription = facility.FacilityDescription,
                    FacilityType = facility.FacilityTypes,
                    Capacity = facility.Capacity,
                    UpcomingReservations = upcomingList,
                    Status = hasEventToday ? "Limited today" : "Available now",
                    ReservationCountThisMonth = reservationsThisMonth,
                    Photos = facility.FacilityPhotos
                        .Select(p => p.PhotoUrl)
                        .ToList()
                });
            }
            return result;
        }




        //==== FACILITY SCHEDULE ====
        public async Task<FacilityScheduleViewModel?> GetFacilityScheduleAsync(int facilityId, int year, int month)
        {
            var facility = await _context.Facilities
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FacilityId == facilityId);

            if (facility == null) return null;

            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var events = await _context.Events
                .AsNoTracking()
                .Where(e =>
                    e.ExpectedFacilities.Any(f => f.FacilityId == facilityId) &&
                    e.StartDate < monthEnd &&
                    e.EndDate >= monthStart)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return new FacilityScheduleViewModel
            {
                FacilityId = facility.FacilityId,
                FacilityName = facility.FacilityName,
                Year = year,
                Month = month,
                Events = events.Select(e => new FacilityScheduleEventViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    Date = e.StartDate,
                    IsAllDay = e.IsAllDay,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime
                }).ToList()
            };
        }





        // ======================================================= LEADERS BELOW CODES =======================================//


        // ============ LEADERS METHOD APPROVE AND REJECT =========

        // TODO: [AUTH MIGRATION] approvedByUserId is Members.MemberId (int) for now.


        public async Task<EventResult> ApproveEventAsync(int eventId, int? approveByUserId)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (existingEvent == null) return new EventResult { Success = false, ErrorMessage = "Event doesn't exist" };

            existingEvent.ApprovalStatus = ApprovalStatus.Approved;
            existingEvent.ApprovedDate = DateTime.Now;
            existingEvent.ApprovedByUserId = approveByUserId;

            await _context.SaveChangesAsync();
            return EventResult.Ok();
        }

        // TODO: [AUTH MIGRATION] approvedByUserId is Members.MemberId (int) for now.

        public async Task<EventResult> RejectEventAsync(int eventId, string reason)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (existingEvent == null) return new EventResult { Success = false, ErrorMessage = "Event doesn't exist" };

            existingEvent.ApprovalStatus = ApprovalStatus.Rejected;

            existingEvent.RejectionReason = reason;

            await _context.SaveChangesAsync();
            return EventResult.Ok();
        }



        public async Task<AttendanceTabViewModel> GetGroupAttendanceOverviewAsync(int eventId)
        {
            //for header
            var eventInfo = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == eventId)
                .Select(e => new { e.EventId, e.StartDate, e.StartTime, e.EndTime, e.EventTitle })
                .FirstOrDefaultAsync();
            if (eventInfo == null) return null;

            //get expectedGroups
            var expectedGroupIds = await _context.Events
                .Where(e => e.EventId == eventId)
                .SelectMany(g => g.ExpectedGroups)
                .Select(g => g.GroupId)
                .ToListAsync();

            //get groups along with its Members convert into a list
            var groups = await _context.Groups
                          .Where(e => expectedGroupIds.Contains(e.GroupId))
                          .Select(g => new
                          {
                              g.GroupId,
                              g.GroupName,
                              MemberIds = g.GroupDetails.Select(g => g.MemberId).ToList()
                          }).ToListAsync();


            var presentMemberIds = await _context.Attendances
                .Where(t => t.EventId == eventId && (t.Status == "Present" || t.Status == "Late"))
                .Select(m => m.MemberId)
                .ToListAsync();

            var presentSet = presentMemberIds.ToHashSet();

            var attendanceStatuses = await _context.Attendances
    .Where(a => a.EventId == eventId)
    .Select(a => new { a.MemberId, a.Status })
    .ToListAsync();

            var statusByMember = attendanceStatuses
                .Where(a => a.MemberId.HasValue)
                .ToDictionary(a => a.MemberId!.Value, a => a.Status);

            var summaries = groups.Select(g =>
            {
                var total = g.MemberIds.Count;
                var present = g.MemberIds.Count(id => id.HasValue && statusByMember.TryGetValue(id.Value, out var s) && (s == "Present" || s == "Late"));


                var Confirmedabsent = g.MemberIds.Count(id => id.HasValue && statusByMember.TryGetValue(id.Value, out var s) && s == "Absent");
                var rate = total == 0 ? 0 : (int)Math.Round((double)present / total * 100);

                var pending = present - Confirmedabsent;
                GroupAttendanceStatus Status;
                if (total == 0) Status = GroupAttendanceStatus.Absent;
                else if (present == total) Status = GroupAttendanceStatus.FullyPresent;
                else if (present > 0) Status = GroupAttendanceStatus.PartiallyPresent;
                else if (Confirmedabsent == total) Status = GroupAttendanceStatus.Absent;
                else Status = GroupAttendanceStatus.Pending;

                return new GroupAttendanceRowViewModel
                {
                    GroupId = g.GroupId,
                    GroupName = g.GroupName,
                    TotalMembers = total,
                    PresentCount = present,
                    AbsentCount = Confirmedabsent,
                    AttendanceRate = rate,
                    Status = Status
                };
            }).ToList();

            return new AttendanceTabViewModel
            {
                EventId = eventId,
                EventTitle = eventInfo.EventTitle,
                EventDate = eventInfo.StartDate,
                StartTime = eventInfo.StartTime,
                EndTime = eventInfo.EndTime,
                GroupAttendance = summaries,
                TotalGroups = summaries.Count,
                FullyPresentCount = summaries.Count(s => s.Status == GroupAttendanceStatus.FullyPresent),
                PartiallyPresentCount = summaries.Count(s => s.Status == GroupAttendanceStatus.PartiallyPresent),
                AbsentCount = summaries.Count(s => s.Status == GroupAttendanceStatus.Absent)
            };

        }


        public async Task<GroupAttendanceDetailsViewModel> GroupAttendanceDetailsView(int groupId, int eventId)
        {
            var groups = await _context.EventGroups
                .AsNoTracking()
                .Where(e => e.GroupId == groupId && e.EventId == eventId && !e.Group.IsArchived)
                .Select(g => new GroupAttendanceDetailsViewModel
                {
                    groupId = g.GroupId,
                    GroupName = g.Group.GroupName,
                    eventTitle = g.Event.EventTitle,
                    EventDate = g.Event.StartDate,
                    StartTime = g.Event.StartTime,
                    EndTime = g.Event.EndTime,
                    GroupLeader = g.Group.LeaderId != null ? g.Group.Leader.FirstName + " " + g.Group.Leader.LastName : "No Leader",
                    TotalMembers = g.Group.GroupDetails.Count(),
                    GroupAttendanceStatus = !g.Group.GroupDetails.Any() ? GroupAttendanceStatus.Absent :
                    g.Group.GroupDetails.All(gd => _context.Attendances.Any(a => a.MemberId == gd.MemberId && a.EventId == g.EventId
                                                                                  && a.Status == "Present"))
                                                    ? GroupAttendanceStatus.FullyPresent : g.Group.GroupDetails
                                                    .Any(gd => _context.Attendances.Any(a => a.MemberId == gd.MemberId
                                                          && a.EventId == g.EventId
                                                          && a.Status == "Present")) ? GroupAttendanceStatus.PartiallyPresent : GroupAttendanceStatus.Absent,

                }).FirstOrDefaultAsync();

            if (groups == null) return null;

            // Need the raw group members separately since they're not on the view model
            var groupMembers = await _context.GroupDetails
                .Where(gd => gd.GroupId == groupId)
                .Select(gd => new
                {
                    gd.MemberId,
                    FullName = gd.Member.FirstName + " " + gd.Member.LastName,
                    gd.Member.ProfilePictureUrl
                })
                .ToListAsync();

            var attendanceLookup = await _context.Attendances
                 .Where(a => a.EventId == eventId)
          .ToDictionaryAsync(a => a.MemberId, a => a);


            groups.attendanceRowViewModels = groupMembers.Select(m =>
            {
                attendanceLookup.TryGetValue(m.MemberId, out var att);
                return new AttendanceRowViewModel
                {
                    MemberId = m.MemberId,
                    FullName = m.FullName,
                    PhotoUrl = m.ProfilePictureUrl,
                    GroupName = groups.GroupName,
                    TimeIn = att?.TimeIn,
                    TimeOut = att?.TimeOut,
                    Points = att?.Points ?? 0,
                    Status = att?.Status ?? "Pending",
                };
            }).ToList();

            const int graceMinutes = 15;
            foreach (var row in groups.attendanceRowViewModels)
            {
                if (row.Status == "Absent")
                {
                    row.Remarks = "Absent";
                }
                else if (row.TimeIn.HasValue && groups.StartTime.HasValue)
                {
                    var lateBy = row.TimeIn.Value.TimeOfDay - groups.StartTime.Value;
                    row.Remarks = lateBy.TotalMinutes > graceMinutes
                        ? $"Late by {(int)lateBy.TotalMinutes} mins"
                        : "On time";
                }
                else
                {
                    row.Remarks = "Never attended";
                }
            }
            return groups;
        }


        public async Task<bool> LeaderOwnsEventsAsync(int eventId, int LeaderMemberId)
        {
            return await _context.Events.Where(g => g.EventId == eventId)
                        .AnyAsync(e => e.CreatedByUserId == LeaderMemberId
                        || e.ExpectedGroups.Any(e => e.Group.LeaderId == LeaderMemberId));
        }
    }
}
