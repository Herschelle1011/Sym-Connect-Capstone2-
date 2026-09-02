using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Implementation;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;
using Sym_Connect__Capstone2_.Helpers;

namespace Sym_Connect__Capstone2_.Controllers
{
    [Authorize]

    public class MediaAndDocumentsController : Controller
    {
        private readonly IMediaService _mediaService;
        private readonly IMemberService _memberService;
        private readonly IGroupService _groupService;

        private readonly UserManager<User> _userManager;

        public MediaAndDocumentsController(IMediaService mediaService,
        IMemberService memberService, UserManager<User> userManager, IGroupService groupService)
        {
            _mediaService = mediaService;
            _memberService = memberService;
            _userManager = userManager;
            _groupService = groupService;
        }

        public async Task<IActionResult> Index()
        {
            var (memberId, isAdmin, _) = await ResolveCurrentUserAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });


            if (!isAdmin && memberId == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" }
                );
            }


            var recentFiles = await _mediaService.RecentFiles(memberId.Value ,isAdmin);
            var folders = await _mediaService.GetAllFoldersAsync(memberId.Value, isAdmin);
            var vm = new MediaAndDocumentsIndexViewModel
            {
                Folders = folders,
                RecentFiles = recentFiles
            };

            return View(vm);
        }


        public async Task<IActionResult> FolderDetails(int? id)
        {
            var (memberId, isAdmin, _) = await ResolveCurrentUserAsync();
            if (memberId == null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            if (id.HasValue)
            {
                bool canView = await _mediaService.CanViewFolderAsync(id.Value, memberId.Value, isAdmin);
                if (!canView) return Forbid();
            }
            var files = await _mediaService.GetFilesInFolderAsync(id);
            var folder = await _mediaService.GetFolderByIdAsync(id.Value);


            ViewBag.FolderId = id;
            ViewBag.FolderName = folder.FolderName;

            return View(files);
        }

        [HttpGet]
        public async Task<IActionResult> Rename(int id)
        {
            var folder = await _mediaService.GetFolderByIdAsync(id);
            if (folder == null) return NotFound();

            var vm = new MediaFolderRenameViewModel
            {
                MediaFolderId = id,
                FolderName = folder.FolderName
            };

            return PartialView("_RenameFolderModal", vm);

        }

        [HttpGet]
        public async Task<IActionResult> RenameFile(int id)
        {
            var file = await _mediaService.GetFileByIdAsync(id);
            if (file == null) return NotFound();

            var vm = new MediaFileRenameViewModel
            {
                MediaFileId = id,
                FileName = file.FileName
            };

            return PartialView("_RenameFileModal", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> RenameFile(int id, string name)
        {
            var result = await _mediaService.RenameFileAsync(id, name);
            return Json(new { success = result.Success, message = result.Message });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Rename(int id, string name)
        {
            var result = await _mediaService.RenameAsync(id, name);
         

            return Json(new { success = result.Success, message = result.Message });

        }

        //DOWNLOAD
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var (memberId, isAdmin, _) = await ResolveCurrentUserAsync();

            if(memberId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            if (!isAdmin && memberId == null)
            {
                return RedirectToPage(
                    "/Account/Login",
                    new { area = "Identity" }
                );
            }


            var file = await _mediaService.GetFileForDownloadAsync(id, memberId.Value, isAdmin);

            if(file == null)
            {
                return NotFound();
            }
            using var httpClient = new HttpClient();
            var fileBytes = await httpClient.GetByteArrayAsync(file.FileUrl);
            string contentType = GetContentType(file.FileExtension);

            return File(fileBytes, contentType, file.FileName);

        }



        // GET CREATE FOLDER MODAL 
        [HttpGet]
        public IActionResult CreateFolder()
        {
            return PartialView("_CreateFolderModal", new MediaFolderCreateViewModel());
        }


        //ADMIN OR LEADERS ONLY (CREATE)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFolder(MediaFolderCreateViewModel vm)
        {

            var (memberId, isAdmin, isLeader) = await ResolveCurrentUserAsync();
            if (memberId is null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            if (!isAdmin && !isLeader) return Forbid();

            var result = await _mediaService.CreateFolderAsync(vm.FolderName, memberId.Value, vm.IsPrivate);
            return Json(new { success = result.Success, message = result.Message });
        }


        //===GET UPLOAD FILES ====

        public IActionResult UploadFiles(int folderId)
        {
            return PartialView("_UploadFilesModal", new MediaFileUploadViewModel
            {
                MediaFolderId = folderId
            });
        }



        //=== UPLOAD FILES === ADMIN OR LEADER ONLY 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(MediaFileUploadViewModel vm)
        {
            Console.WriteLine($"Folder ID: {vm.MediaFolderId}");


            var (memberId, isAdmin, isLeader) = await ResolveCurrentUserAsync();
            if (memberId is null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            if (!isAdmin && !isLeader) return Forbid();

            var result = await _mediaService.UploadFilesAsync(vm.MediaFolderId, vm.Files, memberId.Value);
            return Json(new { success = result.Success, message = result.Message });
        }

        //=== DELETE FILE ====

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteFile(int mediaFileId)
        {
            var (memberId, isAdmin, isLeader) = await ResolveCurrentUserAsync();
            if (memberId is null)
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            if (!isAdmin && !isLeader) return Forbid();

            var result = await _mediaService.DeleteFileAsync(mediaFileId, memberId.Value, isAdmin);
            return Json(new { success = result.Success, message = result.Message });
        }

        private async Task<(int? MemberId, bool IsAdmin, bool IsLeader)>
            ResolveCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return (null, false, false);

            bool isAdmin =
                await _userManager.IsInRoleAsync(user, "Admin");

            // Admin may not have a MemberId
            if (user.MemberId == null)
                return (null, isAdmin, false);

            var member =
                await _memberService.GetMemberByIdAsync(user.MemberId.Value);

            if (member == null)
                return (null, isAdmin, false);

            bool isLeader =
                await _groupService.IsMemberALeaderAsync(member.MemberId);

            return (member.MemberId, isAdmin, isLeader);
        }


        private async Task<bool> IsCurrentUserAdminAsync()
        {
            var user = await _userManager.GetUserAsync(User);



            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, "Admin");
        }


        [HttpGet]
        public async Task<IActionResult> GetArchive(int id)
        {
            var (memberId, isAdmin, _) = await ResolveCurrentUserAsync();

            if (!isAdmin && memberId == null) return Forbid();

            var vm = await _mediaService.GetFolderArchive(id, memberId ?? 0, isAdmin);

            if (vm == null) return NotFound();

            return PartialView("_ArchiveMediaModal", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveFolder(int id)
        {
            var (memberId, isAdmin, isLeader) =
           await ResolveCurrentUserAsync();

            if (!isAdmin && memberId == null)
            {
                return Forbid();
            }

            if (!isAdmin && !isLeader)
            {
                return Forbid();
            }

            var result = await _mediaService.ArchiveAsync(
               id,
               memberId ?? 0,
               isAdmin
           );

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }



        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",

                ".pdf" => "application/pdf",

                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",

                _ => "application/octet-stream"
            };
        }

    }
}
