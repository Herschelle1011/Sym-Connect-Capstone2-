using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sym_Connect__Capstone2_.Models.ElectionModels
{
    public class Vote
    {

        [Key]
        public int VoteId { get; set; }

        public string UserId { get; set; }      // who is voting
        public int ElectionId { get; set; }
        public Positions Position { get; set; } // President, VP, etc.
        public int CandidateId { get; set; }    // who they voted for

        public DateTime DateVoted { get; set; } = DateTime.Now;

        [ForeignKey(nameof(UserId))]

        public User? User { get; set; }

        [ForeignKey(nameof(ElectionId))]
        public Election? Election { get; set; }

        [ForeignKey(nameof(CandidateId))]
        public Candidates? Candidate { get; set; }
    }
}
