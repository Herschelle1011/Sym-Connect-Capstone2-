using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.PartyList.Positions;
namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class PositionService : IPositionService
    {
        private readonly AppDbContext _context;
        
        public PositionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<GetPositionListViewModel>> GetPositionListViewModels()
        {
            return await _context.positions
                .Select(p => new GetPositionListViewModel
                {
                    PositionId = p.PositionId,
                    PositionName = p.PositionName,
                    Description = p.Description,
                    DisplayOrder = p.DisplayOrder,
                    maxCandidate = p.MaxCandidates,
                    IsActive = p.IsActive
                }).ToListAsync();
        }

        public async Task<bool> CreateAsync(CreatePositionViewModel vm)
        {
            var position = new Position
            {
                PositionName = vm.PositionName,
                Description = vm.Description,
                DisplayOrder = vm.DisplayOrder,
                MaxCandidates = vm.MaxCandidate,
                IsActive = true,
            };
             await _context.positions.AddAsync(position);
            await _context.SaveChangesAsync();
            return true;

        }

        //GET
        public async Task<EditPositionViewModel?> GetEditedPositionAsync(int? id)
        {
            if (id == null) return null;
            return await _context.positions
                 .Where(p => p.PositionId == id)
                 .Select(P => new EditPositionViewModel
                 {
                     positionId = P.PositionId,
                     PositionName = P.PositionName,
                     Description = P.Description,
                     DisplayOrder = P.DisplayOrder,
                     MaxCandidate = P.MaxCandidates,
                     IsActive = P.IsActive
                 }).FirstOrDefaultAsync();
        }
        //POST
        public async Task<bool> UpdatePositionAsync(EditPositionViewModel vm)
        {
            var ExistingPosition = await _context.positions.FirstOrDefaultAsync(p => p.PositionId == vm.positionId);
            if (ExistingPosition == null) return false;

            ExistingPosition.PositionName = vm.PositionName;
            ExistingPosition.Description = vm.Description;
            ExistingPosition.DisplayOrder = vm.DisplayOrder;
            ExistingPosition.MaxCandidates = vm.MaxCandidate;
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> SetActiveStatusAsync(int positionId, bool isActive)
        {
            var position = await _context.positions.FindAsync(positionId);
            if (position == null) return false;
            position.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }




    }
}
