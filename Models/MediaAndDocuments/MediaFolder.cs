namespace Sym_Connect__Capstone2_.Models.MediaAndDocuments
{
    public class MediaFolder
    {
        public int MediaFolderId { get; set; }
        public string FolderName { get; set; }

        public DateTime CreatedDate { get; set; }
        public int CreatedByMemberId { get; set; }

        public bool? isArchived { get; set; }
        public DateTime? ArchiveDate { get; set; }

        public bool IsPrivate { get; set; }

        public ICollection<MediaFile> Files { get; set; } = new List<MediaFile>();
    }
}
