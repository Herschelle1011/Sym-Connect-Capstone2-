using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Archive
{
    public class ArchiveConfirmationViewModel
    {
        public int Id { get; set; }
        public ArchiveCategory Category { get; set; }
        public string ItemName { get; set; } = string.Empty;
    }
}
