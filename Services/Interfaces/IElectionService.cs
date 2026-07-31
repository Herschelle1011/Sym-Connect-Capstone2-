using Sym_Connect__Capstone2_.Models.ElectionModels;
using Sym_Connect__Capstone2_.ViewModels.Election;


namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IElectionService
    {
        Task <List<ElectionListViewModel>> GetAllElectionListAsync();
        Task<ElectionDetailsViewModel?> GetElectionByIdAsync(int? id);
        Task<int> CreateElectionAsync(CreateElectionViewModel vm);
       Task<int> CreateElectionWithPartyListsAsync(CreateElectionWizardViewModel vm);
        Task EditElectionAsync(EditElectionViewModel vm);

        Task<EditElectionViewModel?> GetEditElectionByIdAsync(int id);
        Task ArchiveElectionAsync(int? id);

    }
}
