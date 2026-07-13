using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;


namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IFacilityService
    {
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
