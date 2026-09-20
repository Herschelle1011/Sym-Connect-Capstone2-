using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.Archive
{
    public class ArchiveItemViewModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public ArchiveCategory Category { get; set; }
        public DateTime? ArchiveDate { get; set; }

        public bool CanRestore { get; set; } = true;
        public DateTime? RelevantScheduledDate { get; set; }
    }
}
