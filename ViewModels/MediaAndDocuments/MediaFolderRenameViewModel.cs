using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class MediaFolderRenameViewModel
    {
        public int MediaFolderId { get; set; }

        [Required]

        public string FolderName { get; set; } = string.Empty;
    }
}
