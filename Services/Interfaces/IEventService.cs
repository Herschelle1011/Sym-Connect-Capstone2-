using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.EventModels;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Attendance;
using Sym_Connect__Capstone2_.ViewModels.Event;
using Sym_Connect__Capstone2_.ViewModels.Facility;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IEventService
    {
        Task<List<EventCalendarViewModel>> GetAllEventAsync(int year, int month);
        //Task<Event?>GetEventByIdAsync(int id);
        Task<OperationalResult> RecordTimeInAsync(string qrToken, int eventId);

        Task<ScannerViewModel> GetScannerStateAsync(int eventId);
        Task<DetailsEventViewModel?> GetDetailsByAsync(int id);

        Task MarkAsAbsentAsync(int eventId);
        Task<Event?> GetEventByIdAsync(int eventId);
        Task<EventResult> CreateEventAsync(CreateEventViewModel vm, bool isAdminCreated, int? createByUserId);
        Task<EventResult> UpdateEventAsync(int? gId, EditEventViewModel vm);
        Task<ArchiveViewModel?> ConfirmationArchiveAsync(int? id);
        Task ArchiveEventAsync(int id);

        Task<FacilityScheduleViewModel?> GetFacilityScheduleAsync(int facilityId, int year, int month);

        Task<List<ViewAvailableFacilitiesViewModel>> GetAvailableFacilitiesAsync();

        Task<OperationalResult> RecordTimeOutAsync(string qrToken, int eventId);

        Task<List<EventSearchResultViewModel>> SearchPanelAsync(string search);

        Task<List<AttendanceRowViewModel>> ViewAttendanceForEventAsync(int eventId);

        Task<EditEventViewModel?> GetEventForEditAsync(int? id);

        Task<List<ExpectedFacilityCardVM>> GetFacilitiesForModalAsync(int? id);
        Task<List<ExpectedGroupCardVM>> GetGroupsForModalAsync(int? id);


        Task<Event?> HasFacilityConflictAsync(List<int>? facilityId, bool isAllDay, DateTime startDate,
                                             DateTime? endDate, TimeSpan? startTime, TimeSpan? endTime, int? excludeEventId = null);


        //LEADERS PART VIEW ==== My Events Page
        Task<List<EventListItemViewModel>> GetAllEventForLeaderAsync(int memberId);


        //ADMIN Part === View Events Requests
        Task<SummaryEventsViewModel> GetEventsSummaryForAdminAsync();

        // APPROVAL/REJECTS PART 
        Task<EventResult> ApproveEventAsync(int eventId, int? approveByUserId);
        Task<EventResult> RejectEventAsync(int eventId, string reason);

        Task<int> GetPendingRequestCountAsync();

        Task<AttendanceTabViewModel> GetGroupAttendanceOverviewAsync(int eventId);

        Task<GroupAttendanceDetailsViewModel> GroupAttendanceDetailsView(int groupId, int eventId);
       }
}
