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

        public MediaAndDocumentsController(
            IMediaService mediaService)
        {
            _mediaService = mediaService;
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

    }
}