namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class PendingFileItemViewModel
    {
        public int MediaFileId { get; set; }

        public string FileName { get; set; }

        public string FileExtension { get; set; }

        public long FileSizeBytes { get; set; }

        public string UploadedBy { get; set; }

        public DateTime UploadedDate { get; set; }

        public int? DestinationFolderId { get; set; }
        public string DestinationFolderName { get; set; }

        public string FileUrl { get; set; }

    }
}
