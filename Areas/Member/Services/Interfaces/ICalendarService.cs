using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Calendar;
namespace Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces
{
    public interface ICalendarService
    {
        Task<MemberCalendarPageViewModel> GetCalendarPageAsync(
                   int memberId,
                   int year,
                   int month,
                   string activeTab
               );
    }
}
