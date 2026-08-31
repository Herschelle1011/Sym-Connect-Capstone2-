using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.AnnouncementModels;
using Sym_Connect__Capstone2_.Models.Enums.AnnouncementEnums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Announcement;
using System.Runtime.CompilerServices;
namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly AppDbContext _context;
        private readonly IGroupService _groupService;

        public AnnouncementService(AppDbContext context, IGroupService groupService)
        {
            _context = context;
            _groupService = groupService;
        }

        public async Task<List<ListAnnouncementViewModel>> GetAllAnnouncementAsync()
        {
            var now = DateTime.Now;

            return await _context.Announcements
                .AsNoTracking()
                .Where(a => !a.IsArchived)
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

                    IsPinned = a.IsPinned,

                    CreatedBy = a.User != null && a.User.DisplayName != null
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

        public async Task<DetailsAnnouncementViewModel> GetByIdAsync(int id)
        {

            var announcement = await _context.Announcements
                .AsNoTracking()
                .Where(a => !a.IsArchived)
                .Select(a => new DetailsAnnouncementViewModel
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    TargetAudience = a.TargetAudience,
                    CreatedAt = a.CreatedDate,
                    PublishedAt = a.PublishDate,
                    ExpiresAt = a.ExpirationDate,
                    GroupName = a.Group.GroupName != null ? a.Group.GroupName : null,
                })
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);
            if (announcement == null) return null;
            return announcement;
        }

        public async Task<OperationalResult> CreateAsync(AnnouncementViewModel vm, string? createdByLeaderId)
        {
            var NewAnnouncement = new Announcement
            {
                Title = vm.Title,
                Content = vm.Content,
                TargetAudience = vm.TargetAudience,
                GroupId = vm.TargetAudience == TargetAudience.SpecificGroup ? vm.GroupId : null,
                PublishOption = vm.PublishOption,
                PublishDate = ResolvePublishDate(vm),
                ExpirationOption = vm.ExpirationOption,
                ExpirationDate = ResolveExpirationDate(vm),
                IsPinned = vm.IsPinned == true,
                CreatedDate = DateTime.Now,
                CreatedByUserId = createdByLeaderId,
                IsArchived = false,
            };

            _context.Announcements.Add(NewAnnouncement);
            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = "Announcement created successfully" };
        }

        public async Task<AnnouncementViewModel?> GetEditForm(int announcementId)
        {
            if (announcementId == 0) return null;
            var existing = await _context.Announcements
                .Where(a => a.AnnouncementId == announcementId && !a.IsArchived)
                .Select(a => new AnnouncementViewModel
                {
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    TargetAudience = a.TargetAudience,
                    GroupId = a.GroupId,
                    PublishOption = a.PublishOption,
                    PublishDate = a.PublishOption == PublishOption.Scheduled ? a.PublishDate : null,
                    ExpirationOption = a.ExpirationOption,
                    IsPinned = a.IsPinned,
                    CustomExpirationDate = a.ExpirationOption == ExpirationOption.Custom ? a.ExpirationDate : null,
                })
              .FirstOrDefaultAsync();
            if (existing == null) return null;

            existing.GroupOptions = await GetGroupSelectListAsync();

            return existing;
        }

        public async Task<OperationalResult> UpdateAsync(AnnouncementViewModel vm)
        {
            // fetch-first, never overwrite untracked fields
            var existing = await _context.Announcements
                .FirstOrDefaultAsync(a => a.AnnouncementId == vm.AnnouncementId);
            if (existing == null) return new OperationalResult { Success = false, Message = $"Announcement does not exist" }; ;
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            if (vm.IsPinned == true)
            {
                await _context.Announcements
                    .Where(a => a.IsPinned == true && a.AnnouncementId != vm.AnnouncementId)
                    .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsPinned, false));
            }


            existing.Title = vm.Title;
            existing.Content = vm.Content;
            existing.TargetAudience = vm.TargetAudience;
            existing.GroupId = vm.TargetAudience == TargetAudience.SpecificGroup ? vm.GroupId : null;
            existing.PublishOption = vm.PublishOption;
            existing.PublishDate = ResolvePublishDate(vm);
            existing.IsPinned = vm.IsPinned == true;
            existing.ExpirationOption = vm.ExpirationOption;
            existing.ExpirationDate = ResolveExpirationDate(vm);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new OperationalResult { Success = true, Message = $"Announcement{existing.Title} updated successfully" };

        }

        public async Task<List<VisibleAnnouncementViewModel>> GetVisibleAnnouncementsAsync(int memberId,
                                                                bool isLeader, int? groupId)
        {
            var now = DateTime.Now;

            return await _context.Announcements.AsNoTracking()
                    .Where(a => !a.IsArchived)
                    .Where(a => a.PublishDate <= now)
                    .Where(a => a.ExpirationDate == null || a.ExpirationDate > now)
                    .Where(a =>
                            a.TargetAudience == TargetAudience.Everyone ||
                            (a.TargetAudience == TargetAudience.AllLeaders && isLeader) ||
                            a.TargetAudience == TargetAudience.AllMembers ||
                            a.TargetAudience == TargetAudience.SpecificGroup && a.GroupId == groupId)
                    .OrderByDescending(a => a.PublishDate)
                    .Select(a => new VisibleAnnouncementViewModel
                    {
                        AnnouncementId = a.AnnouncementId,
                        Title = a.Title,
                        Content = a.Content,
                        PublishedAt = a.PublishDate,
                        ExpiresAt = a.ExpirationDate,
                        TargetType = a.TargetAudience,
                        PhotoUrl = a.User.ProfilePictureUrl,
                        PostedByName = a.User.DisplayName ?? "Unknown",
                        GroupName = a.Group.GroupName != null ? a.Group.GroupName : null
                    })
                    .ToListAsync();
        }

        //============ PINNED TO DASHBOARD =========== 
        public async Task<VisibleAnnouncementViewModel?> GetPinnedAnnouncementAsync(bool isLeader, int? groupId)
        {
            var now = DateTime.Now;

            return await _context.Announcements.AsNoTracking()
                    .Where(a => !a.IsArchived)
                    .Where(a => a.IsPinned == true)
                    .Where(a => a.PublishDate <= now)
                    .Where(a => a.ExpirationDate == null || a.ExpirationDate > now)
                    .Where(a =>
                            a.TargetAudience == TargetAudience.Everyone ||
                            (a.TargetAudience == TargetAudience.AllLeaders && isLeader) ||
                            a.TargetAudience == TargetAudience.AllMembers ||
                            (a.TargetAudience == TargetAudience.SpecificGroup && a.GroupId == groupId))
                    .OrderByDescending(a => a.PublishDate)
                    .Select(a => new VisibleAnnouncementViewModel
                    {
                        AnnouncementId = a.AnnouncementId,
                        Title = a.Title,
                        Content = a.Content,
                        PublishedAt = a.PublishDate,
                        ExpiresAt = a.ExpirationDate,
                        TargetType = a.TargetAudience,
                        GroupName = a.Group.GroupName
                    })
                    .FirstOrDefaultAsync();
        }


        private DateTime ResolvePublishDate(AnnouncementViewModel vm)
        {
            return vm.PublishOption == PublishOption.Scheduled && vm.PublishDate.HasValue
                ? vm.PublishDate.Value
                : DateTime.Now;
        }
        private DateTime? ResolveExpirationDate(AnnouncementViewModel vm)
        {
            var now = DateTime.Now;
            return vm.ExpirationOption switch
            {
                ExpirationOption.NoExpiration => null,
                ExpirationOption.OneDay => now.AddDays(1),
                ExpirationOption.TwoDays => now.AddDays(2),
                ExpirationOption.ThreeDays => now.AddDays(3),
                ExpirationOption.Custom => vm.CustomExpirationDate,
                _ => null
            };
        }


        public async Task ArchiveAsync(int id)
        {
            var existing = await _context.Announcements.FindAsync(id);
            if (existing == null) return;
            existing.IsArchived = true;
            existing.ArchiveDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task RestoreAsync(int id)
        {
            var existing = await _context.Announcements.FindAsync(id);
            if (existing == null) return;
            existing.IsArchived = false;
            existing.ArchiveDate = null;
            await _context.SaveChangesAsync();
        }

        private async Task<List<SelectListItem>> GetGroupSelectListAsync()
        {
            var groups = await _groupService.GetAllGroupsAsync(); 
            return groups.Select(g => new SelectListItem
            {
                Value = g.GroupId.ToString(),
                Text = g.GroupName
            }).ToList();
        }


        public async Task<List<ListAnnouncementViewModel>> GetAnnouncementsForLeaderAsync(int groupId)
        {
            var now = DateTime.Now;

            return await _context.Announcements
                .AsNoTracking()
                .Where(a =>
                    !a.IsArchived &&
                    (
                        a.TargetAudience == TargetAudience.AllLeaders
                        ||
                        (
                            a.TargetAudience == TargetAudience.SpecificGroup &&
                            a.GroupId == groupId
                        )
                    )
                )
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.CreatedDate)
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
                    IsPinned = a.IsPinned,

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
