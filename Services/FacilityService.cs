using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;


namespace Sym_Connect__Capstone2_.Services
{
    public class FacilityService : IFacilityService
    {
        private readonly AppDbContext _context;
        public FacilityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Facility>> GetAllFacilitiesAsync()
        {
            return await _context.Facilities.
                Where(f => f.IsArchived == false)
                .Include(p => p.FacilityPhotos).ToListAsync();
        }

        public async Task<Facility?> GetFacilityAsyncById(int id)
        {
            return await _context.Facilities.
                Include(p => p.FacilityPhotos)
                .FirstOrDefaultAsync(e => e.FacilityId == id && e.IsArchived == false);
        }

        public async Task CreateFacilityAsync(Facility facility)
        {
             _context.Facilities.Add(facility);
             await _context.SaveChangesAsync();
        }

        public async Task UpdateFacilityAsync(Facility facility)
        {
            _context.Facilities.Update(facility);
            await _context.SaveChangesAsync();
        }
        public async Task SetStatusAsync(int id, FacilityStatus status)
        {
            var facility = await _context.Facilities.FirstOrDefaultAsync(e => e.FacilityId == id);
            if (facility == null) return;
            facility.Status = status;
            await _context.SaveChangesAsync();
        }
        public async Task MaintenanceFacilityAsync(int id) => await SetStatusAsync(id, FacilityStatus.UnderMaintenance);

        public async Task ArchiveFacilityAsync(int id)
        {
            var facility = await _context.Facilities.FirstOrDefaultAsync(e => e.FacilityId == id);
            if (facility == null) return;
            facility.IsArchived = true;
            facility.ArchiveDate = DateTime.Now;
            await _context.SaveChangesAsync();

        }

        public async Task <bool> FacilityNameExist(string name, int? excludeId = null)
        {
           return await _context.Facilities.AnyAsync(e => e.FacilityName.ToLower() == name.ToLower() && e.IsArchived == false  && ( excludeId == null|| e.FacilityId != excludeId.Value));             
        }

        public async Task DeletePhotosAsync(List<int> photoIds)
        {
            // Find all photos in DB that match the given IDs
            var photos = await _context.FacilityPhotos
                .Where(p => photoIds.Contains(p.FacilityPhotoId))
                .ToListAsync();

            _context.FacilityPhotos.RemoveRange(photos);
            await _context.SaveChangesAsync();
        }
    }
}
