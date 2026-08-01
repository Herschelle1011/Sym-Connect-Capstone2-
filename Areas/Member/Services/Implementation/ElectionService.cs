using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.ElectionModels;
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Implementation
{
    public class ElectionService : IElectionService
    {
        private readonly AppDbContext _context;
        public ElectionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindMemberForElectionAsync(string firstname, string email)
        {
            return await _context.Users.FirstOrDefaultAsync
                                (m => m.Email!.ToLower() == email.ToLower()
                                && m.FirstName!.ToLower() == firstname.ToLower());
        }


        public async Task<List<GetAllElectionListViewModel>> getAllElectionList()
        {
            var now = DateTime.Now;

           return await _context.Elections
                .AsNoTracking()
                .Where(e => e.IsArchived == false && e.EndDate >= DateTime.Now)
                .Select(e => new GetAllElectionListViewModel
               {
                    electionId = e.ElectionId,
                    ElectionName = e.ElectionName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Status = now < e.StartDate ? "Incoming" : (now <= e.EndDate ? "Open" : "Closed")
                    }).ToListAsync();    
        }

        public async Task<List<PositionBallot>> GetBallot(int electionId)
        {
            var candidates = await _context.Candidates
                .AsNoTracking()
                .Include(c => c.Member)
                .Include(c => c.PartyList)
                .Where(c => c.PartyList.ElectionId == electionId)
                .ToListAsync();

            return candidates
                .GroupBy(c => c.position)
                .Select(g => new PositionBallot
                {
                    Position = g.Key,
                    Candidates = g.Select(c => new CandidateOption
                    {
                        CandidateId = c.CandidateId,
                        MemberName = c.Member.FullName,
                        PartyName = c.PartyList.PartyName,
                        PhotoUrl = string.IsNullOrEmpty(c.Member.ProfilePictureUrl)
                    ? "/images/default-avatar.png"
                    : c.Member.ProfilePictureUrl

                    }).ToList()
                }).ToList();
        }

        public async Task<bool> HasMemberVotedAsync(string memberId, int electionId)
        {
            return await _context.Votes
        .AnyAsync(v => v.UserId == memberId && v.ElectionId == electionId);
        }

        public async Task<VoteResult> CastVote(TakeVoteViewModel vm)
        {
            var member = await _context.Users
              .FirstOrDefaultAsync(u => u.Id == vm.memberId);

            if (member == null)
                return VoteResult.InvalidSelection;

            bool isAlreadyVoted = await _context.Votes.AnyAsync(v =>
                v.ElectionId == vm.electionId
                && v.UserId == vm.memberId);

            if (isAlreadyVoted) return VoteResult.AlreadyVoted;

            var election = await _context.Elections.FindAsync(vm.electionId);
            if(election == null ||  election.Status != "Open") return VoteResult.InvalidSelection;

            if (vm.selectedVotes == null || vm.selectedVotes.Count == 0) return VoteResult.InvalidSelection;

            bool hasDuplicatePositions = vm.selectedVotes
            .GroupBy(v => v.position)
            .Any(g => g.Count() > 1);
                    if (hasDuplicatePositions) return VoteResult.InvalidSelection;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach(var voteSubmission in vm.selectedVotes)
                {
                    bool validCandidate = await _context.Candidates.AnyAsync(c =>
                        c.CandidateId == voteSubmission.CandidateId &&
                        c.position == voteSubmission.position &&
                        c.PartyList.ElectionId == vm.electionId);

                    if (!validCandidate)
                    {
                        await transaction.RollbackAsync();
                        return VoteResult.InvalidSelection;
                    }

                    //ADD NEW VOTE
                    _context.Votes.Add(new Vote
                    {
                        ElectionId = vm.electionId,
                        UserId = vm.memberId,
                        Position = voteSubmission.position,
                        CandidateId = voteSubmission.CandidateId,
                        DateVoted = DateTime.Now
                    });
                }
                // Save all records together as one single block
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return VoteResult.Success;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return VoteResult.InvalidSelection;
            }
        }
        
    }
}
