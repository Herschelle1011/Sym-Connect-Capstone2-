using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Event;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
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
        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events.
                Include(f => f.Facility)
                    .ThenInclude(fp => fp.FacilityPhotos)
                .Include(u => u.CreatedByUser)
                .Include(eg => eg.ExpectedGroups)
                    .ThenInclude(g => g.Group)
                        .ThenInclude(gd => gd.GroupDetails)
                            .ThenInclude(m => m.Member)
                .Include(e => e.Evaluations)
                .Include( a => a.Attendances).
                         ThenInclude(m => m.Member).
                    FirstOrDefaultAsync(e => e.EventId == id && e.IsArchived == false);
          
        }
        public async Task<bool> CreateEventAsync(CreateEventViewModel vm)
        {
            var hasConflict = await HasFacilityConflictAsync(vm.FacilityId, vm.IsAllDay, vm.StartDate, vm.EndDate, vm.StartTime, vm.EndTime);
            if (hasConflict) return false;


            var newEvent = new Event
            {
                EventTitle = vm.EventTitle,
                EventDescription = vm.EventDescription,
                IsAllDay = vm.IsAllDay,
                FacilityId = vm.FacilityId,
                DateCreated = DateTime.Now,
                StartDate = vm.StartDate,
                EndDate  = vm.EndDate,
                StartTime = vm.StartTime ,
                EndTime = vm.EndTime ,
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


            existingEvent.EventTitle = vm.EventTitle;
            existingEvent.EventDescription = vm.EventDescription;
            existingEvent.StartDate = vm.StartDate;
            existingEvent.EndDate = vm.EndDate;
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
                EventTitle = existingEvent.EventTitle,
                EventDescription = existingEvent.EventDescription ?? "N/A",
                IsAllDay = existingEvent.IsAllDay,
                StartDate = existingEvent.StartDate,
                EndDate = existingEvent.EndDate,
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

    }
}
