using Sym_Connect__Capstone2_.Models.ElectionModels;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Election;


namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IElectionService
    {
        Task <List<ElectionListViewModel>> GetAllElectionListAsync();
        Task<ElectionDetailsViewModel?> GetElectionByIdAsync(int? id);
        Task<int> CreateElectionAsync(CreateElectionViewModel vm);
       Task<int> CreateElectionWithPartyListsAsync(CreateElectionWizardViewModel vm);
        Task<OperationalResult> EditElectionAsync(EditElectionViewModel vm);

        Task<EditElectionViewModel?> GetEditElectionByIdAsync(int id);
        Task<ArchiveElectionViewModel?> GetArchiveElectionAsync(int? electionid);

        Task<bool> ArchiveElectionAsync(int? id);

        Task<ResultViewModel> GetElectionResultAsync(int electionId);

        Task<OperationalResult> RestoreElectionAsync(int id);
        Task<OperationalResult> HardDeleteElectionAsync(int id);

        Task<int?> GetOpenElectionMemberHasNotVotedAsync(string userId);

        Task<ArchivedElectionDetailsViewModel?> GetArchivedDetailsAsync(int id);

    }
}
