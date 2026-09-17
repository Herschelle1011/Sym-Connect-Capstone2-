using Humanizer;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Events;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.EventModels;
using Sym_Connect__Capstone2_.Models.FacilityModels;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Attendance;
using Sym_Connect__Capstone2_.ViewModels.Dashboard;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.ViewModels.Facility;
using Sym_Connect__Capstone2_.ViewModels.Group;
using Sym_Connect__Capstone2_.ViewModels.Notification;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEvaluationService _evaluationService;



        public EventService(AppDbContext context, INotificationService notificationService, IEvaluationService evaluationService)
        {
            _context = context;
            _notificationService = notificationService;
            _evaluationService = evaluationService;
        }

        public async Task<int> GetPendingRequestCountAsync()
        {
            var pendingCount = await _context.Events
            .CountAsync(e => !e.IsArchived && e.ApprovalStatus == ApprovalStatus.Pending);
            return pendingCount;
        }


        public async Task<int> GetApprovedCountAsync()
        {
            var approvedCount = await _context.Events
         .CountAsync(e => !e.IsArchived && !e.IsCancelled && e.ApprovalStatus == ApprovalStatus.Approved);
            return approvedCount;
        }

        public async Task<int> GetRejectedCountAsync()
        {
            var rejectedCount = await _context.Events
        .CountAsync(e => !e.IsArchived && e.ApprovalStatus == ApprovalStatus.Rejected);
            return rejectedCount;
        }




        public async Task<Event?> GetEventByIdAsync(int eventId)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        }


        public async Task<List<EventCalendarViewModel>>
    GetAllEventAsync(int year, int month)
        {
            var startOfMonth =
                new DateTime(year, month, 1);

            var startOfNextMonth =
                startOfMonth.AddMonths(1);


            return await _context.Events
                .AsNoTracking()
                .Where(e =>
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus ==
                        ApprovalStatus.Approved &&
                    e.StartDate >= startOfMonth &&
                    e.StartDate < startOfNextMonth)
                .Select(e => new EventCalendarViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    IsAllDay = e.IsAllDay,
                    StartDate = e.StartDate,
                    StartTime = e.StartTime,
                    Status = e.Status
                })
                .ToListAsync();
        }

        //FOR LEADERS VIEW (THE EVENTS THEY CREATED FOR THEIR GROUP ONLY)
        public async Task<List<EventListItemViewModel>> GetAllEventForLeaderAsync(int memberId)
        {
            return await _context.Events
                .AsNoTracking()
                    .AsSplitQuery()
                .Where(e => e.CreatedByUserId == memberId && !e.IsArchived && !e.IsCancelled)
                .OrderByDescending(e => e.DateCreated)
                .Select(e => new EventListItemViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    StartDate = e.StartDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    ExpiredDate = e.ExpiredDate,
                    ApprovalStatus = e.ApprovalStatus,
                    DateCreated = e.DateCreated,
                    ApprovedDate = e.ApprovedDate,
                    RejectionReason = e.RejectionReason,
                    CreatedByName = e.CreatedByUser != null
                    ? e.CreatedByUser.FullName
                    : "Admin",
                    ApproveByName = e.ApprovedByUser != null ? e.ApprovedByUser.FirstName + " " + e.ApprovedByUser.LastName : null,
                    SubmittedByName = e.CreatedByUser != null ? e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName : null,
                    FacilityNames = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                    GroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList()
                }).ToListAsync();
        }

        public async Task<List<EventListItemViewModel>> GetAllEventForEvaluationAsync(int currentUserId, bool IsAdmin)
        {
            var now = DateTime.Now;
            var today = now.Date;
            var currentTime = now.TimeOfDay;

            
            var query = _context.Events
             .Where(e =>
          !e.IsArchived &&
          !e.IsCancelled &&
          e.ApprovalStatus == ApprovalStatus.Approved &&
                            (e.EndDate < today || (
                              e.EndDate == today &&
                              e.EndTime.HasValue &&
                              e.EndTime.Value < currentTime)));

            if (!IsAdmin)
            {
                query = query.Where(e => e.CreatedByUserId == currentUserId);
            }

            return await query
               .OrderByDescending(e => e.EndDate)
               .ThenByDescending(e => e.EndTime)
               .Select(e => new EventListItemViewModel
               {
                   EventId = e.EventId,
                   EventTitle = e.EventTitle,
                   StartDate = e.StartDate,
                   StartTime = e.StartTime,
                   EndTime = e.EndTime,
                   EndDate = e.EndDate,
                   ApprovalStatus = e.ApprovalStatus,
                   DateCreated = e.DateCreated,
                   ApprovedDate = e.ApprovedDate,
                   RejectionReason = e.RejectionReason,
                   ApproveByName =
               e.ApprovedByUser != null
                   ? e.ApprovedByUser.FirstName + " " + e.ApprovedByUser.LastName
                   : null,
                   SubmittedByName =
               e.CreatedByUser != null
                   ? e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName
                   : null,
                   FacilityNames =
               e.ExpectedFacilities
                   .Select(f => f.Facility.FacilityName)
                   .ToList(),
                   GroupNames =
                      e.ExpectedGroups
                   .Select(g => g.Group.GroupName)
                   .ToList()
               }).ToListAsync();
        }

        //FOR ADMINS VIEW === > Get all pending, approve, rejected events < ==== 
        public async Task<SummaryEventsViewModel> GetEventsSummaryForAdminAsync()
        {
            var events = await _context.Events
                 .AsNoTracking()
                     .AsSplitQuery()

                 .Where(e => e.CreatedByUserId != null && !e.IsArchived && !e.IsCancelled)
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
                     ExpiredDate = e.ExpiredDate,
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
                ExpiredCount = events.Count(e => e.ApprovalStatus == ApprovalStatus.Expired),
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
                 ?? eventEnd;

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

            var rating = 
            existingEvent.EvaluationRating =
                await _evaluationService.GetEventFinalRatingAsync(id);

            Console.WriteLine($"EVENT ID: {id}");
            Console.WriteLine($"RATING: {rating}");


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

                // TIME-IN CLOSE cannot be after TIME-OUT OPEN
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
                // TIME-OUT OPEN must be exactly equal to EVENT END TIME
                if (vm.TimeOutScannerOpenTime.HasValue &&
                    vm.EndTime.HasValue &&
                    vm.TimeOutScannerOpenTime.Value != vm.EndTime.Value)
                {
                    return EventResult.Fail(
                        "Time-Out scanner opening time must be exactly the same as the event end time.");
                }

                if (vm.TimeOutScannerOpenTime.HasValue &&
                vm.TimeOutScannerClosesTime.HasValue &&
                vm.TimeOutScannerClosesTime.Value <= vm.TimeOutScannerOpenTime.Value)
                {
                    return EventResult.Fail(
                        "Time-Out scanner closing time must be after its opening time.");
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

                TimeOutScannerOpensOffset = vm.EndTime,
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

            //NOTIFICATION 
            if (!isAdminCreated)
            {
                await _notificationService.NotifyAdminAsync(
                  new NotificationViewModel
                  {
                      Title = "New Event Request",
                      Message = $"A new event request \"{newEvent.EventTitle}\" has been submitted.",
                      Url = $"/Events/ViewEventsRequests?openEventId={newEvent.EventId}"
                  }
              );
            }

            //NOTIFICATION -> Notify all assigned group members (including their leader) that an event was created for them
            string creatorLabel = "Admin";
            if (!isAdminCreated && createByUserId.HasValue)
            {
                var creator = await _context.Members
                    .Where(m => m.MemberId == createByUserId.Value)
                    .Select(m => new { m.FirstName, m.LastName })
                    .FirstOrDefaultAsync();

                if (creator != null)
                    creatorLabel = $"{creator.FirstName} {creator.LastName}";
            }

            var groupMemberUserIds = await _context.GroupDetails
                .Where(gd => vm.SelectedGroupIds.Contains(gd.GroupId)
                          && gd.Member.User != null
                          && gd.Member.User.Id != null)
                .Select(gd => gd.Member.User.Id)
                .Distinct()
                .ToListAsync();

            foreach (var userId in groupMemberUserIds)
            {
                await _notificationService.NotifyUserAsync(
                    userId,
                    isAdminCreated ? "New Event Assigned" : "New Event Created",
                    isAdminCreated
                        ? $"Admin created a new event \"{newEvent.EventTitle}\" for your group. Click here to view."
                        : $"{creatorLabel} created a new event \"{newEvent.EventTitle}\" for your group. Click here to view.",
                            $"/Member/Events/Index?openEventId={newEvent.EventId}"

                );
            }

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

                // TIME-IN CLOSE cannot be after TIME-OUT OPEN
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


                // TIME-OUT OPEN must be exactly equal to EVENT END TIME (no early or late time-outs)
                if (vm.TimeOutScannerOpenTime.HasValue &&
                    vm.EndTime.HasValue &&
                    vm.TimeOutScannerOpenTime.Value != vm.EndTime.Value)
                {
                    return EventResult.Fail(
                        "Time-Out scanner opening time must be exactly the same as the event end time.");
                }

                // TIME-OUT CLOSE must be after TIME-OUT OPEN
                if (vm.TimeOutScannerOpenTime.HasValue &&
                    vm.TimeOutScannerClosesTime.HasValue &&
                    vm.TimeOutScannerClosesTime.Value <= vm.TimeOutScannerOpenTime.Value)
                {
                    return EventResult.Fail(
                        "Time-Out scanner closing time must be after its opening time.");
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
            existingEvent.TimeOutScannerOpensOffset = vm.EndTime;



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

        public async Task<Event?> HasFacilityConflictAsync(
                     List<int>? facilityId,
                     bool isAllDay,
                     DateTime startDate,
                     DateTime? endDate,
                     TimeSpan? startTime,
                     TimeSpan? endTime,
                     int? excludeEventId = null)
        {
            if (facilityId == null || !facilityId.Any())
                return null;

            if (endDate == null)
                return null;

            var (newStart, newEnd) = EventRange(isAllDay, startDate, endDate, startTime, endTime);

            var possibleEventConflicts = await _context.Events
                .Where(e =>
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus != ApprovalStatus.Rejected &&
                    e.ExpectedFacilities.Any(ef => facilityId.Contains(ef.FacilityId)) &&
                    e.StartDate.Date <= endDate.Value.Date &&
                    e.EndDate.Date >= startDate.Date &&
                    (excludeEventId == null || e.EventId != excludeEventId)
                )
                .ToListAsync();

            foreach (var existing in possibleEventConflicts)
            {
                var (existingStart, existingEnd) = EventRange(
                    existing.IsAllDay, existing.StartDate, existing.EndDate, existing.StartTime, existing.EndTime);

                if (newStart < existingEnd && existingStart < newEnd)
                    return existing;
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
                    PhotoUrl = a.Member.User.ProfilePictureUrl,
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


        private async Task<Group?> GetGroupLedByMemberAsync(int? memberId)
        {
            return await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g =>
                    g.LeaderId == memberId &&
                    !g.IsArchived);
        }

        public async Task<OperationalResult> RecordTimeInAsync(
           string qrToken,
           int eventId,
           string scannedByUserId,
           int? scannedByMemberId,
           bool isAdmin)
        {
            // Find QR owner
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.QRToken == qrToken);

            if (member == null)
            {
                return new OperationalResult { Success = false, Message = "QR code not recognized." };
            }

            // Event must exist and be approved
            var eventEntity = await _context.Events
                 .FirstOrDefaultAsync(e =>
                     e.EventId == eventId &&
                     !e.IsArchived &&
                     !e.IsCancelled &&
                     e.ApprovalStatus == ApprovalStatus.Approved);

            if (eventEntity == null)
            {
                return new OperationalResult { Success = false, Message = "Event is unavailable or has not been approved." };
            }

            // Prevent duplicate attendance
            bool alreadyCheckedIn = await _context.Attendances
                .AnyAsync(a => a.MemberId == member.MemberId && a.EventId == eventId);

            if (alreadyCheckedIn)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = $"{member.FirstName} {member.LastName} already checked in."
                };
            }

            // ============================
            // ADMIN
            // ============================
            if (isAdmin)
            {
                bool memberIsExpected = await _context.EventGroups
                    .AnyAsync(eg =>
                        eg.EventId == eventId &&
                        eg.Group.GroupDetails.Any(gd => gd.MemberId == member.MemberId));

                if (!memberIsExpected)
                {
                    return new OperationalResult
                    {
                        Success = false,
                        Message = $"{member.FirstName} is not part of the expected groups for this event."
                    };
                }

                _context.Attendances.Add(new Attendance
                {
                    MemberId = member.MemberId,
                    EventId = eventId,
                    TimeIn = DateTime.Now,
                    TimeOut = null,
                    Status = "Pending",
                    ScannedByUserId = scannedByUserId
                });

                await _context.SaveChangesAsync();

                return new OperationalResult { Success = true, Message = $"{member.FirstName} checked in successfully." };
            }

            // ============================
            // LEADER
            // ============================
            if (scannedByMemberId == null)
            {
                return new OperationalResult { Success = false, Message = "Leader identity could not be resolved." };
            }

            var leaderGroup = await GetGroupLedByMemberAsync(scannedByMemberId);

            if (leaderGroup == null)
            {
                return new OperationalResult { Success = false, Message = "You are not currently leading a group." };
            }

            // Leader's group must be assigned to this event
            bool groupIsExpected = await _context.EventGroups
                .AnyAsync(eg => eg.EventId == eventId && eg.GroupId == leaderGroup.GroupId);

            if (!groupIsExpected)
            {
                return new OperationalResult { Success = false, Message = "Your group is not assigned to this event." };
            }

            // Leader can only scan their own members
            bool memberInLeaderGroup = await _context.GroupDetails
                .AnyAsync(gd => gd.GroupId == leaderGroup.GroupId && gd.MemberId == member.MemberId);

            if (!memberInLeaderGroup)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = $"{member.FirstName} is not a member of your group."
                };
            }

            _context.Attendances.Add(new Attendance
            {
                MemberId = member.MemberId,
                EventId = eventId,
                TimeIn = DateTime.Now,
                TimeOut = null,
                Status = "Pending",
                ScannedByUserId = scannedByUserId
            });

            // First scan of this event by this leader → auto-mark them present
            bool leaderAlreadyRecorded = await _context.Attendances
                .AnyAsync(a => a.MemberId == scannedByMemberId && a.EventId == eventId);

            if (!leaderAlreadyRecorded)
            {
                var leaderMember = await _context.Members.FindAsync(scannedByMemberId.Value);
                if (leaderMember != null)
                {
                    var scanTime = DateTime.Now;

                    string leaderStatus = "Present";
                    int leaderPoints = eventEntity.Points;

                    if (eventEntity.StartTime.HasValue)
                    {
                        var eventStartDateTime = eventEntity.StartDate.Date + eventEntity.StartTime.Value;
                        var lateCutoff = eventStartDateTime + TimeSpan.FromMinutes(15);

                        if (scanTime > lateCutoff)
                        {
                            leaderStatus = "Late";
                            leaderPoints = eventEntity.Points - 2;
                        }
                    }
                    // eventEntity.StartTime == null (all-day) → falls through, stays "Present" with full points

                    _context.Attendances.Add(new Attendance
                    {
                        MemberId = scannedByMemberId.Value,
                        EventId = eventId,
                        TimeIn = scanTime,
                        TimeOut = null,
                        Status = "Pending",
                        Points = 0,
                        ScannedByUserId = scannedByUserId
                    });

                }
            }

            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = $"{member.FirstName} time in successfully." };
        }


        public async Task MarkAsAbsentAsync(int eventId, int groupId)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (existingEvent == null) return;
            if (existingEvent.IsCancelled) return;

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


        public async Task<OperationalResult> RecordTimeOutAsync(
     string qrToken,
     int eventId,
     string scannedByUserId,
     int? scannedByMemberId,
     bool isAdmin)
        {
            // ==========================================
            // 1. FIND SCANNED MEMBER
            // ==========================================

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.QRToken == qrToken);

            if (member == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "QR code not recognized."
                };
            }


            // ==========================================
            // 2. FIND EVENT
            // ==========================================

            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventId == eventId &&
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus == ApprovalStatus.Approved);

            if (eventEntity == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Event not found."
                };
            }


            // ==========================================
            // 3. EVENT MUST HAVE END TIME
            // ==========================================

            if (!eventEntity.EndTime.HasValue)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "This event does not have a Time-Out schedule."
                };
            }


            var now = DateTime.Now;


            // ==========================================
            // 4. CHECK TIME-OUT SCANNER WINDOW
            // ==========================================

            var timeOutOpen =
                eventEntity.EndDate.Date +
                (eventEntity.TimeOutScannerOpensOffset
                    ?? eventEntity.EndTime.Value);


            var timeOutClose =
                eventEntity.EndDate.Date +
                (eventEntity.TimeOutScannerClosesOffset
                    ?? eventEntity.EndTime.Value.Add(
                        TimeSpan.FromMinutes(30)));


            if (now < timeOutOpen)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message =
                        $"Time-Out scanner opens at {timeOutOpen:h:mm tt}."
                };
            }


            if (now > timeOutClose)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message =
                        $"Time-Out scanner closed at {timeOutClose:h:mm tt}."
                };
            }


            // ==========================================
            // 5. IF LEADER IS SCANNING,
            //    VERIFY THEIR GROUP
            // ==========================================

            Group? leaderGroup = null;

            if (!isAdmin)
            {
                if (scannedByMemberId == null)
                {
                    return new OperationalResult
                    {
                        Success = false,
                        Message = "Leader identity could not be resolved."
                    };
                }


                leaderGroup =
                    await GetGroupLedByMemberAsync(
                        scannedByMemberId.Value);

                if (leaderGroup == null)
                {
                    return new OperationalResult
                    {
                        Success = false,
                        Message =
                            "You are not currently leading a group."
                    };
                }


                bool groupIsExpected =
                    await _context.EventGroups
                        .AnyAsync(eg =>
                            eg.EventId == eventId &&
                            eg.GroupId == leaderGroup.GroupId);

                if (!groupIsExpected)
                {
                    return new OperationalResult
                    {
                        Success = false,
                        Message =
                            "Your group is not assigned to this event."
                    };
                }


                bool memberInLeaderGroup =
                    await _context.GroupDetails
                        .AnyAsync(gd =>
                            gd.GroupId == leaderGroup.GroupId &&
                            gd.MemberId == member.MemberId);

                if (!memberInLeaderGroup)
                {
                    return new OperationalResult
                    {
                        Success = false,
                        Message =
                            $"{member.FirstName} is not a member of your group."
                    };
                }
            }


            // ==========================================
            // 6. FIND MEMBER ATTENDANCE
            // ==========================================

            var attendance =
                await _context.Attendances
                    .FirstOrDefaultAsync(a =>
                        a.MemberId == member.MemberId &&
                        a.EventId == eventId);


            if (attendance == null ||
                !attendance.TimeIn.HasValue)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message =
                        $"{member.FirstName} never checked in — cannot record Time-Out."
                };
            }


            if (attendance.TimeOut.HasValue)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message =
                        $"{member.FirstName} already timed out."
                };
            }


            // ==========================================
            // 7. FINALIZE SCANNED MEMBER ATTENDANCE
            // ==========================================

            SetFinalAttendanceStatus(
                attendance,
                eventEntity);

            attendance.TimeOut = now;

            // Save this before changing anything else.
            // This is the number of points earned from THIS event.
            int memberPointsEarned = attendance.Points;

            member.EarnedPoints += memberPointsEarned;


            // ==========================================
            // 8. AUTO TIME-OUT THE LEADER
            // ==========================================

            Member? leaderMember = null;
            int leaderPointsEarned = 0;
            bool leaderWasTimedOut = false;

            if (!isAdmin &&
                scannedByMemberId.HasValue)
            {
                var leaderAttendance =
                    await _context.Attendances
                        .FirstOrDefaultAsync(a =>
                            a.MemberId ==
                                scannedByMemberId.Value &&
                            a.EventId == eventId);


                if (leaderAttendance != null &&
                    leaderAttendance.TimeIn.HasValue &&
                    !leaderAttendance.TimeOut.HasValue)
                {
                    leaderMember =
                        await _context.Members
                            .FirstOrDefaultAsync(m =>
                                m.MemberId ==
                                    scannedByMemberId.Value);


                    if (leaderMember != null)
                    {
                        SetFinalAttendanceStatus(
                            leaderAttendance,
                            eventEntity);

                        leaderAttendance.TimeOut = now;

                        leaderPointsEarned =
                            leaderAttendance.Points;

                        leaderMember.EarnedPoints +=
                            leaderPointsEarned;

                        leaderWasTimedOut = true;
                    }
                }
            }


            // ==========================================
            // 9. SAVE MEMBER + LEADER TOGETHER
            // ==========================================

            await _context.SaveChangesAsync();


            // ==========================================
            // 10. NOTIFY SCANNED MEMBER
            // ==========================================

            var memberUserId = await _context.Users
                .AsNoTracking()
                .Where(u =>
                    u.MemberId == member.MemberId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();


            if (!string.IsNullOrEmpty(memberUserId))
            {
                await _notificationService.NotifyUserAsync(
                    memberUserId,
                    "Event Check-Out Completed",
                    $"You checked out from \"{eventEntity.EventTitle}\" and earned {memberPointsEarned} point{(memberPointsEarned == 1 ? "" : "s")}.",
                    "/Member/Dashboard"
                );
            }


            // ==========================================
            // 11. NOTIFY LEADER IF AUTO TIMED-OUT
            // ==========================================

            if (leaderWasTimedOut &&
                leaderMember != null &&
                leaderMember.MemberId != member.MemberId)
            {
                var leaderUserId = await _context.Users
                    .AsNoTracking()
                    .Where(u =>
                        u.MemberId == leaderMember.MemberId)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();


                if (!string.IsNullOrEmpty(leaderUserId))
                {
                    await _notificationService.NotifyUserAsync(
                        leaderUserId,
                        "Event Check-Out Completed",
                        $"You were checked out from \"{eventEntity.EventTitle}\" and earned {leaderPointsEarned} point{(leaderPointsEarned == 1 ? "" : "s")}.",
                        "/Leader/Dashboard"
                    );
                }
            }


            // ==========================================
            // 12. RETURN SUCCESS
            // ==========================================

            return new OperationalResult
            {
                Success = true,
                Message =
                    $"{member.FirstName} timed out successfully and earned {memberPointsEarned} point{(memberPointsEarned == 1 ? "" : "s")}."
            };
        }


        public async Task<OperationalResult> SendAttendanceReminderAsync(
     int eventId,
     string reminderType)
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventId == eventId &&
                    !e.IsArchived &&
                    !e.IsCancelled);

            if (eventEntity == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Event not found."
                };
            }


            // ==========================================
            // GET EXPECTED GROUPS
            // ==========================================

            var expectedGroupIds = await _context.EventGroups
                .Where(eg => eg.EventId == eventId)
                .Select(eg => eg.GroupId)
                .ToListAsync();


            // ==========================================
            // GET EXPECTED MEMBERS
            // ==========================================

            var expectedMembers = await _context.GroupDetails
                .Where(gd =>
                    expectedGroupIds.Contains(gd.GroupId) &&
                    gd.Member.User != null &&
                    gd.Member.User.Id != null)
                .Select(gd => new
                {
                    gd.MemberId,
                    gd.Member.FirstName,
                    UserId = gd.Member.User.Id
                })
                .Distinct()
                .ToListAsync();


            // ==========================================
            // GET ATTENDANCE
            // ==========================================

            var attendanceRecords = await _context.Attendances
                .Where(a => a.EventId == eventId)
                .ToListAsync();


            // ==========================================
            // TIME-IN REMINDER
            // ==========================================

            if (reminderType == "TimeIn")
            {
                var membersNotCheckedIn = expectedMembers
                    .Where(member =>
                        !attendanceRecords.Any(a =>
                            a.MemberId == member.MemberId &&
                            a.TimeIn.HasValue))
                    .ToList();

                if (!membersNotCheckedIn.Any())
                {
                    return new OperationalResult
                    {
                        Success = true,
                        Message = "Everyone has already checked in — no reminders needed."
                    };
                }


                foreach (var member in membersNotCheckedIn)
                {
                    await _notificationService.NotifyUserAsync(
                        member.UserId,
                        "Time-In Reminder",
                        $"Don't forget to check in for \"{eventEntity.EventTitle}\".",
                        $"/Member/Events/Index?openEventId={eventId}"

                    );
                }


                return new OperationalResult
                {
                    Success = true,
                    Message = $"Time-In reminder sent to {membersNotCheckedIn.Count} member(s)."
                };
            }


            // ==========================================
            // TIME-OUT REMINDER
            // ==========================================

            if (reminderType == "TimeOut")
            {
                var membersNotCheckedOut = expectedMembers
                    .Where(member =>
                        attendanceRecords.Any(a =>
                            a.MemberId == member.MemberId &&
                            a.TimeIn.HasValue &&
                            !a.TimeOut.HasValue))
                    .ToList();


                if (!membersNotCheckedOut.Any())
                {
                    return new OperationalResult
                    {
                        Success = true,
                        Message = "Everyone who checked in has already checked out — no reminders needed."
                    };
                }


                foreach (var member in membersNotCheckedOut)
                {
                    await _notificationService.NotifyUserAsync(
                        member.UserId,
                        "Time-Out Reminder",
                        $"Don't forget to check out for \"{eventEntity.EventTitle}\".",
                        $"/Member/Events/Index?openEventId={eventId}"
                    );
                }


                return new OperationalResult
                {
                    Success = true,
                    Message = $"Time-Out reminder sent to {membersNotCheckedOut.Count} member(s)."
                };
            }


            return new OperationalResult
            {
                Success = false,
                Message = "Invalid reminder type."
            };
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
                    PhotoUrl = a.Member.User.ProfilePictureUrl,
                    Points = a.Points,
                    GroupName = a.Member.GroupDetails.Select(gd => gd.Group.GroupName).FirstOrDefault() ?? "-",
                    TimeIn = a.TimeIn,
                    TimeOut = a.TimeOut,
                    Status = a.Status,
                    
                    ScanByUserId = a.ScannedByUserId,
                    ScanByName = a.ScannedByUser.Member.FullName ?? "N/A",
                })
                .ToListAsync();
            return rows;
        }
        //==== SEARCH PANEL PART ==== 
        public async Task<List<EventSearchResultViewModel>> SearchPanelAsync(string search)
        {
            search ??= "";

            var existingEvents = await _context.Events
                .AsNoTracking()
                .Where(e =>
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus == ApprovalStatus.Approved &&
                    e.EventTitle.Contains(search))
                .OrderBy(e => e.StartDate)
                .ThenBy(e => e.StartTime)
                .Select(e => new EventSearchResultViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    IsAllDay = e.IsAllDay,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,

                    Facilities = e.ExpectedFacilities
                        .Select(f => f.Facility.FacilityName)
                        .ToList()
                })
                .ToListAsync();

            return existingEvents;
        }


        ////==== VIEW AVAILABLE FACILITIES ==== 
        //public async Task<List<ViewAvailableFacilitiesViewModel>> GetAvailableFacilitiesAsync()
        //{
        //    DateTime today = DateTime.Today;
        //    DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
        //    DateTime startOfNextMonth = startOfMonth.AddMonths(1);

        //    var facilities = await _context.Facilities
        //        .AsNoTracking()
        //        .Include(f => f.FacilityPhotos)
        //        .Where(f =>
        //            f.Status != FacilityStatus.UnderMaintenance &&
        //            f.Status != FacilityStatus.Inactive &&
        //            f.IsArchived != true)
        //        .ToListAsync();

        //    var result = new List<ViewAvailableFacilitiesViewModel>();

        //    foreach (var facility in facilities)
        //    {

        //        var upcoming = await _context.Events
        //           .AsNoTracking()
        //           .Where(e =>
        //               !e.IsCancelled &&
        //               e.ExpectedFacilities.Any(f => f.FacilityId == facility.FacilityId) &&
        //               e.StartDate.Date >= today)
        //           .OrderBy(e => e.StartDate)
        //           .Take(3)
        //           .Select(e => new { e.StartDate, e.EventTitle })
        //           .ToListAsync();

        //        var upcomingList = upcoming
        //         .Select(e => (Date: e.StartDate, Title: e.EventTitle))
        //         .ToList();

        //        bool hasEventToday = await _context.Events
        //            .AsNoTracking()
        //            .AnyAsync(e =>
        //                !e.IsCancelled &&
        //                e.ExpectedFacilities.Any(f => f.FacilityId == facility.FacilityId) &&
        //                e.StartDate.Date <= today &&
        //                e.EndDate >= today
        //            );

        //        int reservationsThisMonth = await _context.Events
        //          .AsNoTracking()
        //          .CountAsync(e =>
        //              !e.IsCancelled &&
        //              e.ExpectedFacilities.Any(f => f.FacilityId == facility.FacilityId) &&
        //              e.StartDate.Date >= today &&
        //              e.StartDate < startOfNextMonth
        //          );

        //        result.Add(new ViewAvailableFacilitiesViewModel
        //        {
        //            FacilityId = facility.FacilityId,
        //            FacilityName = facility.FacilityName,
        //            FacilityDescription = facility.FacilityDescription,
        //            FacilityType = facility.FacilityTypes,
        //            Capacity = facility.Capacity,
        //            UpcomingReservations = upcomingList,
        //            Status = hasEventToday ? "Limited today" : "Available now",
        //            ReservationCountThisMonth = reservationsThisMonth,
        //            Photos = facility.FacilityPhotos.Select(p => p.PhotoUrl).ToList()
        //        });
        //    }
        //    return result;
        //}

        private void SetFinalAttendanceStatus(
    Attendance attendance,
    Event eventEntity)
        {
            const int lateMinutes = 15;

            if (eventEntity.StartTime.HasValue)
            {
                var eventStartDateTime =
                    eventEntity.StartDate.Date +
                    eventEntity.StartTime.Value;

                var lateCutoff =
                    eventStartDateTime.AddMinutes(
                        lateMinutes);


                if (attendance.TimeIn.HasValue &&
                    attendance.TimeIn.Value > lateCutoff)
                {
                    attendance.Status = "Late";

                    attendance.Points =
                        Math.Max(
                            eventEntity.Points - 2,
                            0);
                }
                else
                {
                    attendance.Status = "Present";

                    attendance.Points =
                        eventEntity.Points;
                }
            }
            else
            {
                // All-day event
                attendance.Status = "Present";

                attendance.Points =
                    eventEntity.Points;
            }
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
                    !e.IsCancelled &&
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
            var existingEvent = await _context.Events
                  .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.EventId == eventId);
            if (existingEvent == null) return new EventResult { Success = false, ErrorMessage = "Event doesn't exist" };


            var eventStart =
                existingEvent.StartDate.Date +
                (existingEvent.StartTime ?? TimeSpan.Zero);

            if (eventStart <= DateTime.Now)
            {
                existingEvent.ApprovalStatus = ApprovalStatus.Expired;
                existingEvent.ExpiredDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return EventResult.Fail(
                    "This event request has expired because its scheduled start time has already passed.");
            }

            existingEvent.ApprovalStatus = ApprovalStatus.Approved;
            existingEvent.ApprovedDate = DateTime.Now;
            existingEvent.ApprovedByUserId = approveByUserId;

            await _context.SaveChangesAsync();

            if (existingEvent.CreatedByUser != null &&
                !string.IsNullOrWhiteSpace(existingEvent.CreatedByUser.UserId))
            {
                await _notificationService.NotifyUserAsync(
                existingEvent.CreatedByUser.UserId,
                 "Event Approved!",
                  $"Your event \"{existingEvent.EventTitle}\" has been approved.",
                    $"/Leader/Events?openEventId={existingEvent.EventId}"
          );
         }
            return EventResult.Ok();


        }

        // TODO: [AUTH MIGRATION] approvedByUserId is Members.MemberId (int) for now.

        public async Task<EventResult> RejectEventAsync(int eventId, string reason)
        {
            var existingEvent = await _context.Events
                .Include(e => e.CreatedByUser)
                .FirstOrDefaultAsync(e => e.EventId == eventId);
            if (existingEvent == null) return new EventResult { Success = false, ErrorMessage = "Event doesn't exist" };

            var eventStart =
            existingEvent.StartDate.Date +
            (existingEvent.StartTime ?? TimeSpan.Zero);

            if (eventStart <= DateTime.Now)
            {
                existingEvent.ApprovalStatus = ApprovalStatus.Expired;
                existingEvent.ExpiredDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return EventResult.Fail(
                    "This event request has already expired and can no longer be rejected.");
            }


            existingEvent.ApprovalStatus = ApprovalStatus.Rejected;

            existingEvent.RejectionReason = reason;

            await _context.SaveChangesAsync();


            if (existingEvent.CreatedByUser != null &&
                !string.IsNullOrWhiteSpace(existingEvent.CreatedByUser.UserId))
            {
                await _notificationService.NotifyUserAsync(
                    existingEvent.CreatedByUser.UserId,
                    "Event Rejected!",
                    $"Your event \"{existingEvent.EventTitle}\" has been rejected.",
                    $"/Leader/Events?openEventId={existingEvent.EventId}"
                );
            }

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
            var groupData = await _context.EventGroups
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
                    EventFacility = string.Join(", ", g.Event.ExpectedFacilities.Select(f => f.Facility.FacilityName)),
                    EventPhotoUrl = g.Event.ExpectedFacilities
                        .SelectMany(f => f.Facility.FacilityPhotos)
                        .Select(p => p.PhotoUrl)
                        .FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (groupData == null) return null;

            var groupMembers = await _context.GroupDetails
                .Where(gd => gd.GroupId == groupId)
                .Select(gd => new
                {
                    gd.MemberId,
                    FullName = gd.Member.FirstName + " " + gd.Member.LastName,
                    gd.Member.User.ProfilePictureUrl
                })
                .ToListAsync();

            var attendanceLookup = await _context.Attendances
                .Where(a => a.EventId == eventId)
                .Include(a => a.ScannedByUser)
                .ToDictionaryAsync(a => a.MemberId, a => a);

            groupData.attendanceRowViewModels = groupMembers.Select(m =>
            {
                attendanceLookup.TryGetValue(m.MemberId, out var att);
                return new AttendanceRowViewModel
                {
                    MemberId = m.MemberId,
                    FullName = m.FullName,
                    PhotoUrl = m.ProfilePictureUrl,
                    GroupName = groupData.GroupName,
                    TimeIn = att?.TimeIn,
                    TimeOut = att?.TimeOut,
                    ScanByUserId = att?.ScannedByUserId ?? "",
                    ScanByName = att?.ScannedByUser != null ? (att.ScannedByUser.DisplayName ?? "N/A") : "N/A",
                    Points = att?.Points ?? 0,
                    Status = att?.Status ?? "Pending",
                };
            }).ToList();

            const int graceMinutes = 15;
            foreach (var row in groupData.attendanceRowViewModels)
            {
                if (row.Status == "Absent")
                {
                    row.Remarks = "Absent";
                }
                else if (row.TimeIn.HasValue && groupData.StartTime.HasValue)
                {
                    var lateBy = row.TimeIn.Value.TimeOfDay - groupData.StartTime.Value;
                    row.Remarks = lateBy.TotalMinutes > graceMinutes
                        ? $"Late by {(int)lateBy.TotalMinutes} mins"
                        : "On time";
                }
                else
                {
                    row.Remarks = row.Status == "Pending" ? "Not yet checked in" : "Never attended";
                }
            }

            // Derive summary counts from the rows we just built — one pass, no repeated Attendances subqueries
            groupData.PresentCount = groupData.attendanceRowViewModels.Count(r => r.Status == "Present" || r.Status == "Late");
            groupData.AbsentCount = groupData.attendanceRowViewModels.Count(r => r.Status == "Absent");
            groupData.PendingCount = groupData.attendanceRowViewModels.Count(r => r.Status == "Pending");

            groupData.AttendancePercentage = groupData.TotalMembers == 0
                ? 0
                : (int)Math.Round((double)groupData.PresentCount / groupData.TotalMembers * 100);

            groupData.GroupAttendanceStatus =
                groupData.TotalMembers == 0 ? GroupAttendanceStatus.Absent :
                groupData.PresentCount == groupData.TotalMembers ? GroupAttendanceStatus.FullyPresent :
                groupData.PresentCount > 0 ? GroupAttendanceStatus.PartiallyPresent :
                GroupAttendanceStatus.Absent;

            return groupData;
        }


        public async Task<bool> LeaderOwnsEventsAsync(int eventId, int LeaderMemberId)
        {
            return await _context.Events.Where(g => g.EventId == eventId)
                        .AnyAsync(e => e.CreatedByUserId == LeaderMemberId
                        || e.ExpectedGroups.Any(e => e.Group.LeaderId == LeaderMemberId));
        }

        public async Task<int> GetUpcomingCountAsync()
        {
            var now = DateTime.Now;

            return await _context.Events
                .Where(e => !e.IsArchived
                         && !e.IsCancelled
                         && e.ApprovalStatus == ApprovalStatus.Approved
                         && e.StartDate.Date >= now.Date
                         && (e.StartDate.Date > now.Date ||
                             (e.StartTime == null || e.StartTime.Value >= now.TimeOfDay)))
                .CountAsync();
        }


        //==== ALL EVENTS (READ-ONLY, ALL LEADERS) ====
        public async Task<List<AllEventsListItemViewModel>> GetAllApprovedEventsForOrgAsync()
        {
            return await _context.Events
                .AsNoTracking()
                 .AsSplitQuery()
                .Where(e => !e.IsArchived && !e.IsCancelled && e.ApprovalStatus == ApprovalStatus.Approved)
                .OrderBy(e => e.StartDate)
                .ThenBy(e => e.StartTime)
                .Select(e => new AllEventsListItemViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsAllDay = e.IsAllDay,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    GroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList(),
                    FacilityNames = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                    CreatedByName = e.CreatedByUser != null
                        ? e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName
                        : "Admin",
                    CreatedByRole = e.CreatedByUser != null ? "Group Leader" : "Admin",
                    ApprovalStatus = e.ApprovalStatus
                }).ToListAsync();
        }

        public async Task<EventDetailsReadOnlyViewModel?> GetEventDetailsReadOnlyAsync(int id)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == id && !e.IsArchived)
                .Select(e => new EventDetailsReadOnlyViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    EventDescription = e.EventDescription,
                    ApprovalStatus = e.ApprovalStatus,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsAllDay = e.IsAllDay,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    FacilityNames = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                    FacilityPhotoUrl = e.ExpectedFacilities
                        .SelectMany(f => f.Facility.FacilityPhotos)
                        .Select(p => p.PhotoUrl)
                        .FirstOrDefault(),
                    GroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList(),
                    CreatedByName = e.CreatedByUser != null
                        ? e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName
                        : "Admin",
                    CreatedByRole = e.CreatedByUser != null ? "Group Leader" : "Admin"
                })
                .FirstOrDefaultAsync();
        }


        // Events an admin created and assigned to this leader's group — not events the leader created themselves
        public async Task<List<EventListItemViewModel>> GetAssignedEventsForLeaderAsync(int leaderMemberId)
        {
            return await _context.Events
                .Where(e => !e.IsArchived
                         && !e.IsCancelled
                         && e.ApprovalStatus == ApprovalStatus.Approved
                         && e.CreatedByUserId != leaderMemberId
                         && e.ExpectedGroups.Any(eg => eg.Group.LeaderId == leaderMemberId))
                .OrderByDescending(e => e.StartDate)
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
                    SubmittedByName = "Admin",
                    FacilityNames = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                    GroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList()
                }).ToListAsync();
        }


        // ==== CANCEL (only allowed while the event hasn't happened yet) ====
        public async Task<EventResult> CancelEventAsync(int eventId, string? reason)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (existingEvent == null) return EventResult.Fail("Event doesn't exist");

            if (existingEvent.IsArchived)
                return EventResult.Fail("This event is already archived.");

            if (existingEvent.IsCancelled)
                return EventResult.Fail("This event is already cancelled.");

            if (existingEvent.ApprovalStatus != ApprovalStatus.Approved)
                return EventResult.Fail("Only approved events can be cancelled.");

            var eventStart = existingEvent.StartDate.Date + (existingEvent.StartTime ?? TimeSpan.Zero);
            if (eventStart <= DateTime.Now)
                return EventResult.Fail("Cannot cancel an event that has already started or completed.");

            existingEvent.IsCancelled = true;
            existingEvent.CancelledDate = DateTime.Now;
            existingEvent.CancellationReason = reason;

            await _context.SaveChangesAsync();

            // ==========================================
            // GET GROUPS ASSIGNED TO THIS EVENT
            // ==========================================

            var groupIds = await _context.EventGroups
                .Where(eg => eg.EventId == eventId)
                .Select(eg => eg.GroupId)
                .Distinct()
                .ToListAsync();


            // ==========================================
            // GET USERS OF ALL MEMBERS
            // ==========================================

            var memberUserIds = await _context.GroupDetails
                .Where(gd =>
                    groupIds.Contains(gd.GroupId) &&
                    gd.Member.User != null &&
                    gd.Member.User.Id != null)
                .Select(gd => gd.Member.User.Id)
                .Distinct()
                .ToListAsync();




            var leaderUserIds = await _context.Groups
                .Where(g =>
                    groupIds.Contains(g.GroupId) &&
                    g.LeaderId != null &&
                    g.Leader != null &&
                    g.Leader.User != null &&
                    g.Leader.User.Id != null)
                .Select(g => g.Leader.User.Id)
                .Distinct()
                .ToListAsync();


            var usersToNotify = memberUserIds
                .Concat(leaderUserIds)
                .Distinct()
                .ToList();


            // ==========================================
            // NOTIFY MEMBERS -> Member area page
            // ==========================================
            foreach (var userId in memberUserIds)
            {
                var message = string.IsNullOrWhiteSpace(reason)
                    ? $"The event \"{existingEvent.EventTitle}\" has been cancelled."
                    : $"The event \"{existingEvent.EventTitle}\" has been cancelled. Reason: {reason}";

                await _notificationService.NotifyUserAsync(
                    userId,
                    "Event Cancelled",
                    message,
                    $"/Member/Events/CancelledDetails/{existingEvent.EventId}"
                );
            }

            // ==========================================
            // NOTIFY LEADERS -> Leader area page
            // ==========================================
            foreach (var userId in leaderUserIds)
            {
                var message = string.IsNullOrWhiteSpace(reason)
                    ? $"The event \"{existingEvent.EventTitle}\" has been cancelled."
                    : $"The event \"{existingEvent.EventTitle}\" has been cancelled. Reason: {reason}";

                await _notificationService.NotifyUserAsync(
                    userId,
                    "Event Cancelled",
                    message,
                    $"/Leader/Events/CancelledDetails/{existingEvent.EventId}"
                );
            }

            return EventResult.Ok();
        }

        public async Task<EventResult> RestoreCancelledEventAsync(int eventId)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (existingEvent == null) return EventResult.Fail("Event doesn't exist");

            if (!existingEvent.IsCancelled)
                return EventResult.Fail("This event isn't cancelled.");

            var eventStart = existingEvent.StartDate.Date + (existingEvent.StartTime ?? TimeSpan.Zero);
            if (eventStart <= DateTime.Now)
            {
               
                return EventResult.Fail(
                    "This event's original date has already passed. Please edit it with new dates before restoring.");
            }

            existingEvent.IsCancelled = false;
            existingEvent.CancelledDate = null;
            existingEvent.CancellationReason = null;

            await _context.SaveChangesAsync();

            // ==========================================
            // NOTIFY GROUP MEMBERS AND LEADERS
            // ==========================================

            foreach (var eventGroup in existingEvent.ExpectedGroups)
            {
                var groupId = eventGroup.GroupId;

                // Notify members of the group
                await _notificationService.NotifyGroupMembersAsync(
                    groupId,
                    "Event Restored",
                    $"The event \"{existingEvent.EventTitle}\" has been restored and is active again.",
                    $"/Member/Calendar/Index"
                );

                // Notify the group's leader
                if (eventGroup.Group?.LeaderId != null)
                {
                    var leaderUserId = await _context.Users
                        .Where(u => u.MemberId == eventGroup.Group.LeaderId)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(leaderUserId))
                    {
                        await _notificationService.NotifyUserAsync(
                            leaderUserId,
                            "Event Restored",
                            $"The event \"{existingEvent.EventTitle}\" has been restored and is active again.",
                            $"/Leader/Events/AssignedEvents"
                        );
                    }
                }
            }
            return EventResult.Ok();
        }

        // ==== LIST: Upcoming (approved, not cancelled, not archived, still in the future) ====
        public async Task<List<EventListItemViewModel>> GetUpcomingEventsAsync(bool isAdmin, int? currentUserId)
        {
            var now = DateTime.Now;

            var query = _context.Events
                .Where(e => !e.IsArchived
                         && !e.IsCancelled
                         && e.ApprovalStatus == ApprovalStatus.Approved
                         && e.StartDate.Date >= now.Date
                         && (e.StartDate.Date > now.Date
                             || e.StartTime == null
                             || e.StartTime.Value >= now.TimeOfDay));

            if (!isAdmin)
            {
                query = query.Where(e => e.CreatedByUserId == currentUserId
                                       || e.ExpectedGroups.Any(eg => eg.Group.LeaderId == currentUserId));
            }

            return await query
                .OrderBy(e => e.StartDate).ThenBy(e => e.StartTime)
                .Select(e => new EventListItemViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    StartDate = e.StartDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    ApprovalStatus = e.ApprovalStatus,
                    DateCreated = e.DateCreated,
                    FacilityNames = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                    GroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList()
                }).ToListAsync();
        }


        // ==== LIST: Cancelled ====
        public async Task<List<EventListItemViewModel>> GetCancelledEventsAsync(bool isAdmin, int? currentUserId)
        {
            var query = _context.Events.Where(e => !e.IsArchived && e.IsCancelled);

            if (!isAdmin)
            {
                query = query.Where(e => e.CreatedByUserId == currentUserId
                                       || e.ExpectedGroups.Any(eg => eg.Group.LeaderId == currentUserId));
            }

            return await query
                .OrderByDescending(e => e.CancelledDate)
                .Select(e => new EventListItemViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    StartDate = e.StartDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    IsCancelled = e.IsCancelled,
                    CancellationReason = e.CancellationReason,
                    CancelledDate = e.CancelledDate,
                    ApprovalStatus = e.ApprovalStatus,
                    DateCreated = e.DateCreated,
                    FacilityNames = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                    GroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList()
                }).ToListAsync();
        }

        public async Task<ArchiveViewModel?> GetEventForCancelAsync(int eventId)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == eventId && !e.IsArchived)
                .Select(e => new ArchiveViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    StartDate = e.StartDate
                })
                .FirstOrDefaultAsync();
        }



        //get details EVENT CANCELLED 
        public async Task<CancelledEventDetailsViewModel?> GetCancelledEventDetailsAsync(int eventId)
        {
            var eventDetails = await _context.Events
                .AsNoTracking()
                .Where(e => e.EventId == eventId && e.IsCancelled && !e.IsArchived)
                .Select(e => new CancelledEventDetailsViewModel
                {
                    EventId = e.EventId,
                    EventTitle = e.EventTitle,
                    EventDescription = e.EventDescription,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsAllDay = e.IsAllDay,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    FacilityNames = e.ExpectedFacilities.Select(f => f.Facility.FacilityName).ToList(),
                    AssignedGroupNames = e.ExpectedGroups.Select(g => g.Group.GroupName).ToList(),
                    CreatedByName = e.CreatedByUser != null
                        ? e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName
                        : "Admin",
                    CancellationReason = e.CancellationReason,
                    CancelledDate = e.CancelledDate
                })
                .FirstOrDefaultAsync();

            return eventDetails;
        }


        public async Task ExpirePendingEventRequestsAsync()
        {
            var now = DateTime.Now;

            var pendingEvents = await _context.Events
                .Where(e =>
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus == ApprovalStatus.Pending)
                .ToListAsync();

            foreach (var eventItem in pendingEvents)
            {
                var eventStart =
                    eventItem.StartDate.Date +
                    (eventItem.StartTime ?? TimeSpan.Zero);

                if (eventStart <= now)
                {
                    eventItem.ApprovalStatus = ApprovalStatus.Expired;
                    eventItem.ExpiredDate = now;
                }
            }

            await _context.SaveChangesAsync();
        }


        public async Task CheckPendingEventRemindersAsync()
        {
            var now = DateTime.Now;

            var pendingEvents = await _context.Events
                .Include(e => e.CreatedByUser)
                .Where(e =>
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus == ApprovalStatus.Pending)
                .ToListAsync();

            foreach (var eventItem in pendingEvents)
            {
                var eventStart =
                    eventItem.StartDate.Date +
                    (eventItem.StartTime ?? TimeSpan.Zero);

                var timeRemaining = eventStart - now;


                // ==========================================
                // EVENT ALREADY STARTED -> EXPIRE
                // ==========================================

                if (timeRemaining <= TimeSpan.Zero)
                {
                    eventItem.ApprovalStatus = ApprovalStatus.Expired;
                    eventItem.ExpiredDate = now;

                    if (eventItem.CreatedByUser != null &&
                     !string.IsNullOrWhiteSpace(eventItem.CreatedByUser.UserId))
                    {
                        await _notificationService.NotifyUserAsync(
                            eventItem.CreatedByUser.UserId,
                            "Event Request Expired",
                            $"Your event request \"{eventItem.EventTitle}\" expired because it was not reviewed before its scheduled start time.",
                            $"/Leader/Events?openEventId={eventItem.EventId}"
                        );
                    }
                    continue;
                }


                // ==========================================
                // 1 HOUR REMINDER
                // ==========================================

                if (timeRemaining <= TimeSpan.FromHours(1) &&
                    eventItem.OneHourReminderSentAt == null)
                {
                    await _notificationService.NotifyAdminAsync(
                        new NotificationViewModel
                        {
                            Title = "Urgent Event Request",
                            Message =
                                $"\"{eventItem.EventTitle}\" starts in less than 1 hour and is still waiting for approval.",
                            Url =
                                $"/Events/ViewEventsRequests?openEventId={eventItem.EventId}"
                        });

                    eventItem.OneHourReminderSentAt = now;

                    continue;
                }


                // ==========================================
                // 1 DAY REMINDER
                // ==========================================

                if (timeRemaining <= TimeSpan.FromDays(1) &&
                    eventItem.OneDayReminderSentAt == null)
                {
                    await _notificationService.NotifyAdminAsync(
                        new NotificationViewModel
                        {
                            Title = "Event Request Needs Review",
                            Message =
                                $"\"{eventItem.EventTitle}\" starts within 1 day and is still waiting for approval.",
                            Url =
                                $"/Events/ViewEventsRequests?openEventId={eventItem.EventId}"
                        });

                    eventItem.OneDayReminderSentAt = now;

                    continue;
                }


                // ==========================================
                // 3 DAY REMINDER
                // ==========================================

                if (timeRemaining <= TimeSpan.FromDays(3) &&
                    eventItem.ThreeDayReminderSentAt == null)
                {
                    await _notificationService.NotifyAdminAsync(
                        new NotificationViewModel
                        {
                            Title = "Pending Event Request",
                            Message =
                                $"\"{eventItem.EventTitle}\" is scheduled within 3 days and still needs review.",
                            Url =
                                $"/Events/ViewEventsRequests?openEventId={eventItem.EventId}"
                        });

                    eventItem.ThreeDayReminderSentAt = now;
                }
            }

            await _context.SaveChangesAsync();
        }


        public async Task<RestoreEventViewModel?> ConfirmationRestoreAsync(int? eventId)
        {
            if (eventId == null) return null;

            var existingEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == eventId.Value && !e.IsArchived);

            if (existingEvent == null) return null;

            var eventStart = existingEvent.StartDate.Date + (existingEvent.StartTime ?? TimeSpan.Zero);

            // Same rule RestoreCancelledEventAsync enforces — computed here so the
            // modal can warn before the click, not just reject after it.
            var canRestore = eventStart > DateTime.Now;

            return new RestoreEventViewModel
            {
                EventId = existingEvent.EventId,
                EventTitle = existingEvent.EventTitle,
                StartDate = existingEvent.StartDate,
                StartTime = existingEvent.StartTime,
                CancelledDate = existingEvent.CancelledDate,
                CancellationReason = existingEvent.CancellationReason,
                CanRestore = canRestore
            };
        }



        public async Task<List<GroupAttendanceChartItemViewModel>> GetGroupAttendanceChartAsync(int groupId)
        {
            var now = DateTime.Now;

            // Last 5 events this group was expected at, that have already ended —
            // date-only comparison, so an event ending later today still counts as
            // "not yet completed" and is excluded for now.
            var recentEvents = await _context.EventGroups
                .Where(eg => eg.GroupId == groupId
                          && eg.Event.EndDate.Date < now.Date
                          && !eg.Event.IsArchived
                          && !eg.Event.IsCancelled)
                .OrderByDescending(eg => eg.Event.EndDate)
                .Select(eg => new { eg.EventId, eg.Event.EventTitle, eg.Event.EndDate })
                .Take(5)
                .ToListAsync();

            var groupMemberIds = await _context.GroupDetails
                .Where(gd => gd.GroupId == groupId)
                .Select(gd => gd.MemberId)
                .ToListAsync();

            var chartData = new List<GroupAttendanceChartItemViewModel>();

            foreach (var evt in recentEvents.OrderBy(e => e.EndDate))
            {
                var presentCount = await _context.Attendances
                    .CountAsync(a => a.EventId == evt.EventId
                                   && groupMemberIds.Contains(a.MemberId)
                                   && (a.Status == "Present" || a.Status == "Late"));

                chartData.Add(new GroupAttendanceChartItemViewModel
                {
                    EventTitle = evt.EventTitle,
                    PresentCount = presentCount
                });
            }

            return chartData;
        }


        public async Task<MemberEventsViewModel> GetEventsForMemberAsync(int memberId)
        {
            var now = DateTime.Now;


            // ==========================================
            // 1. FIND THE MEMBER'S GROUP
            // ==========================================

            var memberGroup = await _context.GroupDetails
                .AsNoTracking()
                .Where(gd =>
                    gd.MemberId == memberId &&
                    gd.Group != null &&
                    !gd.Group.IsArchived)
                .Select(gd => new
                {
                    gd.GroupId,
                    GroupName = gd.Group.GroupName,

                    MemberCount = gd.Group.GroupDetails
                        .Count(x =>
                            x.Member != null &&
                            x.Member.ApplicationStatus ==
                                MembershipApplicationStatus.Approved &&
                            x.Member.IsArchive != true)
                })
                .FirstOrDefaultAsync();


            // Member does not belong to a group
            if (memberGroup == null)
            {
                return new MemberEventsViewModel
                {
                    GroupId = null,
                    GroupName = "No Group",
                    GroupMemberCount = 0,
                    Events = new List<MemberEventItemViewModel>()
                };
            }


            // ==========================================
            // 2. GET EVENTS ASSIGNED TO THAT GROUP
            // ==========================================

            var events = await _context.Events
                .AsNoTracking()
                .Where(e =>
                    !e.IsArchived && 
                    !e.IsCancelled &&
                    e.ApprovalStatus == ApprovalStatus.Approved &&

                    e.ExpectedGroups.Any(eg =>
                        eg.GroupId == memberGroup.GroupId))
                 .OrderByDescending(e => e.StartDate)
                .ThenByDescending(e => e.StartTime)
                .Select(e => new MemberEventItemViewModel
                {
                    EventId = e.EventId,

                    EventTitle = e.EventTitle,

                    StartDate = e.StartDate,

                    EndDate = e.EndDate,

                    IsAllDay = e.IsAllDay,

                    StartTime = e.StartTime,

                    EndTime = e.EndTime,


                    // First assigned facility
                    FacilityName = e.ExpectedFacilities
                        .Select(ef => ef.Facility.FacilityName)
                        .FirstOrDefault() ?? "No facility",


                    // First photo from the event's facilities
                    FacilityPhotoUrl = e.ExpectedFacilities
                        .SelectMany(ef => ef.Facility.FacilityPhotos)
                        .Select(photo => photo.PhotoUrl)
                        .FirstOrDefault(),


                    // If CreatedByUser is null, it was created by Admin
                    HostedByName = e.CreatedByUser != null
                        ? e.CreatedByUser.FirstName + " " +
                          e.CreatedByUser.LastName
                        : "Admin",

                    HostedByRole = e.CreatedByUser != null
                        ? "Leader"
                        : "Admin"
                })
                .ToListAsync();


            // ==========================================
            // 3. CALCULATE EVENT STATUS
            // ==========================================

            foreach (var eventItem in events)
            {
                DateTime eventStart;
                DateTime eventEnd;


                // ALL-DAY EVENT
                if (eventItem.IsAllDay)
                {
                    eventStart = eventItem.StartDate.Date;

                    eventEnd = eventItem.EndDate.Date
                        .AddDays(1)
                        .AddTicks(-1);
                }

                // NORMAL EVENT
                else
                {
                    eventStart =
                        eventItem.StartDate.Date +
                        (eventItem.StartTime ?? TimeSpan.Zero);

                    eventEnd =
                        eventItem.EndDate.Date +
                        (eventItem.EndTime ?? TimeSpan.Zero);
                }


                if (now < eventStart)
                {
                    eventItem.Status = "Upcoming";
                }
                else if (now <= eventEnd)
                {
                    eventItem.Status = "Ongoing";
                }
                else
                {
                    eventItem.Status = "Completed";
                }
            }

            events = events
            .Where(e =>
                e.Status == "Upcoming" ||
                e.Status == "Ongoing")
            .OrderBy(e => e.StartDate)
            .ThenBy(e => e.StartTime)
            .ToList();


            // ==========================================
            // 4. RETURN THE WHOLE PAGE VIEWMODEL
            // ==========================================

            return new MemberEventsViewModel
            {
                GroupId = memberGroup.GroupId,

                GroupName = memberGroup.GroupName,

                GroupMemberCount = memberGroup.MemberCount,

                Events = events
            };
        }


        public async Task<MemberEventDetailsViewModel?>GetMemberEventDetailsAsync(int eventId, int memberId)
        {
            var now = DateTime.Now;


            // ==========================================
            // 1. GET EVENT DETAILS
            // ==========================================

            var eventDetails = await _context.Events
                .AsNoTracking()
                .Where(e =>
                    e.EventId == eventId &&

                    // Member only sees approved,
                    // active events
                    !e.IsArchived &&
                    !e.IsCancelled &&
                    e.ApprovalStatus == ApprovalStatus.Approved &&

                    // Event must be assigned to
                    // this member's group
                    e.ExpectedGroups.Any(eg =>
                        eg.Group.GroupDetails.Any(gd =>
                            gd.MemberId == memberId)))
                .Select(e => new MemberEventDetailsViewModel
                {
                    EventId = e.EventId,

                    EventTitle = e.EventTitle,

                    EventDescription =
                        e.EventDescription ??
                        "No description available.",


                    // DATE
                    StartDate = e.StartDate,

                    EndDate = e.EndDate,


                    // TIME
                    IsAllDay = e.IsAllDay,

                    StartTime = e.StartTime,

                    EndTime = e.EndTime,


                    // FACILITIES
                    FacilityNames = e.ExpectedFacilities
                        .Select(ef =>
                            ef.Facility.FacilityName)
                        .ToList(),


                    // CREATOR
                    PostedByName =
                        e.CreatedByUser != null
                            ? e.CreatedByUser.FirstName + " " +
                              e.CreatedByUser.LastName
                            : "Admin",

                    PostedByRole =
                        e.CreatedByUser != null
                            ? "Leader"
                            : "Admin",


                    // POINTS
                    Points = e.Points
                })
                .FirstOrDefaultAsync();


            if (eventDetails == null)
            {
                return null;
            }



            eventDetails.FacilityPhotoUrls =
                await _context.EventFacilities
                    .AsNoTracking()
                    .Where(ef =>
                        ef.EventId == eventId)

                    .Select(ef =>
                        ef.Facility.FacilityPhotos
                            .OrderBy(photo => photo.FacilityPhotoId)
                            .Select(photo => photo.PhotoUrl)
                            .FirstOrDefault())

                    .Where(photoUrl =>
                        photoUrl != null &&
                        photoUrl != "")

                    .Select(photoUrl => photoUrl!)
                    .ToListAsync();



            DateTime eventStart;
            DateTime eventEnd;


            if (eventDetails.IsAllDay)
            {
                eventStart =
                    eventDetails.StartDate.Date;

                eventEnd =
                    eventDetails.EndDate.Date
                        .AddDays(1)
                        .AddTicks(-1);
            }
            else
            {
                eventStart =
                    eventDetails.StartDate.Date +
                    (eventDetails.StartTime ??
                     TimeSpan.Zero);

                eventEnd =
                    eventDetails.EndDate.Date +
                    (eventDetails.EndTime ??
                     TimeSpan.Zero);
            }


            if (now < eventStart)
            {
                eventDetails.Status = "Upcoming";
            }
            else if (now <= eventEnd)
            {
                eventDetails.Status = "Ongoing";
            }
            else
            {
                eventDetails.Status = "Completed";
            }


            return eventDetails;
        }

    }
}
