using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.FacilityModels;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Facility;


namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IFacilityService
    {
        //to be updated VIEWMODELS
        Task<List<FacilityListViewModel>> GetAllFacilitiesAsync();
         Task<List<FacilityListViewModel>> GetAllFacilitiesForEventsAsync();
        Task<FacilityDetailsViewModel?> GetFacilityAsyncById(int id);
        Task<OperationalResult> CreateFacilityAsync(CreateFacilityViewModel vm);
        Task<OperationalResult> UpdateFacilityAsync(EditFacilityViewModel vm);
        Task<OperationalResult> SetStatusAsync(int id, FacilityStatus status);
        Task<OperationalResult> ArchiveFacilityAsync(int id);
        Task MaintenanceFacilityAsync(int id);
        Task<bool> FacilityNameExist(string name, int? excludeId = null);
        Task<OperationalResult> DeletePhotosAsync(List<int> photoIds);

    }
}
