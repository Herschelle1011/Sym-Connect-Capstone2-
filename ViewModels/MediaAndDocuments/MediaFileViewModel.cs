using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class MediaFileViewModel
    {
        public int MediaFileId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileExtension { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime UploadedDate { get; set; }
        public int UploadedByMemberId { get; set; }
        public string UploadedBy { get; set; }

        public string fullName { get; set; }

        public bool? IsArchived { get; set; }

        public string folderName { get; set; }

        public int? MediaFolderId { get; set; }

        public MediaFileStatus Status { get; set; }

        public bool IsFolderPrivate { get; set; }
        public int? FolderCreatedByMemberId { get; set; }
    }
}
