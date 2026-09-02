namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class MediaArchiveViewModel
    {
        public int MediaFolderId { get; set; }
        public string folderName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;

        public int FileCount { get; set; }
        public bool IsPrivate { get; set; }

        public List<FilesArchiveViewModel> files { get; set; } = new List<FilesArchiveViewModel>();
    }

    public class FilesArchiveViewModel
    {
        public int fileId { get; set; }
        public string fileName { get; set; } = string.Empty;
        public string Extension { get; set; }


    }
}
