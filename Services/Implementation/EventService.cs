using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.EventModels;
using Sym_Connect__Capstone2_.Models.FacilityModels;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.ViewModels.Facility;
using Sym_Connect__Capstone2_.ViewModels.Group;
using System.Diagnostics.CodeAnalysis;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<Event?> GetEventByIdAsync(int eventId)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        }




        public async Task<List<EventCalendarViewModel>> GetAllEventAsync(int year, int month)
        {
            return await _context.Events
                                .Where(e => !e.IsArchived &&
                                e.StartDate.Year == year && e.StartDate.Month == month)
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
                    TimeOutScannerOpenTime = e.TimeOutScannerOpensOffset,
                    TimeInScannerOpenTime = e.TimeInScannerOpensOffset,
                    EndTime = e.EndTime,
                    DateCreated = e.DateCreated,
                    CreatedBy = e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName,
                    Facilities = e.ExpectedFacilities.Select(f => new ExpectedFacilityCardVM
                    {
                        FacilityId = f.FacilityId,
                        FacilityCapacity = f.Facility.Capacity,
                        FacilityName = f.Facility.FacilityName
                    }).ToList(),
                    ExpectedGroups = e.ExpectedGroups.Select(g => new ExpectedGroupCardVM
                    {
                        GroupId = g.GroupId,
                        GroupName = g.Group.GroupName,
                        MemberCount = g.Group.GroupDetails.Count()
                    }).ToList(),

                    ExpectedGroupCount = e.ExpectedGroups.Count(),

                    ScheduleTimeline = new List<ScheduleTimelineVM>()

                }).FirstOrDefaultAsync();
            if (existingEvent == null) return null;

            existingEvent.AttendanceRecords = await ViewAttendanceForEventAsync(id);


            if (existingEvent.StartTime.HasValue && existingEvent.EndTime.HasValue)
            {
                var qrOpenTime = existingEvent.TimeInScannerOpenTime ??
                                existingEvent.StartTime.Value.Add(TimeSpan.FromMinutes(-30));


                existingEvent.ScheduleTimeline.Add(new ScheduleTimelineVM
                {
                    Time = DateTime.Today.Add(qrOpenTime).ToString("h:mm tt"),
                    Description = "QR Scanner Opens"
                });

                existingEvent.ScheduleTimeline.Add(new ScheduleTimelineVM
                {
                    Time = DateTime.Today.Add(existingEvent.StartTime.Value).ToString("h:mm tt"),
                    Description = "Event Starts"
                });

                existingEvent.ScheduleTimeline.Add(new ScheduleTimelineVM
                {
                    Time = DateTime.Today.Add(existingEvent.EndTime.Value).ToString("h:mm tt"),
                    Description = "Event Ends"
                });
            }

            var startDate = existingEvent.StartDate.Date + (existingEvent.StartTime ?? TimeSpan.Zero);
            var endDate = existingEvent.EndDate?.Date + (existingEvent.EndTime ?? TimeSpan.Zero);

            var now = DateTime.UtcNow;
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
        public async Task<EventResult> CreateEventAsync(CreateEventViewModel vm)
        {
            //VALIDATION FOR START DATE AND END DATE
            if (vm.StartDate == default(DateTime) || vm.EndDate == default(DateTime))
            {
                return EventResult.Fail("Please provide valid start and end dates.");
            }
            if (vm.EndDate < vm.StartDate.Date)
            {
                return EventResult.Fail("End date must be higher than start date.");
            }

            if (vm.StartDate.Date < DateTime.Today)
            {
                return EventResult.Fail("You cannot create an event in the past.");
            }

       


            var hasConflict = await HasFacilityConflictAsync(vm.SelectedFacilityIds, vm.IsAllDay, vm.StartDate, vm.EndDate, vm.StartTime, vm.EndTime);
            if (hasConflict != null)
            {
                return new EventResult
                {

                    Success = false,
                    ErrorMessage = $"This facility is already booked for \"{hasConflict.EventTitle}\" during that time.",
                    ConflictingEventTitle = hasConflict.EventTitle
                };
            }
            if (!vm.IsAllDay && (vm.StartDate.Date == null || vm.EndDate == null))
            {
                return EventResult.Fail("Start time and end time is required.");
            }


            if (vm.IsAllDay && (vm.StartTime == null || vm.EndTime == null))
            {
                vm.StartTime = null;
                vm.EndTime = null;
            }

            if (!vm.IsAllDay)
            {
                //VALIDATION FOR OPENTIME SCANNER 
                if (vm.TimeInScannerOpenTime.HasValue && vm.StartTime.HasValue
                  && vm.TimeInScannerOpenTime.Value > vm.StartTime.Value)
                {
                    return EventResult.Fail("Time-In scanner cannot open after the event has already started.");
                }

                if (vm.TimeOutScannerOpenTime.HasValue && vm.EndTime.HasValue
                    && vm.TimeOutScannerOpenTime.Value > vm.EndTime.Value)
                {
                    return EventResult.Fail("Time-Out scanner cannot open after the event has already ended.");
                }

                if (vm.TimeOutScannerOpenTime.HasValue && vm.StartTime.HasValue
                    && vm.TimeOutScannerOpenTime.Value < vm.StartTime.Value)
                {
                    return EventResult.Fail("Time-Out scanner cannot open before the event starts.");
                }
            }

            var newEvent = new Event
            {
                EventTitle = vm.EventTitle,
                Points = vm.Points,
                EventDescription = vm.EventDescription,
                IsAllDay = vm.IsAllDay,
                DateCreated = DateTime.UtcNow,
                StartDate = vm.StartDate.Date,
                TimeInScannerOpensOffset = vm.TimeInScannerOpenTime,
                TimeOutScannerOpensOffset = vm.TimeOutScannerOpenTime,
                EndDate  = vm.EndDate?.Date,
                StartTime = vm.StartTime ,
                EndTime = vm.EndTime ,
                IsArchived = false
            };
            foreach(var facilityId in vm.SelectedFacilityIds)
            {
                newEvent.ExpectedFacilities.Add(new EventFacility
                {
                    FacilityId = facilityId
                });
            }
            foreach(var groupId in vm.SelectedGroupIds)
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
            //find the ID first from the EVENT
            var existingEvent = await _context.Events.
                            Include(eg => eg.ExpectedGroups)
                            .FirstOrDefaultAsync(e => e.EventId == gId);

            if (existingEvent == null) return EventResult.Fail("Event doesn't exist");

            var hasConflict = await HasFacilityConflictAsync(vm.SelectedFacilityIds, vm.IsAllDay, vm.StartDate, vm.EndDate, vm.StartTime, vm.EndTime, excludeEventId: gId);
            if (hasConflict != null)
            {
                return EventResult.Fail($"This facility is already booked for \"{hasConflict.EventTitle}\" during that time.");
            }
            if (vm.IsAllDay)
            {
                vm.StartTime = null;
                vm.EndTime = null;
            }

            if (!vm.IsAllDay)
            {
                //VALIDATION FOR OPENTIME SCANNER 
                if (vm.TimeInScannerOpenTime.HasValue && vm.StartTime.HasValue
              && vm.TimeInScannerOpenTime.Value > vm.StartTime.Value)
                {
                    return EventResult.Fail("Time-In scanner cannot open after the event has already started.");
                }

                if (vm.TimeOutScannerOpenTime.HasValue && vm.EndTime.HasValue
                    && vm.TimeOutScannerOpenTime.Value > vm.EndTime.Value)
                {
                    return EventResult.Fail("Time-Out scanner cannot open after the event has already ended.");
                }

                if (vm.TimeOutScannerOpenTime.HasValue && vm.StartTime.HasValue
                    && vm.TimeOutScannerOpenTime.Value < vm.StartTime.Value)
                {
                    return EventResult.Fail("Time-Out scanner cannot open before the event starts.");
                }
            }

            existingEvent.EventTitle = vm.EventTitle;
            existingEvent.EventDescription = vm.EventDescription;
            existingEvent.StartDate = vm.StartDate;
            existingEvent.EndDate = vm.EndDate;
            existingEvent.Points = vm.Points;
            existingEvent.StartTime = vm.StartTime;
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

            foreach(var remove in groupRemove)
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

        public async Task <EditEventViewModel?> GetEventForEditAsync(int? id)
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
                TimeInScannerOpenTime = existingEvent.TimeInScannerOpensOffset,
                TimeOutScannerOpenTime = existingEvent.TimeOutScannerOpensOffset,
                StartTime = existingEvent.StartTime,
                EndTime = existingEvent.EndTime,
                SelectedFacilityIds = existingEvent.ExpectedFacilities.Select(f => f.FacilityId).ToList(),
                SelectedGroupIds = existingEvent.ExpectedGroups.Select(eg => eg.GroupId).ToList()
            };
        }

        public async Task <List<ExpectedFacilityCardVM>>GetFacilitiesForModalAsync(int? id)
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

        public async Task<List<ExpectedGroupCardVM>>GetGroupsForModalAsync(int? id)
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

        private(DateTime StartDate, DateTime EndDate) EventRange(bool IsAllDay, DateTime startDate, DateTime? endDate, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (IsAllDay)
            {
                return (startDate.Date, endDate.Value.Date.AddDays(1));
            }
            return (startDate.Date + (startTime ?? TimeSpan.Zero), endDate.Value.Date + (endTime ?? TimeSpan.Zero));
        }

        //understand this part >>>> to be check....
        public async Task <Event?> HasFacilityConflictAsync(List<int>? facilityId,bool isAllDay ,DateTime startDate, 
                                        DateTime? endDate, TimeSpan? startTime, TimeSpan? endTime, int? excludeEventId = null)
        {
            var (newStart, newEnd) = EventRange(isAllDay, startDate, endDate, startTime, endTime);
            //list of those events in the same DATE
            var possibleEventConflicts = await _context.Events.Where(e =>
                                        e.ExpectedFacilities.Any(ef => facilityId.Contains(ef.FacilityId))
                                        && e.StartDate.Date <= endDate.Value.Date
                                        && e.EndDate.Value.Date >= startDate.Date
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
                    MemberId = a.MemberId,
                    FullName = a.Member.FullName,
                    PhotoUrl = a.Member.ProfilePictureUrl,
                    GroupName = a.Member.GroupDetails
                        .Where(gd => _context.Events
                            .Where(e => e.EventId == eventId)
                            .SelectMany(e => e.ExpectedGroups)
                            .Any(eg => eg.GroupId == gd.GroupId))
                        .Select(gd => gd.Group.GroupName)
                        .FirstOrDefault(),
                    CheckedInAt = a.TimeIn
                })
                .ToListAsync();

            return new ScannerViewModel
            {
                EventId = eventId,
                EventName = scannerState.EventTitle,
                TotalMembersCount = scannerState.TotalMembersCount,
                PresentCount = participants.Select(p => p.MemberId).Distinct().Count(),
                Participants = participants
            };
        }






        //-------- QR SCANNER PART CODE ------

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
                Points = eventEntity.Points,
                TimeIn = DateTime.Now,
                TimeOut = null,
                Status = "Pending"
            });

            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = $"{member.FirstName} Checked in Successfully." };
        }


        public async Task<bool> NoTimeOut(string qrToken, int eventId)
        {
            var memberTask = _context.Members.FirstOrDefaultAsync(m => m.QRToken == qrToken);
            var eventTask = _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId);

            await Task.WhenAll(memberTask, eventTask);

            var member = await memberTask;
            var existingEvent = await eventTask;

            if (member == null || existingEvent == null)
            {
                return false;
            }

            var currentDateTime = DateTime.Now;
            bool isEventExpired = existingEvent.EndDate < currentDateTime.Date ||
                                 (existingEvent.EndDate == currentDateTime.Date && existingEvent.EndTime < currentDateTime.TimeOfDay);

            if (isEventExpired)
            {
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.MemberId == member.MemberId && a.EventId == eventId);

                if (attendance != null && attendance.Status == "Pending")
                {
                    attendance.Status = "Absent";
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }



        public async Task<OperationalResult> RecordTimeOutAsync(string qrToken, int eventId)
        {
            var member = await _context.Members
              .FirstOrDefaultAsync(m => m.QRToken == qrToken);
            if (member == null)
            {
                return new OperationalResult { Success = false, Message = "QR code not recognized." };
            }

            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
                return new OperationalResult { Success = false, Message = "Event not found." };


            var MemberhasTimeIn = await _context.Attendances.FirstOrDefaultAsync(m => m.MemberId == member.MemberId && m.EventId == eventId);
         
            if (MemberhasTimeIn == null)
            {
                return new OperationalResult { Success = false, Message = $"{member.FirstName} never checked in — cannot record time-out." };
            }

            if (MemberhasTimeIn.TimeOut.HasValue)
                return new OperationalResult { Success = false, Message = $"{member.FirstName} already timed out." };


            MemberhasTimeIn.TimeOut = DateTime.Now;
            MemberhasTimeIn.Status = "Present";
            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = $"{member.FirstName} Timed Out Successfully." };

        }


        //private string ComputeStatus(
        //      DateTime? timeIn,
        //      DateTime? timeOut,
        //      DateTime startDate,
        //      TimeSpan startTime)
        //{
        //    if (!timeIn.HasValue)
        //        return "Pending";

        //    var eventStart = startDate.Date.Add(startTime);

        //    // 10-minute grace period
        //    var lateThreshold = eventStart.AddMinutes(10);

        //    if (timeIn.Value <= lateThreshold)
        //        return "Present";

        //    return "Late";
        //}


        //------------ATTENDANCE PART --------

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
                        e.ExpectedFacilities.Any(f => f.FacilityId== facility.FacilityId) &&
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


    }
}
