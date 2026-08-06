using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.ViewModels.Member;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Controllers
{
   
    public class YouthMembersController : Controller
    {
        private readonly IMemberService _memberService;

        public YouthMembersController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        // GET: YouthMembers
        public async Task<IActionResult> Index()
        {
            var members = await _memberService.GetAllMembersAsync();

            var viewModel = members.Select(member => new MemberListViewModel
            {
                MemberId = member.MemberId,
                Email = member.Email,
                FirstName = member.FirstName,
                MiddleName = member.MiddleName,
                LastName = member.LastName,
                Status = member.Status,
                Address = member.Address,
                BirthDate = member.BirthDate,
                ContactNumber = member.ContactNumber,
                Gender = member.Gender,
                DateRegistered = member.DateRegistered,
                GroupDetails = member.GroupDetails
            }).ToList();

            return View(viewModel);
        }
        // GET: YouthMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _memberService.GetMemberByIdAsync(id.Value);

            if(member == null)
            {
                return NotFound();
            }

            var getMemberDetails = new MemberListViewModel
            {
                MemberId = member.MemberId,
                Email = member.Email,
                FirstName = member.FirstName,
                MiddleName = member.MiddleName,
                LastName = member.LastName,
                Status = member.Status,
                Address = member.Address,
                BirthDate = member.BirthDate,
                ContactNumber = member.ContactNumber,
                Gender = member.Gender,
            };

            return View(getMemberDetails);
        }

        // GET: YouthMembers/Create
        public IActionResult Create()
        {
            return PartialView("_CreateMemberForm", new MembersCreateViewModel());

        }

        // POST: YouthMembers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MembersCreateViewModel vm , IFormFile? Photo)
        {
            if (!ModelState.IsValid)
            {
                return View("_CreateMemberForm", vm);
            }

            bool success = await _memberService.CreateMember(vm, Photo);

            if (!success)
            {
                ModelState.AddModelError("", "Failed to create member.");
                return View(vm);
            }
            return RedirectToAction("Index");
        }

        // GET: YouthMembers/Edit/5
        public async Task<IActionResult> Edit(int? id, IFormFile? photo)
        {
            if (id == null)
            {
                return NotFound();
            }
            var member = await _memberService.GetMemberByIdAsync(id.Value);
            if (member == null) return NotFound();

            var vm = new MembersEditViewModel
            {
                memberId = member.MemberId,
                Email = member.Email,
                FirstName = member.FirstName,
                MiddleName = member.MiddleName,
                LastName = member.LastName,
                ContactNumber = member.ContactNumber,
                Address = member.Address,
                BirthDate = member.BirthDate,
                Gender = member.Gender,
                Status = member.Status,
                photoUrl = member.ProfilePictureUrl,
            };

            return PartialView("_EditMemberForm", vm);
        }

        // POST: YouthMembers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MembersEditViewModel vm, int id, IFormFile? photo)
        {
            if (vm.memberId != id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View("_EditMemberForm", vm);
            }

            try
            {
                await _memberService.UpdateMemberAsync(vm, photo);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await MemberExists(vm.memberId))
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


        // GET: YouthMembers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            var viewModel = new MemberDeleteViewModel
            {
                memberId = member.MemberId,
                Email = member.Email,
                FullName = member.FullName,
                groupName = string.Join(", ",
                member.GroupDetails
                      .Where(g => g.Group != null)       // skip any orphaned records
                      .Select(g => g.Group!.GroupName))  // ! tells C# "I've already checked"
            };


            return PartialView("_DeleteMemberForm", viewModel);
        }

        // POST: YouthMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);

            if (member != null)
            {
                await _memberService.DeleteMemberAsync(member.MemberId); 
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> MemberExists(int id)
        {
            return await _memberService.MemberExists(id);
        }
    }
}
