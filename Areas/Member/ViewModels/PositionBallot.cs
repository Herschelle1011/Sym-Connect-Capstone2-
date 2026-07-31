using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels
{
    public class PositionBallot
    {
       public Positions Position { get; set; }
        public List<CandidateOption> Candidates { get; set; } = new();
    }
}
