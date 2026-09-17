using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class MediaAndDocumentsController : Controller
    {
        private readonly IMediaService _mediaService;
        private readonly ICurrentUserContext _currentUser;

        public MediaAndDocumentsController(
            IMediaService mediaService, ICurrentUserContext currentUser)
        {
            _mediaService = mediaService;
            _currentUser = currentUser;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var folders =
                await _mediaService
                    .GetPublicAdminFoldersForMemberAsync();

            var viewModel = new MediaAndDocumentsIndexViewModel
            {
                Folders = folders
            };


            return View(viewModel);
        }



        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var files =
                await _mediaService
                    .GetPublicAdminFolderFilesForMemberAsync(id);

            if (files == null || files.Count == 0)
            {
    
                ViewBag.FolderId = id;
            }

            return View(files);
        }

        public async Task<IActionResult> FileInformation(int? id)
        {
            if (id == null) return NotFound();
            var fileInfo = await _mediaService.FileInformation(id.Value);

            return PartialView("_ViewFileInformationModal", fileInfo);
        }


        //DOWNLOAD
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            // Get the currently logged-in leader
            var (memberId, ok) =
                await RequireMemberAsync();

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

        private async Task<(int? memberId, bool ok)> RequireMemberAsync()
        {
            var (memberId, isAdmin, isLeader) =
                await _currentUser.ResolveAsync(User);

            if (memberId == null)
            {
                return (memberId, false);
            }

            return (memberId, true);
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


    }
}