using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.ViewModels.Event;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IEventService
    {
        Task<List<EventCalendarViewModel>>GetAllEventAsync(int year, int month);
        //Task<Event?>GetEventByIdAsync(int id);
        Task<ScanResult> RecordTimeInAsync(string qrToken, int eventId);

        Task<ScannerViewModel> GetScannerStateAsync(int eventId);
        Task<DetailsEventViewModel?> GetDetailsByAsync(int id);
        Task<bool> CreateEventAsync(CreateEventViewModel vm);
        Task<bool> UpdateEventAsync(int? gId, EditEventViewModel vm);
        Task<ArchiveViewModel?> ConfirmationArchiveAsync(int? id);
        Task ArchiveEventAsync(int id);

        Task<List<AttendanceRowViewModel>> ViewAttendanceForEventAsync(int eventId);

        Task<EditEventViewModel?> GetEventForEditAsync(int? id);
        Task<bool> HasFacilityConflictAsync(int facilityId, bool isAllDay, DateTime startDate,
                                        DateTime endDate, TimeSpan? startTime, TimeSpan? endTime, int? excludeEventId = null);

    }
}
