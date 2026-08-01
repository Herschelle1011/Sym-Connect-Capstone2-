namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class CandidateMemberSearchViewModel
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("photoUrl")]
        public string? PhotoUrl { get; set; }
    }
}
