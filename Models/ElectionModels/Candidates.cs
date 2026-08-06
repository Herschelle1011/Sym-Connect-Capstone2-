using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.SignalR;
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Models.ElectionModels
{
    public class Candidates
    {
        [Key]
        public int CandidateId { get; set; }

        public int positionId { get; set; }

        public int MemberId { get; set; }
        public int PartyId { get; set; }


        //Navigation Properties
        [ForeignKey(nameof(MemberId))]
        public Member Member { get; set; }

        [ForeignKey(nameof(PartyId))]
        public PartyList PartyList { get; set; }

        [ForeignKey(nameof(positionId))]
        public Position? Position { get; set; }


    }
}
