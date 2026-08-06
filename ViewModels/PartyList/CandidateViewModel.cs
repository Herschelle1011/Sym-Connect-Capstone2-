using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.ViewModels.PartyList
{
    public class CandidateViewModel
    {
        public int CandidateId { get; set; }

        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        
        public string FullName { get; set; } = string.Empty;
        public int? MemberId { get; set; }

        public string PhotoUrl { get; set; } = string.Empty;

    }
}
