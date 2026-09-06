using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models.MediaAndDocuments
{
    public class MediaFolder
    {
        public int MediaFolderId { get; set; }
        public string FolderName { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey(nameof(Member))]
        public int CreatedByMemberId { get; set; }

        public Member? Member { get; set; } = null!;

        public bool? isArchived { get; set; }
        public DateTime? ArchiveDate { get; set; }

        public bool IsPrivate { get; set; }

        public ICollection<MediaFile> Files { get; set; } = new List<MediaFile>();
    }
}
