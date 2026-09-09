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
    [Authorize(Roles = "Admin")]
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


            ViewBag.PendingFilesCount =
                          await _mediaService.GetPendingFilesCountAsync();

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

            ViewBag.IsAdmin = isAdmin;  

            return View(vm);
        }



        [HttpGet]
        public async Task<IActionResult> PendingFilesCount()
        {
            var count =
                await _mediaService.GetPendingFilesCountAsync();

            return Json(new
            {
                count = count
            });
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
            var result = await _mediaService.RenameFolderAsync(id, name);    

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
            if (!isAdmin) return Forbid();

            var result = await _mediaService.UploadFilesAsync(vm.MediaFolderId, vm.Files, memberId.Value, isAdmin);
            return Json(new { success = result.Success, message = result.Message });
        }


        //=== DELETE FILE ====

        [HttpGet]
        public async Task<IActionResult> GetDeleteFile(int mediaFileId)
        {
            var file = await _mediaService.GetDeleteFile(mediaFileId);
            return PartialView("_DeleteFileModalConfirmation", file);
        }


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

        [HttpGet]
        public async Task<IActionResult> GetAvailableFolders(int currentFolderid, int fileId)
        {
            var folders = await _mediaService.GetAvailableFolderAsync(currentFolderid);
            var vm = new MediaFolderViewModel
            {
                Folders = folders,
            };

            ViewBag.FileId = fileId; 
            return PartialView("_AvailableFoldersModal", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveFileAsync(int fileid, int folderId)
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

            var result = await _mediaService.MoveFile(fileid, folderId, memberId ?? 0, isAdmin);
            return Json(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> FileInformation(int? id)
        {
            if (id == null) return NotFound();
            var fileInfo = await _mediaService.FileInformation(id.Value);
           
            return PartialView("_ViewFileInformationModal", fileInfo);
        }



        //=== PENDING FILES QUEUE === ADMIN ONLY
        [HttpGet]
        public async Task<IActionResult> PendingFiles(int? openFileId)
        {
            var (memberId, isAdmin, _) = await ResolveCurrentUserAsync();
            if (!isAdmin) return Forbid();

            var vm = await _mediaService.GetPendingFiles();
            ViewBag.OpenFileId = openFileId;

            return View(vm);
        }

        //MODAL OPENER FOR VIEW BUTTON
        public async Task<IActionResult> PendingFileInformation(int id)
        {
            var (memberId, isAdmin, _) = await ResolveCurrentUserAsync();
            if (!isAdmin) return Forbid();

            var vm = await _mediaService.GetPendingFilesDetails(id);
            if (vm == null) return NotFound();
            return PartialView("_ViewPendingFileInformationModal", vm);
        }

        //=== APPROVE FILE === ADMIN ONLY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFile(int mediaFileId)
        {
            var (memberId, isAdmin, _) = await ResolveCurrentUserAsync();
            if (!isAdmin) return Forbid();

            var result = await _mediaService.ApproveFileAsync(mediaFileId, memberId ?? 0);
            return Json(new { success = result.Success, message = result.Message });
        }

        //=== REJECT FILE (POST) === ADMIN ONLY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectFile(int mediaFileId, string reason)
        {
            var (memberId, isAdmin, _) = await ResolveCurrentUserAsync();
            if (!isAdmin) return Forbid();

            var result = await _mediaService.RejectFileAsync(mediaFileId, reason, memberId ?? 0);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> ChangeVisibility(int folderId)
        {
            var (_, isAdmin, _) = await ResolveCurrentUserAsync();
            if (!isAdmin) return Forbid();

            var folder = await _mediaService.GetFolderByIdAsync(folderId);
            if (folder == null) return NotFound();

            var vm = new ChangeVisibilityViewModel
            {
                MediaFolderId = folder.MediaFolderId,
                FolderName = folder.FolderName,
                IsPrivate = folder.IsPrivate
            };

            return PartialView("_ChangeVisibilityModal", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeVisibility(
                int folderId,
                bool isPrivate)
        {
            var (_, isAdmin, _) = await ResolveCurrentUserAsync();

            if (!isAdmin)
                return Forbid();

            var result = await _mediaService.ChangeVisibility(
                folderId,
                isPrivate
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
