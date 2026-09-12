namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class LeaderGroupMemberViewModel
    {
        public int RowNumber { get; set; }
        public int MemberId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? MiddleName { get; set; }
        public string Status { get; set; }
        public int EarnedPoints { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string PhotoUrl { get; set; }
        public DateTime DateRegistered { get; set; }

        public string DisplayName =>
            MiddleName is { Length: > 0 }
                ? $"{LastName}, {FirstName} {MiddleName[0]}."
                : $"{LastName}, {FirstName}";
    }
}
