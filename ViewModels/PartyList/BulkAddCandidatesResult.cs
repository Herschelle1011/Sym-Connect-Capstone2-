namespace Sym_Connect__Capstone2_.ViewModels.PartyList
{
    public class BulkAddCandidatesResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
