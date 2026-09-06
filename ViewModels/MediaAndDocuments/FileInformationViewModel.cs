using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class FileInformationViewModel
    {
        public int FileId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileExtension { get; set; } = string.Empty;

        public string Size { get; set; }

        public string UploadedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public MediaFileStatus Status { get; set; }

        public string Visibility { get; set; }

    }
}
