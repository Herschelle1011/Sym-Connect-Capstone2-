namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class ArchivedMediaDetailsViewModel
    {
        public int MediaFolderId { get; set; }

        public string FolderName { get; set; } = string.Empty;

        public bool IsPrivate { get; set; }

        public string CreatedBy { get; set; } = "Unknown";

        public DateTime CreatedDate { get; set; }

        public DateTime? ArchiveDate { get; set; }

        public int FileCount { get; set; }

        public List<ArchivedMediaFileViewModel> Files { get; set; } = new();
    }

    public class ArchivedMediaFileViewModel
    {
        public int MediaFileId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileExtension { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public string UploadedBy { get; set; } = "Unknown";

        public DateTime UploadDate { get; set; }
    }
}
