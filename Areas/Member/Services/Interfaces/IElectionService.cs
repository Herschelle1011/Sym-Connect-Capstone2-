using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Election;
using MemberModel = Sym_Connect__Capstone2_.Models.Member;
using Sym_Connect__Capstone2_.Models.Enums;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces
{
    public interface IElectionService
    {
        Task<MemberModel?> FindMemberForElectionAsync(string firstname, string email);
        Task<List<GetAllElectionListViewModel>> getAllElectionList();
        
        Task<VoteResult> CastVote(TakeVoteViewModel vm);

        Task<List<PositionBallot>> GetBallot(int electionId);

        Task<bool> HasMemberVotedAsync(string memberId, int electionId);
    }
}
