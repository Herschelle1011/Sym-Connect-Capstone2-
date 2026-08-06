using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.ViewModels.PartyList.Positions;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IPositionService
    {
        Task<List<GetPositionListViewModel>> GetPositionListViewModels();
        Task<bool> CreateAsync(CreatePositionViewModel vm);

        Task<bool> UpdatePositionAsync(EditPositionViewModel vm);
        Task<EditPositionViewModel?> GetEditedPositionAsync(int? id);
         Task<bool> SetActiveStatusAsync(int positionId, bool isActive);
    }
}
