using Sym_Connect__Capstone2_.Models.Enums;
namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class MediaFolderViewModel
    {
        public int MediaFolderId { get; set; }
        public string FolderName { get; set; }
        public int FileCount { get; set; }
        public DateTime CreatedDate { get; set; }

        public bool IsPrivate { get; set; }

        public bool? isArchived { get; set; }

        public MediaFileStatus MediaFileStatus { get; set; }

        public List<MediaFolderViewModel> Folders { get; set; }
              = new List<MediaFolderViewModel>();

        public string createByMemberName { get; set; } = string.Empty;
    }
}
