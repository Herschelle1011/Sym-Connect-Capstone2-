using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Implementation;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments;


namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Authorize(Roles = "Leader")]
    [Area("Leader")]
    public class MediaAndDocumentsController : Controller
    {
        private readonly IMediaService _mediaService;
        private readonly ICurrentUserContext _currentUser;
        private readonly UserManager<User> _userManager;
        private readonly IMemberService _memberService;

        public MediaAndDocumentsController(IMediaService mediaService, ICurrentUserContext currentUser, UserManager<User> userManager, IMemberService memberService)
        {
            _mediaService = mediaService;
            _currentUser = currentUser;
            _userManager = userManager;
            _memberService = memberService;
        }


            private async Task<(int? memberId, bool ok)> RequireLeaderAsync()
                {
                   var (memberId, isAdmin, isLeader) = await _currentUser.ResolveAsync(User);
                if (memberId == null || !isLeader) return (memberId, false);
                return (memberId, true);
                 }

    public async Task<IActionResult> Index()
    {
        var (memberId, ok) = await RequireLeaderAsync();
        if (!ok) return Forbid();

        var recentFiles = await _mediaService.RecentFiles(memberId.Value, isAdmin: false);
        var folders = await _mediaService.GetAllFoldersAsync(memberId.Value, isAdmin: false);

        var vm = new MediaAndDocumentsIndexViewModel { Folders = folders, RecentFiles = recentFiles };
        return View(vm);
    }

        public async Task<IActionResult> FolderDetails(
            int id,
            int? openFileId)
        {
            var (memberId, ok) = await RequireLeaderAsync();

            if (!ok)
                return Forbid();

            bool canView =
                await _mediaService.CanViewFolderAsync(
                    id,
                    memberId.Value,
                    isAdmin: false
                );

            if (!canView)
                return Forbid();

            var files =
                await _mediaService.GetFilesInFolderAsync(id);

            var folder =
                await _mediaService.GetFolderByIdAsync(id);

            ViewBag.FolderId = id;
            ViewBag.FolderName = folder.FolderName;

            ViewBag.OpenFileId = openFileId;

            return View(files);
        }

        [HttpGet]
        public async Task<IActionResult> FileInformation(int? id)
        {
            if (id == null) return NotFound();
            var fileInfo = await _mediaService.FileInformation(id.Value);

            return PartialView("_ViewFileInformationModal", fileInfo);
        }

        [HttpGet]
    public IActionResult UploadFiles(int folderId)
    {
        return PartialView("_UploadFilesModal", new MediaFileUploadViewModel { MediaFolderId = folderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(MediaFileUploadViewModel vm)
    {
        var (memberId, ok) = await RequireLeaderAsync();
        if (!ok) return Forbid();

        var result = await _mediaService.UploadFilesAsync(vm.MediaFolderId, vm.Files, memberId.Value, IsAdmin: false);
        return Json(new { success = result.Success, message = result.Message });
    }




        //DOWNLOAD
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            // Get the currently logged-in leader
            var (memberId, ok) =
                await RequireLeaderAsync();

            if (!ok)
            {
                return Forbid();
            }


            // Ask the service for the file.
            // isAdmin is false because this is Leader.
            var file =
                await _mediaService.GetFileForDownloadAsync(
                    id,
                    memberId!.Value,
                    isAdmin: false
                );

            if (file == null)
            {
                return NotFound();
            }


            // Download the actual file from Cloudinary/file storage
            using var httpClient = new HttpClient();

            var fileBytes =
                await httpClient.GetByteArrayAsync(file.FileUrl);


            // Tell the browser what kind of file this is
            string contentType =
                GetContentType(file.FileExtension);


            // Send the file to the browser
            return File(
                fileBytes,
                contentType,
                file.FileName
            );
        }

        private string GetContentType(string extension)
        {
            extension = extension.ToLower();

            return extension switch
            {
                ".pdf" => "application/pdf",

                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",

                ".doc" => "application/msword",

                ".docx" =>
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                ".xls" => "application/vnd.ms-excel",

                ".xlsx" =>
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                ".ppt" => "application/vnd.ms-powerpoint",

                ".pptx" =>
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation",

                ".txt" => "text/plain",

                _ => "application/octet-stream"
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetDeleteFile( int mediaFileId)
        {
            var (memberId, ok) =
                await RequireLeaderAsync();

            if (!ok)
            {
                return Forbid();
            }


            var file =
                await _mediaService.GetDeleteFile(
                    mediaFileId
                );

            if (file == null)
            {
                return NotFound();
            }


            return PartialView(
                "_DeleteFileConfirmationModal",
                file
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(
            int mediaFileId)
        {
            var (memberId, ok) =
                await RequireLeaderAsync();

            if (!ok)
            {
                return Json(new
                {
                    success = false,
                    message = "You are not allowed to delete this file."
                });
            }


            var result =
                await _mediaService.DeleteFileAsync(
                    mediaFileId,
                    memberId!.Value,
                    isAdmin: false
                );


            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        

    }
}
