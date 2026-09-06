using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments
{
    public class RejectFileViewModel
    {
        public int MediaFileId { get; set; }

        [Required(ErrorMessage = "A rejection reason is required")]
        public string Reason { get; set; }
    }
}
