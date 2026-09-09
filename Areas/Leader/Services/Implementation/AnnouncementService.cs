using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Leader.Services.Interfaces;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.ViewModels.Announcement;

namespace Sym_Connect__Capstone2_.Areas.Leader.Services.Implementation
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly AppDbContext _context;

        public AnnouncementService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<ListAnnouncementViewModel>> GetAnnouncementsByGroupAsync(int groupId)
        {
            var now = DateTime.Now;

            return await _context.Announcements
                .AsNoTracking()
                .Where(a =>
                    !a.IsArchived &&
                    a.GroupId == groupId)
                .OrderByDescending(a => a.CreatedDate)
                .Select(a => new ListAnnouncementViewModel
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    CreatedAt = a.CreatedDate,

                    TargetType = a.TargetAudience,

                    GroupName = a.Group != null
                        ? a.Group.GroupName
                        : null,

                    ExpiresAt = a.ExpirationDate,

                    PublishOption = a.PublishOption,

                    CreatedByPhotoUrl = a.User != null ? a.User.ProfilePictureUrl : "",

                    CreatedBy = a.User != null
                        ? a.User.DisplayName
                        : "Unknown",

                    Status =
                        a.ExpirationDate.HasValue &&
                        a.ExpirationDate.Value <= now
                            ? "Expired"
                            : a.PublishDate.HasValue &&
                              a.PublishDate.Value > now
                                ? "Scheduled"
                                : "Published",

                    PublishDate = a.PublishDate
                })
                .ToListAsync();
        }








    }
}
