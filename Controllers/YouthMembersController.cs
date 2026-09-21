using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Account;
using Sym_Connect__Capstone2_.ViewModels.Emails;
using Sym_Connect__Capstone2_.ViewModels.Member;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Sym_Connect__Capstone2_.Controllers
{

    [Authorize(Roles = "Admin")]
    public class YouthMembersController : Controller
    {
        private readonly IMemberService _memberService;
        private readonly IEmailService _emailService;
        private readonly IRazorViewRenderer _razorViewRenderer;

        public YouthMembersController(IMemberService memberService, IEmailService emailService, IRazorViewRenderer razorViewRenderer)
        {
            _memberService = memberService;
            _emailService = emailService;
            _razorViewRenderer = razorViewRenderer;
        }




        // GET: YouthMembers
        public async Task<IActionResult> Index(int? openApplicantId)
        {
            var vm = new MembersPageViewModel
            {
                Members = (await _memberService.GetAllMembersAsync()).ToList(),
                Applicants = await _memberService.AllMemberApplicants()
            };

            if (openApplicantId.HasValue)
            {
                var stillPending = await _memberService.GetMemberForApprovalAsync(openApplicantId.Value);

                ViewBag.OpenApplicantId = stillPending != null ? openApplicantId : null;
            }

            ViewBag.OpenApplicantId = openApplicantId;

            return View(vm);
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
                Email = member.User.Email ?? "Unverified",
                FirstName = member.FirstName,
                MiddleName = member.MiddleName,
                LastName = member.LastName,
                Status = member.Status,
                Country = member.Country,
                Province = member.Province,
                City = member.City,
                Address = member.Address,
                Barangay = member.Barangay,
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
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = string.Join(" ", errors)
                });
            }

            var result = await _memberService.CreateMember(vm, Photo);

            if (!result.Success)
            {
                return Json(new
                {
                    success = false,
                    message = result.Message
                });
            }

            //confirmation LINK
            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "YouthMembers",
                new
                {
                    userId = result.UserId,
                    token = result.EmailConfirmationToken
                },
                  "https",
                    "excuse-fastball-crunchy.ngrok-free.dev");

            if (confirmationLink != null)
            {
                var emailModel = new MemberVerificationEmailViewModel
                {
                    FirstName = vm.FirstName,
                    ConfirmationLink = confirmationLink
                };

                var emailBody =
                     await _razorViewRenderer.RenderViewToStringAsync(
                          "EmailTemplates/MemberVerification",
                           emailModel
                    );
                try
                {


                    await _emailService.SendEmailAsync(
                        vm.Email,
                        "Verify your SYM-Connect email",
                        emailBody
                    );
                }
                catch
                {
                    return Json(new
                    {
                        success = false,
                        message = "Member was created, but the verification email could not be sent."
                    });
                }
            }


            return Json(new
            {
                success = true,
                message = result.Message,                
            });
        }

        //===  CONFIRM EMAIL PART === 

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(
            string userId,
            string token)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Invalid confirmation link.");
            }

            var result =
                await _memberService.ConfirmEmailAsync(userId, token);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return RedirectToAction("SetPassword", new
            {
                userId = result.UserId,
                token = result.PasswordToken,
            });
        }

        [HttpGet]
        public IActionResult SetPassword(
                    string userId,
                    string token)
        {
            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Invalid password setup link.");
            }

            var vm = new SetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(vm);
        }


        public async Task<IActionResult> PasswordCreated()
        {
            return View();
        }

        //ADD PASSWORD ==== 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(
                  SetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _memberService.SetPasswordAsync(
                vm.UserId,
                vm.Token,
                vm.Password
            );

            if (!result.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message
                );

                return View(vm);
            }

            return RedirectToAction(nameof(PasswordCreated));
        }





        //======= MEMBERSHIP =========
        //HTTPGET
        public async Task<IActionResult> GetAllApplicants()
        {
            var applicant = await _memberService.AllMemberApplicants();
            return View(applicant);
        }
        //HTTPGET
        public async Task<IActionResult> GetApplicationInformation(int id)
        {
            var applicant = await _memberService.GetApplicantInformationByIdAsync(id);
     
            return PartialView("_ApplicationInformation", applicant);
        }
        //HTTPGET

        public async Task<IActionResult> ApproveModal(int id)
        {
            var getMemberForApproval = await _memberService.GetMemberForApprovalAsync(id);
            if (getMemberForApproval == null)
            {
                return NotFound();
            }
            return PartialView("_ApplicationApproveModal", getMemberForApproval);
        }
        //HTTPGET
        public async Task<IActionResult> RejectModal(int id)
        {
            var getMemberForApproval = await _memberService.GetMemberForRejectionAsync(id);
            if (getMemberForApproval == null)
            {
                return NotFound();
            }
            return PartialView("_RejectMembershipApplicationModal", getMemberForApproval);
        }



        // POST: YouthMembers/ApproveApplicant/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveApplicant(int id)
        {
            var result = await _memberService.ApproveApplicantAsync(id);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        // POST: YouthMembers/RejectApplicant/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplicant(int id, string reason)
        {
            var result = await _memberService.RejectApplicantAsync(id, reason);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }




        //GET QR GENERATOR
        public async Task<IActionResult> ViewQrCode(int memberId)
        {
            var token = await _memberService.GetOrCreateQrTokenAsync(memberId);
            var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(token, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.PngByteQRCode(qrData);
            byte[] qrImageBytes = qrCode.GetGraphic(20); 
            return File(qrImageBytes, "image/png");
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
                Email = member.User?.Email ?? "",
                IsEmailConfirmed = member.User?.EmailConfirmed ?? false,
                FirstName = member.FirstName,
                MiddleName = member.MiddleName,
                LastName = member.LastName,
                ContactNumber = member.ContactNumber,
                Barangay = member.Barangay,
                City = member.City,
                Province = member.Province,
                Country = member.Country,

                BirthDate = member.BirthDate,
                Gender = member.Gender,
                Status = member.Status,
                PhotoUrl = member.User.ProfilePictureUrl,
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
                Email = member.User?.Email != null ? member.User.Email : "Unverified",
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


        //========= PROFILE PART ===========
        public async Task<IActionResult> Profile(int id)
        {
            var member = await _memberService.GetMemberDetailsByIdAsync(id);
            return PartialView("_ProfileModal", member);
        }


        // GET: 
        public async Task<IActionResult> ArchiveModal(int id)
        {
            var member = await _memberService.GetArchiveMemberAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return PartialView("_ArchiveMemberModal", member);
        }


        // POST: Archives the member
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveMember(int memberId)
        {
            var result = await _memberService.ArchiveMemberAsync(memberId);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
    }
}
