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
            ExistingPosition.DisplayOrder = vm.DisplayOrder;
            ExistingPosition.MaxCandidates = vm.MaxCandidate;
            await _context.SaveChangesAsync();
            return true;

        }

  
        public async Task<bool> SetActiveStatusAsync(int positionId, bool isActive)
        {
            var position = await _context.positions.FindAsync(positionId);
            if (position == null) return false;

            if (!isActive)
            {
                var now = DateTime.Now;

                var candidates = await _context.Candidates
                    .Include(c => c.PartyList)
                        .ThenInclude(p => p.Election)
                    .Where(c => c.positionId == positionId)
                    .ToListAsync();

                // Only candidates whose election has NOT started yet
                // (or that are not in any election) are safe to remove.
                var removable = candidates
                    .Where(c =>
                        c.PartyList == null ||
                        c.PartyList.Election == null ||
                        now < c.PartyList.Election.StartDate.Date + c.PartyList.Election.StartTime)
                    .ToList();

                // Never remove a candidate who already has votes.
                var removableIds = removable.Select(c => c.CandidateId).ToList();

                var votedIds = await _context.Votes
                    .Where(v => removableIds.Contains(v.CandidateId))
                    .Select(v => v.CandidateId)
                    .Distinct()
                    .ToListAsync();

                var toRemove = removable
                    .Where(c => !votedIds.Contains(c.CandidateId))
                    .ToList();

                _context.Candidates.RemoveRange(toRemove);
            }

            position.IsActive = isActive;

            // One SaveChanges, so the candidate removal and the status change
            // succeed or fail together.
            await _context.SaveChangesAsync();
            return true;
        }




    }
}