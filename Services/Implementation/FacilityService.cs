using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.FacilityModels;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Facility;


namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class FacilityService : IFacilityService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public FacilityService(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<List<FacilityListViewModel>> GetAllFacilitiesAsync()
        {
            return await _context.Facilities
                .AsNoTracking().
                Where(f => f.IsArchived == false)
                .Select(f => new FacilityListViewModel
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    FacilityDescription = f.FacilityDescription,
                    FacilityTypes = f.FacilityTypes,
                    Status = f.Status,
                    Capacity = f.Capacity,
                    Photos = f.FacilityPhotos.Select(p => new ExistingPhotoViewModel
                    {
                        PhotoId = p.FacilityPhotoId,
                        PhotoUrl = p.PhotoUrl
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<List<FacilityListViewModel>> GetAllFacilitiesForEventsAsync()
        {
            return await _context.Facilities
                .AsNoTracking().
                Where(f => f.IsArchived == false && f.Status == FacilityStatus.Active)
                .Select(f => new FacilityListViewModel
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    FacilityDescription = f.FacilityDescription,
                    FacilityTypes = f.FacilityTypes,
                    Status = f.Status,
                    Capacity = f.Capacity,
                    Photos = f.FacilityPhotos.Select(p => new ExistingPhotoViewModel
                    {
                        PhotoId = p.FacilityPhotoId,
                        PhotoUrl = p.PhotoUrl
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<FacilityDetailsViewModel?> GetFacilityAsyncById(int id)
        {
            var existingFacility = await _context.Facilities
                .AsNoTracking()
                .Where(f => f.FacilityId == id && f.IsArchived == false)
                .Select(f => new FacilityDetailsViewModel
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    FacilityDescription = f.FacilityDescription,
                    Status = f.Status,
                    Capacity = f.Capacity,
                    Photos = f.FacilityPhotos.Select(p => new ExistingPhotoViewModel
                    {
                        PhotoId = p.FacilityPhotoId,
                        PhotoUrl = p.PhotoUrl
                    }).ToList()
                }).FirstOrDefaultAsync();

            return existingFacility;
        }

        public async Task<OperationalResult> CreateFacilityAsync(CreateFacilityViewModel vm)
        {
            if (await FacilityNameExist(vm.FacilityName))
            {
                return new OperationalResult { Success = false, Message = "Facility name already exists." };
            }

            var newFacility = new Facility
            {
                FacilityName = vm.FacilityName,
                FacilityDescription = vm.FacilityDescription,
                Capacity = vm.Capacity,
                Status = vm.Status,
                FacilityTypes = vm.FacilityTypes,
                IsArchived = false
            };

            if (vm.Photos != null && vm.Photos.Count > 0)
            {
                foreach (var photoFile in vm.Photos)
                {
                    var photoUrl = await _cloudinaryService.UploadFacilityPhotoAsync(photoFile, vm.FacilityName);

                    if (photoUrl != null)
                    {
                        newFacility.FacilityPhotos.Add(new FacilityPhoto
                        {
                            PhotoUrl = photoUrl
                        });
                    }
                }
            }

            _context.Facilities.Add(newFacility);
            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = $"{newFacility.FacilityName} created" };
        }


        public async Task<OperationalResult> UpdateFacilityAsync(EditFacilityViewModel vm)
        {
            if (await FacilityNameExist(vm.FacilityName, vm.FacilityId))
            {
                return new OperationalResult { Success = false, Message = "Facility name already exists." };
            }

            var facility = await _context.Facilities.FirstOrDefaultAsync(f => f.FacilityId == vm.FacilityId);
            if (facility == null)
            {
                return new OperationalResult { Success = false, Message = "Facility not found." };
            }

            facility.FacilityName = vm.FacilityName;
            facility.FacilityDescription = vm.FacilityDescription;
            facility.FacilityTypes = vm.FacilityTypes;
            facility.Capacity = vm.Capacity;
            facility.Status = vm.Status;

            if (vm.RemovedPhotoIds != null && vm.RemovedPhotoIds.Count > 0)
            {
                await DeletePhotosAsync(vm.RemovedPhotoIds);
            }

            if (vm.Photos != null && vm.Photos.Count > 0)
            {
                facility.FacilityPhotos ??= new List<FacilityPhoto>();

                foreach (var photo in vm.Photos)
                {
                    var photoUrl = await _cloudinaryService.UploadFacilityPhotoAsync(photo, vm.FacilityName);

                    if (photoUrl != null)
                    {
                        facility.FacilityPhotos.Add(new FacilityPhoto
                        {
                            PhotoUrl = photoUrl,
                            FacilityId = facility.FacilityId
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = $"{facility.FacilityName} updated" };
        }


        public async Task<OperationalResult> SetStatusAsync(int id, FacilityStatus status)
        {
            var facility = await _context.Facilities.FirstOrDefaultAsync(e => e.FacilityId == id);
            if (facility == null) return new OperationalResult { Success = false, Message = "Facility not found." };
           
            facility.Status = status;
            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = "Facility status updated." };

        }
        public async Task MaintenanceFacilityAsync(int id) => await SetStatusAsync(id, FacilityStatus.UnderMaintenance);

        public async Task<OperationalResult> ArchiveFacilityAsync(int id)
        {
            var facility = await _context.Facilities.FirstOrDefaultAsync(e => e.FacilityId == id);
            if (facility == null) return new OperationalResult { Success = false, Message = "Facility not found." };
            facility.IsArchived = true;
            facility.ArchiveDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = "Facility successfully saved to archived." };
        }

        public async Task <bool> FacilityNameExist(string name, int? excludeId = null)
        {
           return await _context.Facilities.AnyAsync(e => e.FacilityName.ToLower() == name.ToLower() && e.IsArchived == false  && ( excludeId == null|| e.FacilityId != excludeId.Value));             
        }

        public async Task<OperationalResult> DeletePhotosAsync(List<int> photoIds)
        {
            // Find all photos in DB that match the given IDs
            var photos = await _context.FacilityPhotos
                .Where(p => photoIds.Contains(p.FacilityPhotoId))
                .ToListAsync();

            _context.FacilityPhotos.RemoveRange(photos);
            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = "Facility photo removed." };

        }
    }
}
