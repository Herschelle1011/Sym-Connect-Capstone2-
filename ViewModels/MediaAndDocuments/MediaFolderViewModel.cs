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
    }
}
