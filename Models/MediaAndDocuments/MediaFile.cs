using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models.MediaAndDocuments
{
    public class MediaFile
    {
        public int MediaFileId { get; set; }

        public int? MediaFolderId { get; set; }
        public MediaFolder Folder { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileUrl { get; set; } // cloudinary URL
        public string PublicId { get; set; } //to be search

        public string ResourceType { get; set; } // image or raw
        public string FileExtension { get; set; }

        public long FileSizeBytes { get; set; }

        public DateTime UploadDate { get; set; }

        [ForeignKey(nameof(Member))]
        public int UploadedByMemberId { get; set; } 

        public Member Member { get; set; }

        public bool? IsArchived { get; set; }
        public DateTime? DateArchived { get; set; }




    }
}
