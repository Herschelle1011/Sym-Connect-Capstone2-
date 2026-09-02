using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums.AnnouncementEnums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Announcement;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace Sym_Connect__Capstone2_.Controllers
{

    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IGroupService _groupService;
        private readonly UserManager<User> _userManager;



        public AnnouncementsController(IAnnouncementService announcementService, IGroupService groupService, UserManager<User> userManager)
        {
            _announcementService = announcementService;
            _groupService = groupService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _announcementService.GetAllAnnouncementAsync();
            return View(announcements);
        }

        //==== HTTP GET ==== CREATE
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new AnnouncementViewModel { GroupOptions = await GetGroupSelectListAsync() };
            return PartialView("_AnnouncementFormCreate", vm);
        }

        //=== HTTP POST ==== CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(AnnouncementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.GroupOptions = await GetGroupSelectListAsync();
                return PartialView("_AnnouncementFormCreate", vm);
            }

            int? createdByMemberId = HttpContext.Session.GetInt32("CurrentMemberId");

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Unauthorized();

            var result = await _announcementService.CreateAsync(vm, user.Id);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var a = await _announcementService.GetByIdAsync(id);
            if (a == null) return NotFound();

            var vm = await _announcementService.GetEditForm(id);
            return PartialView("_AnnouncementFormEdit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(AnnouncementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.GroupOptions = await GetGroupSelectListAsync();
                return PartialView("_AnnouncementFormEdit", vm);
            }

            var result = await _announcementService.UpdateAsync(vm);
            return Json(new { success = result.Success, message = result.Message });
        }



        [HttpGet]
        public async Task<IActionResult> Archive(int? id)
        {
            var announcement = await _announcementService.GetByIdAsync(id.Value);
            if (announcement == null) return NotFound();

            var vm = new ArchiveAnnouncementViewModel
            {
                AnnouncementId = announcement.AnnouncementId,
                Title = announcement.Title,
                TargetType = announcement.TargetAudience.ToString(),
                CreatedAt = announcement.CreatedAt
            };

            return PartialView("_ArchiveAnnouncementModal", vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            await _announcementService.ArchiveAsync(id);
            return Json(new { success = true, message = "Announcement archived successfully"});
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            await _announcementService.RestoreAsync(id);
            return Json(new { success = true });
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
    }
}
