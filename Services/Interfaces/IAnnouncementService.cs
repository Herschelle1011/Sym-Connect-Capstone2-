using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Announcement;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IAnnouncementService
    {
        //FOR VIEW
        Task<List<ListAnnouncementViewModel>> GetAllAnnouncementAsync();
        Task<DetailsAnnouncementViewModel> GetByIdAsync(int id);

        //CRUD
        Task<OperationalResult> CreateAsync(AnnouncementViewModel vm, string? createdByLeaderId); 
        Task<OperationalResult> UpdateAsync(AnnouncementViewModel vm);

        Task ArchiveAsync(int id);
        Task RestoreAsync(int id);
        Task<AnnouncementViewModel?> GetEditForm(int announcementId);

        Task<VisibleAnnouncementViewModel?> GetPinnedAnnouncementAsync(bool isLeader, int? groupId);

        Task<List<ListAnnouncementViewModel>> GetAnnouncementsForLeaderAsync(int groupId);

        Task<List<VisibleAnnouncementViewModel>> GetVisibleAnnouncementsAsync(int memberId, bool isLeader, int? group);
    }
}
