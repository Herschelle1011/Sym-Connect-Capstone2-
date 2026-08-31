using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class MediaFolderCreateViewModel
    {
        [Required(ErrorMessage = "Folder name is required")]
        public string FolderName { get; set; }

        public bool IsPrivate { get; set; }
    }
}
