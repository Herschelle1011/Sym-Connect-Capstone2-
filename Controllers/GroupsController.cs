using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Group;

namespace Sym_Connect__Capstone2_.Controllers
{
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        // GET: Groups
        public async Task<IActionResult> Index()
        {
            // GetAllGroupsAsync already returns fully-mapped GroupListViewModel — no remapping needed
            var groups = await _groupService.GetAllGroupsAsync();
            return View(groups);
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var group = await _groupService.GetGroupByIdAsync(id.Value);
            if (group == null) return NotFound();

            // GetGroupByIdAsync already returns a fully-mapped GroupDetailsViewModel
            return View(group);
        }

        // GET: Groups/Create
        public async Task<IActionResult> GetCreateForm()
        {
            var vm = new GroupCreateViewModel(); 

            return PartialView("_CreateGroupForm", vm);
        }

        [HttpGet]
        public async Task<IActionResult> SelectLeaderModal(string? search = null, int? excludeGroupId = null)
        {
            var leaders = await _groupService.GetAvailableLeadersAsync(search, excludeGroupId); // renamed
            return PartialView("_SelectLeaderModal", leaders);
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupCreateViewModel vm)
        {
            if (await _groupService.GroupNameExists(vm.GroupName))
            {
                ModelState.AddModelError("GroupName", "GroupName already exists.");
            }

            if (!ModelState.IsValid)
            {
                return PartialView("_CreateGroupForm", vm);
            }

            var result = await _groupService.CreateGroupAsync(vm);
            return Json(result);
        }

        // GET: Groups/Edit/5 (kept as-is — note this loads an empty form, not the group's own data;
        // GetEditForm below is the one that actually populates fields)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vm = new GroupEditViewModel();
            
            return View(vm);
        }

        // GET EDIT FORM
        public async Task<IActionResult> GetEditForm(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null) return NotFound();

            var vm = new GroupEditViewModel
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                Description = group.Description,
                Status = group.Status,
                LeaderId = group.LeaderId
           
            };

            return PartialView("_EditGroupForm", vm);
        }

        // POST: Groups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupEditViewModel vm)
        {
            if (vm.GroupId == 0) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _groupService.UpdateGroupAsync(vm);

            if (result == null || !result.Success)
            {
                if (!await _groupService.GroupExists(vm.GroupId))
                {
                    return NotFound();
                }
                ModelState.AddModelError(string.Empty, result?.Message ?? "Update failed.");
                return Json(result);
            }

            return Json(result);

        }

        // GET: Groups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var group = await _groupService.GetGroupByIdAsync(id.Value);
            if (group == null) return NotFound();

            var vm = new GroupDeleteViewModel
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                LeaderName = group.LeaderName,
                Status = group.Status,
                TotalMembers = group.Members.Count
            };
            return View(vm);
        }

        // GET: Load delete confirmation form into modal
        [HttpGet]
        public async Task<IActionResult> GetDeleteForm(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null) return NotFound();

            var vm = new GroupDeleteViewModel
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                LeaderName = group.LeaderName,
                Status = group.Status,
                TotalMembers = group.Members.Count
            };

            return PartialView("_DeleteGroupForm", vm);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _groupService.DeleteGroupAsync(id);

            if (!result.Success) return NotFound();

            TempData["ToastMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        // GET: Load available members for a group
        [HttpGet]
        public async Task<IActionResult> LoadMembers(int groupId)
        {
            var members = await _groupService.GetAvailableMemberAsync(groupId);

            var vm = members.Select(m => new GroupMemberViewModel
            {
                MemberId = m.MemberId,
                FullName = m.FullName,
                Email = m.Email,
                ContactNumber = m.ContactNumber,
                Status = m.Status,
                
                DateRegistered = m.DateRegistered
            }).ToList();

            return PartialView("_AvailableMembersList", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMemberToGroup(int groupId, List<int> memberIds)
        {
            if (groupId == 0 || memberIds == null || !memberIds.Any())
                return BadRequest();

            OperationalResult? lastResult = null;
            foreach (var memberId in memberIds)
            {
                lastResult = await _groupService.AddMemberToGroupAsync(groupId, memberId);
            }

            return RedirectToAction(nameof(Details), new { id = groupId });
        }

        // SEARCH MEMBERS
        [HttpGet]
        public async Task<IActionResult> SearchMembers(string query, int groupId)
        {
            var available = await _groupService.GetAvailableMemberAsync(groupId);

            var results = available
                .Where(m =>
                    m.FullName.Contains(query ?? "", StringComparison.OrdinalIgnoreCase) ||
                    (m.Email.Contains(query ?? "", StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(m => m.Points)
                .Take(10)
                .Select(m => new
                {
                    m.MemberId,
                    m.FullName,
                    m.Email,
                    m.Points
                });

            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetRemoveMemberForm(int groupId, int memberId)
        {
            if (groupId == 0 || memberId == 0) return NotFound();

            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null) return NotFound();

            // group.Members is already List<GroupMemberViewModel> — no nested .Member
            var member = group.Members.FirstOrDefault(m => m.MemberId == memberId);
            if (member == null) return NotFound();

            var vm = new RemoveMemberViewModel
            {
                groupId = groupId,
                memberId = memberId,
                MemberName = member.FullName,
                GroupName = group.GroupName
            };

            return PartialView("_RemoveMemberForm", vm);
        }

        [HttpGet]
        public async Task<IActionResult> getTranferMember(int memberId, int groupId)
        {
            if (memberId == 0) return NotFound();

            var groups = await _groupService.GetAllGroupsAsync();
            var viewmodel = new TransferMemberViewModel
            {
                memberId = memberId,
                currentGroupId = groupId,
                availableGroups = groups
                    .Where(g => g.GroupId != groupId)
                    .Select(g => new GroupOptionViewModel
                    {
                        groupId = g.GroupId,
                        groupName = g.GroupName,
                        memberName = g.LeaderName // already flattened, no ?. needed
                    }).ToList()
            };
            return PartialView("_TransferMemberModal", viewmodel);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmedTranferMember(TransferMemberViewModel vm)
        {
            if (vm.memberId == 0 || vm.newGroupId == 0)
            {
                return Json(new OperationalResult { Success = false, Message = "Please select a group." });
            }

            var result = await _groupService.TransferAMemberAsync(vm.memberId, vm.newGroupId);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMemberFromGroupAsync(RemoveMemberViewModel vm)
        {
            if (vm.groupId == 0 || vm.memberId == 0) return BadRequest();

            var result = await _groupService.RemoveMemberFromGroupAsync(vm.groupId, vm.memberId);
            return Json(result);
        }
    }
}