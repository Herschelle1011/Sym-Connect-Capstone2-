namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class MediaFileUploadViewModel
    {
        public int? MediaFolderId { get; set; } 
        public List<IFormFile> Files { get; set; } = new();
    }
}
