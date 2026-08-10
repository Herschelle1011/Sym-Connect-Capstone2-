using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.EventModels;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.ViewModels.Group;

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




        public async Task<List<EventCalendarViewModel>> GetAllEventAsync(int year, int month )
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
                    EndTime = e.EndTime,
                    DateCreated = e.DateCreated,
                    CreatedBy = e.CreatedByUser.FirstName + " " + e.CreatedByUser.LastName,
                    FacilityId = e.FacilityId,
                    FacilityName = e.Facility.FacilityName,
                    FacilityDescription = e.Facility.FacilityDescription,
                    FacilityCapacity = e.Facility.Capacity,

                    FacilityPhotos = e.Facility.FacilityPhotos.Select(f => new FacilityPhotoVM
                    {
                        FacilityPhotoId = f.FacilityPhotoId,
                        PhotoUrl = f.PhotoUrl,
                        FacilityId = f.FacilityId
                    }).ToList(),


                    ExpectedGroups = e.ExpectedGroups.Select(g => new ExpectedGroupCardVM
                    {
                        GroupId = g.GroupId,
                        GroupName = g.Group.GroupName,
                        MemberCount = g.Group.GroupDetails.Count()
                    }).ToList(), 
                    
                    ScheduleTimeline = new List<ScheduleTimelineVM>()

                }).FirstOrDefaultAsync();
            if (existingEvent == null) return null;

            existingEvent.AttendanceRecords = await ViewAttendanceForEventAsync(id);



            if (existingEvent.StartTime.HasValue && existingEvent.EndTime.HasValue)
            {
                var QrOpenTime = existingEvent.StartTime.Value.Add(TimeSpan.FromHours(-1));

                existingEvent.ScheduleTimeline.Add(new ScheduleTimelineVM
                {
                    Time = DateTime.Today.Add(QrOpenTime).ToString("h:mm tt"),
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
                    Description = "Event ends"
                });

            }

            var startDate = existingEvent.StartDate.Date + (existingEvent.StartTime ?? TimeSpan.Zero);
            var endDate = existingEvent.EndDate.Date + (existingEvent.EndTime ?? TimeSpan.Zero);

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
        public async Task<bool> CreateEventAsync(CreateEventViewModel vm)
        {
            if (vm.StartDate == default(DateTime) || vm.EndDate == default(DateTime))
            {
                return false;
            }

            var hasConflict = await HasFacilityConflictAsync(vm.FacilityId, vm.IsAllDay, vm.StartDate, vm.EndDate, vm.StartTime, vm.EndTime);
            if (hasConflict) return false;

            if (vm.IsAllDay)
            {
                vm.StartTime = null;
                vm.EndTime = null;
            }

            var newEvent = new Event
            {
                EventTitle = vm.EventTitle,
                Points = vm.Points,
                EventDescription = vm.EventDescription,
                IsAllDay = vm.IsAllDay,
                FacilityId = vm.FacilityId,
                DateCreated = DateTime.UtcNow,
                StartDate = vm.StartDate.Date,
                EndDate  = vm.EndDate.Date,
                StartTime = vm.StartTime ,
                EndTime = vm.EndTime ,
                IsArchived = false
            };
            foreach(var groupId in vm.SelectedGroupIds)
            {
                newEvent.ExpectedGroups.Add(new EventGroup
                {
                    GroupId = groupId
                });
            }
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> UpdateEventAsync(int? gId, EditEventViewModel vm)
        {
            //find the ID first from the EVENT
            var existingEvent = await _context.Events.
                            Include(eg => eg.ExpectedGroups)
                            .FirstOrDefaultAsync(e => e.EventId == gId);

            if (existingEvent == null) return false;

            var hasConflict = await HasFacilityConflictAsync(vm.FacilityId, vm.IsAllDay, vm.StartDate, vm.EndDate, vm.StartTime, vm.EndTime, excludeEventId: gId);
            if (hasConflict) return false;

            if (vm.IsAllDay)
            {
                vm.StartTime = null;
                vm.EndTime = null;
            }

            existingEvent.EventTitle = vm.EventTitle;
            existingEvent.EventDescription = vm.EventDescription;
            existingEvent.StartDate = vm.StartDate;
            existingEvent.EndDate = vm.EndDate;
            existingEvent.Points = vm.Points;
            existingEvent.StartTime = vm.StartTime;
            existingEvent.IsAllDay = vm.IsAllDay;
            existingEvent.EndTime = vm.EndTime;
            existingEvent.FacilityId = vm.FacilityId;

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
            return true;
        }

        public async Task <EditEventViewModel?> GetEventForEditAsync(int? id)
        {
            var existingEvent = await _context.Events.
                    Include(eg => eg.ExpectedGroups)
             
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
                StartTime = existingEvent.StartTime,
                EndTime = existingEvent.EndTime,
                FacilityId = existingEvent.FacilityId,
                SelectedGroupIds = existingEvent.ExpectedGroups.Select(eg => eg.GroupId).ToList()
            };
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
                FacilityId = existingEvent.FacilityId,
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

        private(DateTime StartDate, DateTime EndDate) EventRange(bool IsAllDay, DateTime startDate, DateTime endDate, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (IsAllDay)
            {
                return (startDate.Date, endDate.Date.AddDays(1));
            }
            return (startDate.Date + (startTime ?? TimeSpan.Zero), endDate.Date + (endTime ?? TimeSpan.Zero));
        }

        //understand this part >>>> to be check....
        public async Task <bool> HasFacilityConflictAsync(int facilityId,bool isAllDay ,DateTime startDate, 
                                        DateTime endDate, TimeSpan? startTime, TimeSpan? endTime, int? excludeEventId = null)
        {
            var (newStart, newEnd) = EventRange(isAllDay, startDate, endDate, startTime, endTime);
            //list of those events in the same DATE
            var possibleEventConflicts = await _context.Events.Where(e => e.FacilityId == facilityId
                                                                                && e.StartDate.Date <= endDate.Date
                                                                                && e.EndDate.Date >= startDate.Date
                                                                                && (excludeEventId == null || e.EventId != excludeEventId)).ToListAsync();
            //checks the list 
            foreach(var existing in possibleEventConflicts)
            {
                var (existingStart, existingEnd) = EventRange
                    (existing.IsAllDay, existing.StartDate, existing.EndDate, existing.StartTime, existing.EndTime);

                if (newStart < existingEnd && existingStart < newEnd) return true;
            }
            return false;
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

        public async Task<ScanResult> RecordTimeInAsync(string qrToken, int eventId)
        {
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.QRToken == qrToken);
            if (member == null)
            {
                return new ScanResult { Success = false, Message = "QR code not recognized." };
            }

            var EventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!EventExists)
            {
                return new ScanResult { Success = false, Message = "Event not found." };
            }

            var ExistingRecord = await _context.Attendances.FirstOrDefaultAsync(m => m.MemberId == member.MemberId
            && m.EventId == eventId);


            if (ExistingRecord != null)
            {
                return new ScanResult { Success = false, Message = $"{member.FirstName} {member.LastName} already Checked In" };
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

            if (memberGroupIds == null) return new ScanResult { Success = false, Message = "Event not found." };

            bool isExpected = expectedGroupIds.Any(id => memberGroupIds.Contains(id));

            if (!isExpected)
                return new ScanResult { Success = false, Message = $"{member.FirstName} is not part of the expected groups for this event." };

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

            return new ScanResult { Success = true, Message = $"{member.FirstName} Checked in Successfully." };
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



        public async Task<ScanResult> RecordTimeOutAsync(string qrToken, int eventId)
        {
            var member = await _context.Members
              .FirstOrDefaultAsync(m => m.QRToken == qrToken);
            if (member == null)
            {
                return new ScanResult { Success = false, Message = "QR code not recognized." };
            }

            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
                return new ScanResult { Success = false, Message = "Event not found." };


            var MemberhasTimeIn = await _context.Attendances.FirstOrDefaultAsync(m => m.MemberId == member.MemberId && m.EventId == eventId);
         
            if (MemberhasTimeIn == null)
            {
                return new ScanResult { Success = false, Message = $"{member.FirstName} never checked in — cannot record time-out." };
            }

            if (MemberhasTimeIn.TimeOut.HasValue)
                return new ScanResult { Success = false, Message = $"{member.FirstName} already timed out." };


            MemberhasTimeIn.TimeOut = DateTime.Now;
            MemberhasTimeIn.Status = "Present";
            await _context.SaveChangesAsync();

            return new ScanResult { Success = true, Message = $"{member.FirstName} Timed Out Successfully." };

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


    }
}
