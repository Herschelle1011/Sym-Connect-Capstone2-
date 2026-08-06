using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Election
{
    public class PositionBallot
    {
       public int PositionId { get; set; }
        public string positionName { get; set; } = string.Empty;
        public List<CandidateTicketViewModel> Tickets { get; set; } = new();
    }
}
