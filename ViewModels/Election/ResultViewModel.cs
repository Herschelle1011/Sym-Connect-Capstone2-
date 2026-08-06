using Sym_Connect__Capstone2_.Models.Enums;
using System;
using System.Collections.Generic;

namespace Sym_Connect__Capstone2_.ViewModels.Election
{
    public class ResultViewModel
    {
        // --- Election header ---
        public int ElectionId { get; set; }
        public string ElectionTitle { get; set; }
        public string Status { get; set; }              // or ElectionStatus enum if you have one
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // --- Stat cards ---
        public int TotalVotes { get; set; }
        public int TotalCandidates { get; set; }
        public double ParticipationPercentage { get; set; }

        // --- Results grouped by position ---
        public List<PositionResultViewModel> Positions { get; set; } = new();

        // --- Footer meta ---
        public DateTime LastUpdated { get; set; }
        public bool IsFinalized { get; set; }
    }

    public class PositionResultViewModel
    {
        public int positionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public List<CandidateResultViewModel> Candidates { get; set; } = new();
    }

    public class CandidateResultViewModel
    {
        public int CandidateId { get; set; }
        public int MemberId { get; set; }
        public string Name { get; set; }                 // from Member
        public string PhotoUrl { get; set; }              // from Member (Cloudinary)
        public string PartyName { get; set; }             // from PartyList
        public int Rank { get; set; }                     // computed per-position
        public int Votes { get; set; }
        public double Percentage { get; set; }            // computed per-position
    }
}