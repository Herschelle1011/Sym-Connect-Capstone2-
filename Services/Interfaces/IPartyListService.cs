using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Election;
using Sym_Connect__Capstone2_.ViewModels.PartyList;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IPartyListService
    {
        Task<List<PartyListsViewModel>> GetAllPartyListsAsync();
        Task<List<PartyListsViewModel>> GetUnassignedPartyListsAsync();
        Task<PartyListDetailsViewModel?> GetPartyListByIdAsync(int id);
        Task AssignPartyListsToElectionAsync(int electionId, List<int> partyListIds);
        Task CreatePartyListAsync(CreatePartyListViewModel vm);
        Task AddCandidateAsync(AddCandidateViewModel vm);

        Task EditPartyListAsync(EditPartyListViewModel vm);
        Task<ArchiveModalViewModel> GetArchiveModalAsync(int partyListId);

        Task<GetPartyListPositionViewModel?> GetPositionsForPartyListAsync(int partyListId);
         Task ArchivePartyListAsync(int id);
        Task<BulkAddCandidatesResult> AddOrUpdateCandidatesAsync(BulkAddCandidatesViewModel model);

        Task<List<CandidateMemberSearchViewModel>> SearchActiveMembersAsync(string term, int partyListId);

        Task<OperationalResult> RestorePartyListAsync(int id);
        Task<OperationalResult> HardDeletePartyListAsync(int id);

        Task<ArchivedPartyListDetailsViewModel?> GetArchivedDetailsAsync(int id);

    }
}
