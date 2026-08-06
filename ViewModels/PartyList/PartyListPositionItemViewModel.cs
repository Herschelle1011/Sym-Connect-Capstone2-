namespace Sym_Connect__Capstone2_.ViewModels.PartyList
{
    public class PartyListPositionItemViewModel
    {
        public int positionId { get; set; }
        public string positionName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        public int? candidateId { get; set; }
        public string? candidateName { get; set; }
        public string? candidatePhotoUrl { get; set; }

        public int maxCandidates { get; set; }
        public List<CandidateSlotDisplayViewModel> Slots { get; set; } = new();
        public bool hasCandidate => candidateId.HasValue;
    }
}
