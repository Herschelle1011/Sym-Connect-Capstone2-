using Sym_Connect__Capstone2_.Models;

namespace Sym_Connect__Capstone2_.Services
{
    public interface IFacilityService
    {
        Task<List<Facility>> GetAllFacilitiesAsync();
        Task<Facility?> GetFacilityAsyncById(int id);
        Task CreateFacilityAsync(Facility facility);
        Task UpdateFacilityAsync(Facility facility);
        Task SetStatusAsync(int id, Facility.FacilityStatus Status);
        Task ArchiveFacilityAsync(int id);
        Task MaintenanceFacilityAsync(int id);
        Task<bool> FacilityNameExist(string name, int? excludeId = null);
        Task DeletePhotosAsync(List<int> PhotoIds);

    }
}
