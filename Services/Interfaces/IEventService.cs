using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.ViewModels.Event;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IEventService
    {
        Task<List<EventCalendarViewModel>>GetAllEventAsync(int year, int month);
        Task<Event?>GetEventByIdAsync(int id);
        Task CreateEventAsync(CreateEventViewModel vm);
        Task UpdateEventAsync(int? id, EditEventViewModel vm);
        Task<ArchiveViewModel?> ConfirmationArchiveAsync(int? id);
        Task ArchiveEventAsync(int id);
        Task<EditEventViewModel?> GetEventForEditAsync(int? id);

    }
}
