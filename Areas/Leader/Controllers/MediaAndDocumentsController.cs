using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public MediaAndDocumentsController(IMediaService mediaService, ICurrentUserContext currentUser)
        {
            _mediaService = mediaService;
            _currentUser = currentUser;
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
        var (memberId, ok) = await RequireLeaderAsync();
        if (!ok) return Forbid();

        var result = await _mediaService.DeleteFileAsync(mediaFileId, memberId.Value, isAdmin: false);
        return Json(new { success = result.Success, message = result.Message });
    }
    }
}
