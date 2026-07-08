using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Services;
using Sym_Connect__Capstone2_.Models;

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
            var groups = await _groupService.GetAllGroupsAsync();

            var viewModel = groups.Select(g => new GroupListViewModel
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                Status = g.Status,
                LeaderName = g.Leader != null ? g.Leader.FullName : "No Leader",
                LeaderEmail = g.Leader !=null ? g.Leader.Email : "N/A",
                Members = g.GroupDetails
                        .Where(e => e.Member != null)
                        .Select(e => new GroupMemberViewModel
                        {
                            FullName = e.Member != null ? e.Member.FullName.Trim() : "N/A"
                        }).ToList()
            });

            return View(viewModel);
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var getGroupDetails = await _groupService.GetGroupByIdAsync(id.Value);

            if (getGroupDetails == null)
            {
                return NotFound();
            }

            var viewModel = new GroupDetailsViewModel
            {
                GroupId = getGroupDetails.GroupId,
                GroupName = getGroupDetails.GroupName,
                Description = getGroupDetails.Description,
                Status = getGroupDetails.Status,
                LeaderName = getGroupDetails.Leader?.FullName ?? "No Leader",
                LeaderContact = getGroupDetails.Leader?.ContactNumber ?? "N/A",
                LeaderEmail = getGroupDetails.Leader?.Email ?? "N/A",

                Members = getGroupDetails.GroupDetails
                    .Where(gd => gd.Member != null)
                    .Select(gd => new GroupMemberViewModel
                    {
                        MemberId = gd.MemberId ?? 0,
                        FullName = gd.Member!.FullName,
                        Email = gd.Member.Email,
                        ContactNumber = gd.Member.ContactNumber,
                        Status = gd.Member.Status,
                        DateRegistered = gd.Member.DateRegistered
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: Groups/Create
        public async Task<IActionResult> Create()
        {
            var vm = new GroupCreateViewModel
            {
                LeaderOptions = await _groupService.GetLeaderOptionsAsync()
            };

            return PartialView("_CreateGroupForm", vm);
        }



        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                vm.LeaderOptions = await _groupService.GetLeaderOptionsAsync(vm.LeaderId); //load
                return PartialView("_CreateGroupForm", vm);
            }
          
            var group = new Group
            {                
                GroupName = vm.GroupName,
                Description = vm.Description,
                LeaderId = vm.LeaderId ?? 0,
                Status = vm.Status,
            };
           await _groupService.CreateGroupAsync(group);

            return RedirectToAction(nameof(Index));
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vm = new GroupEditViewModel {
               LeaderOptions = await _groupService.GetLeaderOptionsAsync()

            };
            return View(vm);
        }

        //GET CreateForm
        public async Task<IActionResult> GetCreateForm()
        {
            var vm = new GroupCreateViewModel
            {
                LeaderOptions = await _groupService.GetLeaderOptionsAsync()
            };

            return PartialView("_CreateGroupForm", vm);
        }

        //GET EDIT FORM
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
                LeaderId = group.LeaderId,
                LeaderOptions = await _groupService.GetLeaderOptionsAsync()
            };

            return PartialView("_EditGroupForm", vm);
        }


        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupEditViewModel vm)
        {
            if ( vm.GroupId == 0) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.LeaderOptions = await _groupService.GetLeaderOptionsAsync(vm.LeaderId);
                return View(vm);
            }
            var group = new Group
            {
               GroupId = vm.GroupId,
               GroupName = vm.GroupName,
               Description = vm.Description,
               LeaderId = vm.LeaderId ?? 0,
               Status = vm.Status,
            };
            try
            {
                await _groupService.UpdateGroupAsync(group);
            }
            catch
            {
                if (! await _groupService.GroupExists(vm.GroupId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

    

        // GET: Groups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            //get group id first from the GetGroupByIdAsync method in GroupService.cs
            var group = await _groupService.GetGroupByIdAsync(id.Value);

            if (group == null) return NotFound(); //valodation

            //get information to confirm deletion
            var vm = new GroupDeleteViewModel
            {
                GroupId = group.GroupId,
                GroupName = group.GroupName,
                LeaderName = group.Leader != null ? group.Leader.FullName : "No Leader",
                Status = group.Status,
                TotalMembers = group.GroupDetails.Count()
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
                LeaderName = group.Leader != null ? group.Leader.FullName : "No Leader",
                Status = group.Status,
                TotalMembers = group.GroupDetails.Count()
            };

            return PartialView("_DeleteGroupForm", vm);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
                await _groupService.DeleteGroupAsync(id);     

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

            foreach (var memberId in memberIds)
            {
                await _groupService.AddMemberToGroupAsync(groupId, memberId);
            }

            return RedirectToAction(nameof(Details), new { id = groupId });
        }

        //SEARCH MEMBERS
        [HttpGet]
        public async Task<IActionResult> SearchMembers(string query, int groupId)
        {
            var available = await _groupService.GetAvailableMemberAsync(groupId);

            var results = available
                .Where(m =>
                    m.FullName.Contains(query ?? "", StringComparison.OrdinalIgnoreCase) ||
                    m.Email.Contains(query ?? "", StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .Select(m => new
                {
                    m.MemberId,
                    m.FullName,
                    m.Email
                });

            return Json(results);
        }

    }
}
