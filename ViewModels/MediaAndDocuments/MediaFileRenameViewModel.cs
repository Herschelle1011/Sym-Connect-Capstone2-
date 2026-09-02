using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class MediaFileRenameViewModel
    {
        public int MediaFileId { get; set; }

        [Required]

        public string FileName { get; set; } = string.Empty;
    }
}
