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

        Task<OperationalResult> ArchiveAnnouncementAsync(
                   int announcementId);
            Task RestoreAsync(int id);
        Task<AnnouncementViewModel?> GetEditForm(int announcementId);

        Task<List<VisibleAnnouncementViewModel?>> GetPinnedAnnouncementsAsync(bool isLeader, int? groupId);
        Task<List<ListAnnouncementViewModel>> GetAnnouncementsForLeaderAsync(int groupId);

        Task<List<VisibleAnnouncementViewModel>> GetVisibleAnnouncementsAsync(int memberId, bool isLeader, int? group);

        Task<OperationalResult> CreateAdminAnnouncementAsync(AnnouncementViewModel vm, string adminUserId);

        Task<AnnouncementViewModel?> GetEditFormForLeaderAsync(int announcementId, string leaderId, int leaderGroupId);
      
        Task<OperationalResult> UpdateLeaderAnnouncementAsync(AnnouncementViewModel vm, string leaderId ,int leaderGroupId);

        Task<OperationalResult> ArchiveLeaderAnnouncementAsync(int id, string leaderId, int leaderGroupId);

        //get
        Task<ArchiveAnnouncementViewModel?> GetArchivePreviewAsync(int announcementId);

        Task<ArchivedAnnouncementDetailsViewModel?> GetArchivedAnnouncementDetailsAsync(int id);

        Task<OperationalResult> HardDeleteAnnouncement(int id);
    }
}
