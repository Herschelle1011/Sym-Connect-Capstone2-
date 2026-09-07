// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Membership;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Sym_Connect__Capstone2_.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly CloudinaryService _cloudinaryService;


        public RegisterModel(
          UserManager<User> userManager,
          AppDbContext context,
          IEmailService emailService, CloudinaryService cloudinaryService)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public MembershipRegistrationViewModel Input { get; set; } = new();

        public async Task<IActionResult> OnPostSendCodeAsync(
     [FromBody] EmailVerificationRequestViewModel request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Please enter your email address."
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "This email is already registered."
                });
            }

            var code = Random.Shared.Next(100000, 999999).ToString();

            var existingVerification =
                await _context.EmailVerifications
                    .FirstOrDefaultAsync(v => v.Email == request.Email);

            if (existingVerification == null)
            {
                existingVerification = new EmailVerification
                {
                    Email = request.Email,
                    VerificationCode = code,
                    ExpiresAt = DateTime.Now.AddMinutes(10),
                    IsVerified = false
                };

                _context.EmailVerifications.Add(existingVerification);
            }
            else
            {
                existingVerification.VerificationCode = code;
                existingVerification.ExpiresAt = DateTime.Now.AddMinutes(10);
                existingVerification.IsVerified = false;
            }

            await _context.SaveChangesAsync();

            await _emailService.SendVerificationCodeAsync(
                request.Email,
                code);

            return new JsonResult(new
            {
                success = true,
                message = "Verification code sent."
            });
        }


        public async Task<IActionResult> OnPostVerifyCodeAsync(
     [FromBody] VerifyEmailCodeViewModel request)
        {
            var verification =
                await _context.EmailVerifications
                    .FirstOrDefaultAsync(v =>
                        v.Email == request.Email &&
                        v.VerificationCode == request.Code);

            if (verification == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Invalid verification code."
                });
            }

            if (verification.ExpiresAt < DateTime.Now)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Verification code has expired."
                });
            }

            verification.IsVerified = true;

            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                success = true,
                message = "Email verified successfully."
            });
        }

        public async Task<IActionResult> OnPostAsync(MembershipRegistrationViewModel registration, IFormFile? ProfilePhoto)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Please complete all required fields."
                });
            }

            // 2. Make sure the email was verified
            var emailVerification = await _context.EmailVerifications
                .FirstOrDefaultAsync(v =>
                    v.Email == registration.Email &&
                    v.IsVerified);

            if (emailVerification == null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "Please verify your email address first."
                });
            }


            // 3. Check if email already exists
            var existingEmailUser =
                await _userManager.FindByEmailAsync(registration.Email);

            if (existingEmailUser != null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "This email address is already registered."
                });
            }


            // 4. Check if username already exists
            var existingUsernameUser =
                await _userManager.FindByNameAsync(registration.Username);

            if (existingUsernameUser != null)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "This username is already taken."
                });
            }


            // 5. Create Identity User
            var user = new User
            {
                UserName = registration.Username,
                Email = registration.Email,

                DisplayName =
                    $"{registration.FirstName} {registration.LastName}",

                EmailConfirmed = true
            };


            var createUserResult =
               await _userManager.CreateAsync(
                   user,
                   registration.Password
               );

            if (!createUserResult.Succeeded)
            {
                var errorMessage =
                    createUserResult.Errors
                        .FirstOrDefault()?.Description
                    ?? "Unable to create account.";

                return new JsonResult(new
                {
                    success = false,
                    message = errorMessage
                });
            }

            // Optional profile photo
            if (ProfilePhoto != null &&
                ProfilePhoto.Length > 0)
            {
                var allowedExtensions =
                    new[] { ".jpg", ".jpeg", ".png" };

                var extension =
                    Path.GetExtension(ProfilePhoto.FileName)
                        .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    await _userManager.DeleteAsync(user);

                    return new JsonResult(new
                    {
                        success = false,
                        message = "Profile photo must be JPG or PNG."
                    });
                }


                const long maxFileSize =
                    2 * 1024 * 1024;

                if (ProfilePhoto.Length > maxFileSize)
                {
                    await _userManager.DeleteAsync(user);

                    return new JsonResult(new
                    {
                        success = false,
                        message = "Profile photo must not be larger than 2 MB."
                    });
                }


                var profilePictureUrl =
                    await _cloudinaryService
                        .UploadProfilePicture(
                            ProfilePhoto,
                            user.Id
                        );


                if (string.IsNullOrWhiteSpace(
                    profilePictureUrl))
                {
                    await _userManager.DeleteAsync(user);

                    return new JsonResult(new
                    {
                        success = false,
                        message = "Unable to upload profile photo."
                    });
                }


                user.ProfilePictureUrl =
                    profilePictureUrl;


                await _userManager.UpdateAsync(user);
            }

            // 6. Create Member
            var member = new Sym_Connect__Capstone2_.Models.Member
            {
                FirstName = registration.FirstName,

                MiddleName = registration.MiddleName,

                LastName = registration.LastName,

                BirthDate = registration.DateOfBirth,

                Gender = registration.Gender,

                ContactNumber = registration.ContactNumber,

                Address =
                    $"{registration.Barangay}, " +
                    $"{registration.City}, " +
                    $"{registration.Province}, " +
                    $"{registration.Country}",

                PreferredGroupId = registration.GroupId.Value,


                ApplicationStatus =
                    MembershipApplicationStatus.Pending,

                DateApplied = DateTime.Now

            };


            _context.Members.Add(member);

            await _context.SaveChangesAsync();


            // 7. Connect Identity User to Member
            user.MemberId = member.MemberId;
            await _userManager.UpdateAsync(user);

            await _userManager.UpdateAsync(user);


            // 9. Remove the used email verification record
            _context.EmailVerifications.Remove(
                emailVerification);

            member.UserId = user.Id;
            await _context.SaveChangesAsync();


            // 10. Return success
            return new JsonResult(new
            {
                success = true,
                message =
                    "Your membership application has been submitted and is now pending admin approval."
            });
        }

        public async Task<IActionResult> OnGetGroupsAsync()
        {
            var groups = await _context.Groups
                .AsNoTracking()
                .Where(g => !g.IsArchived)
                .OrderBy(g => g.GroupName)
                .Select(g => new
                {
                    groupId = g.GroupId,
                    groupName = g.GroupName
                })
                .ToListAsync();

            return new JsonResult(groups);
        }

        //REQUEST MODELS
        public class EmailVerificationRequestViewModel
        {
            public string Email { get; set; } = string.Empty;
        }


        public class VerifyEmailCodeViewModel
        {
            public string Email { get; set; } = string.Empty;

            public string Code { get; set; } = string.Empty;
        }
    }
}
