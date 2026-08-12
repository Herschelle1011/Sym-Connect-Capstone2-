using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.FacilityModels;
using Sym_Connect__Capstone2_.ViewModels.Facility;


namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IFacilityService
    {
        //to be updated VIEWMODELS
        Task<List<Facility>> GetAllFacilitiesAsync();
        Task<Facility?> GetFacilityAsyncById(int id);
        Task CreateFacilityAsync(Facility facility);
        Task UpdateFacilityAsync(Facility facility);
        Task SetStatusAsync(int id, FacilityStatus Status);
        Task ArchiveFacilityAsync(int id);
        Task MaintenanceFacilityAsync(int id);
        Task<bool> FacilityNameExist(string name, int? excludeId = null);
        Task DeletePhotosAsync(List<int> PhotoIds);

    }
}
