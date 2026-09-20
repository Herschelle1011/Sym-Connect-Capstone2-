using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Archive;
namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class ArchiveService : IArchiveService
    {
        private readonly AppDbContext _context;
        private readonly IPartyListService _partyListService;
        private readonly IElectionService _electionService;
        private readonly UserManager<User> _userManager;
        private readonly IAnnouncementService _announcementService;
        private readonly IFacilityService _facilityService;
        private readonly IGroupService _groupService;


        public ArchiveService(AppDbContext context, IPartyListService partyListService, IElectionService electionService, UserManager<User> userManager, IAnnouncementService announcementService, IFacilityService facilityService)
        {
            _context = context;
            _partyListService = partyListService;
            _electionService = electionService;
            _userManager = userManager;
            _announcementService = announcementService;
            _facilityService = facilityService;

        }

        public async Task<List<ArchiveItemViewModel>> GetAllArchivedAsync()
        {
            var items = new List<ArchiveItemViewModel>();
            var now = DateTime.Now;

            var archivedGroups = await _context.Groups
                .AsNoTracking()
                .Where(g => g.IsArchived)
                .Select(g => new ArchiveItemViewModel
                {
                    Id = g.GroupId,
                    ItemName = g.GroupName,
                    Category = ArchiveCategory.Group,
                    ArchiveDate = g.ArchivedDate,
                    CanRestore = true 
                }).ToListAsync();
            items.AddRange(archivedGroups);

            var archivedMembers = await _context.Members
                .AsNoTracking()
                .Where(m => m.IsArchive == true) // nullable bool needs explicit == true
                .Select(m => new ArchiveItemViewModel
                {
                    Id = m.MemberId,
                    ItemName = m.FirstName + " " + m.LastName, 
                    Category = ArchiveCategory.Member,

                    ArchiveDate = m.ArchiveDate,
                    CanRestore = true
                }).ToListAsync();
            items.AddRange(archivedMembers);

            var archivedEvaluations = await _context.Evaluations
                .AsNoTracking()
                .Where(e => e.IsArchived)
                .Select(e => new ArchiveItemViewModel
                {
                    Id = e.EvaluationId,
                    ItemName = e.EvaluationTitle,
                    Category = ArchiveCategory.Evaluation,
                    ArchiveDate = e.ArchivedDate,
                    CanRestore = e.OpenAt > now
                }).ToListAsync();
            items.AddRange(archivedEvaluations);

            var archivedFacilities = await _context.Facilities
            .AsNoTracking()
            .Where(f => f.IsArchived == true)
            .Select(f => new ArchiveItemViewModel
            {
                Id = f.FacilityId,
                ItemName = f.FacilityName,
                Category = ArchiveCategory.Facility,
                ArchiveDate = f.ArchiveDate,
                CanRestore = true
            }).ToListAsync();
            items.AddRange(archivedFacilities);

            // MEDIA FOLDERS
            var archivedMediaFolders = await _context.MediaFolders
                .AsNoTracking()
                .Where(m => m.isArchived == true)
                .Select(m => new ArchiveItemViewModel
                {
                    Id = m.MediaFolderId,
                    ItemName = m.FolderName,
                    Category = ArchiveCategory.Media,
                    ArchiveDate = m.ArchiveDate,
                    CanRestore = true
                })
                .ToListAsync();

            items.AddRange(archivedMediaFolders);


            var archivedPartyLists = await _context.PartyLists
            .AsNoTracking()
            .Where(p => p.isArchived == true)
            .Select(p => new ArchiveItemViewModel
            {
                Id = p.PartyListId,
                ItemName = p.PartyName,
                Category = ArchiveCategory.PartyList,
                ArchiveDate = p.ArchivedDate,
                CanRestore = true
            })
            .ToListAsync();
            items.AddRange(archivedPartyLists);

            var archivedElections = await _context.Elections
            .AsNoTracking()
            .Where(e => e.IsArchived == true) // nullable bool, same as Member/Facility
            .Select(e => new ArchiveItemViewModel
            {
                Id = e.ElectionId,
                ItemName = e.ElectionName,
                Category = ArchiveCategory.Election,
                ArchiveDate = e.ArchivedDate,
                CanRestore = e.StartDate > now
            }).ToListAsync();
            items.AddRange(archivedElections);

            var archivedAnnouncements = await _context.Announcements
            .AsNoTracking()
            .Where(a => a.IsArchived == true)
            .Select(a => new ArchiveItemViewModel
            {
                Id = a.AnnouncementId,
                ItemName = a.Title,
                Category = ArchiveCategory.Announcement,
                ArchiveDate = a.ArchiveDate,
                CanRestore = a.ExpirationDate == null ||
                     a.ExpirationDate > now
            })
            .ToListAsync();

            items.AddRange(archivedAnnouncements);


            return items.OrderByDescending(i => i.ArchiveDate).ToList();


        }

        public async Task<OperationalResult> RestoreAsync(int id, ArchiveCategory category)
        {
            switch (category)
            {
                case ArchiveCategory.Group:
                    var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == id);
                    if (group == null) return new OperationalResult { Success = false, Message = "Group not found." };
                    group.IsArchived = false;
                    group.ArchivedDate = null;
                    break;

                case ArchiveCategory.Member:
                    {
                        var member = await _context.Members
                            .FirstOrDefaultAsync(m => m.MemberId == id);

                        if (member == null)
                        {
                            return new OperationalResult
                            {
                                Success = false,
                                Message = "Member not found."
                            };
                        }

                        if (member.IsArchive != true)
                        {
                            return new OperationalResult
                            {
                                Success = false,
                                Message = "This member is not archived."
                            };
                        }

                        // Restore member
                        member.IsArchive = false;
                        member.ArchiveDate = null;
                        member.Status = "Active";
                        member.EarnedPoints = 0;

                        // Restore login access
                        if (!string.IsNullOrEmpty(member.UserId))
                        {
                            var user = await _userManager.FindByIdAsync(member.UserId);

                            if (user != null)
                            {
                                await _userManager.SetLockoutEndDateAsync(user, null);

                                await _userManager.UpdateSecurityStampAsync(user);
                            }
                        }

                        break;
                    }

                case ArchiveCategory.Evaluation:
                    var evaluation = await _context.Evaluations.FirstOrDefaultAsync(e => e.EvaluationId == id);
                    if (evaluation == null) return new OperationalResult { Success = false, Message = "Evaluation not found." };
                    evaluation.IsArchived = false;
                    evaluation.ArchivedDate = null;
                    break;

                case ArchiveCategory.Announcement:
                    {
                        var announcement = await _context.Announcements
                            .FirstOrDefaultAsync(a => a.AnnouncementId == id);

                        if (announcement == null)
                            return new OperationalResult { Success = false, Message = "Evaluation not found." };

                        var now = DateTime.Now;

                        if (announcement.ExpirationDate.HasValue &&
                            announcement.ExpirationDate.Value <= now)
                        {
                            return new OperationalResult { Success = false, Message = "Evaluation not found." };
                        }

                        announcement.IsArchived = false;
                        announcement.ArchiveDate = null;

                        break;
                    }


                case ArchiveCategory.Media:

                    var mediaFolder = await _context.MediaFolders
                        .Include(m => m.Files)
                        .FirstOrDefaultAsync(m => m.MediaFolderId == id);

                    if (mediaFolder == null)
                    {
                        return new OperationalResult
                        {
                            Success = false,
                            Message = "Media folder not found."
                        };
                    }

                    mediaFolder.isArchived = false;
                    mediaFolder.ArchiveDate = null;

                    foreach (var file in mediaFolder.Files)
                    {
                        file.IsArchived = false;
                    }

                    break;

                case ArchiveCategory.Facility:
                    var facility = await _context.Facilities.FirstOrDefaultAsync(f => f.FacilityId == id);
                    if (facility == null) return new OperationalResult { Success = false, Message = "Facility not found." };
                    facility.IsArchived = false;
                    facility.ArchiveDate = null;
                    break;

                case ArchiveCategory.PartyList:
                    return await _partyListService.RestorePartyListAsync(id);

                case ArchiveCategory.Election:
                    return await _electionService.RestoreElectionAsync(id);

                default:
                    return new OperationalResult { Success = false, Message = "Unsupported category." };
            }

            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = "Restored successfully." };
        }

        public async Task<OperationalResult> HardDeleteAsync(int id, ArchiveCategory category)
        {
            try
            {
                switch (category)
                {

                    case ArchiveCategory.Group:
                        return await _groupService.HardDeleteGroupAsync(id);

                    case ArchiveCategory.Member:
                        return new OperationalResult
                        {
                            Success = false,
                            Message = "Members cannot be permanently deleted. You can restore archived members instead."
                        };

                    case ArchiveCategory.Evaluation:
                        var evaluation = await _context.Evaluations.FirstOrDefaultAsync(e => e.EvaluationId == id);
                        if (evaluation == null) return new OperationalResult { Success = false, Message = "Evaluation not found." };
                        _context.Evaluations.Remove(evaluation);
                        break;


                    case ArchiveCategory.PartyList:
                        return await _partyListService.HardDeletePartyListAsync(id);

                    case ArchiveCategory.Election:
                        return await _electionService.HardDeleteElectionAsync(id);


                    case ArchiveCategory.Announcement:
                        return await _announcementService.HardDeleteAnnouncement(id);

                    case ArchiveCategory.Facility:
                        return await _facilityService.HardDeleteFacilityAsync(id);


                    case ArchiveCategory.Media:
                        return new OperationalResult
                        {
                            Success = false,
                            Message = "Media folders cannot be permanently deleted from the archive."
                        };

                    default:
                        return new OperationalResult { Success = false, Message = "Unsupported category." };
                }

                await _context.SaveChangesAsync();
                return new OperationalResult { Success = true, Message = "Permanently deleted." };
            }
            catch (DbUpdateException)
            {
                // Safety net for any relationship we didn't explicitly check above —
                // e.g. Attendance records or EvaluationDetails still referencing this row.
                return new OperationalResult
                {
                    Success = false,
                    Message = "This item can't be permanently deleted because other records still reference it."
                };
            }
        }
    }
}
