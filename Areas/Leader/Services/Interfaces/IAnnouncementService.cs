using Sym_Connect__Capstone2_.ViewModels.Announcement;

namespace Sym_Connect__Capstone2_.Areas.Leader.Services.Interfaces
{
    public interface IAnnouncementService 
    {
          Task<List<ListAnnouncementViewModel>>GetAnnouncementsByGroupAsync(int groupId);
    }
}
