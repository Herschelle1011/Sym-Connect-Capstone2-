namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class ArchivedGroupDetailsViewModel
    {
        public int GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public string? Description { get; set; }


        public int MemberCount { get; set; }

        public DateTime? ArchiveDate { get; set; }
    }
}
