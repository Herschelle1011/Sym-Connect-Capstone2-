using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.ElectionModels;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Election;
using Sym_Connect__Capstone2_.ViewModels.Election.PartyList;


namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class ElectionService : IElectionService
    {
        private readonly AppDbContext _context;

        public ElectionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ElectionListViewModel>> GetAllElectionListAsync()
        {
            var now = DateTime.Now;

            return await _context.Elections
                    .AsNoTracking()
                         .Where(e => e.IsArchived == false)
                            .Select(e => new ElectionListViewModel
                            {
                                ElectionName = e.ElectionName,
                                StartDate = e.StartDate,
                                EndDate = e.EndDate,
                                StartTime = e.StartTime,
                                EndTime = e.EndTime,
                                Status = now < e.StartDate ? "Incoming" : (now <= e.EndDate ? "Open" : "Closed")
                            }).ToListAsync();                           
        }

        public async Task<ElectionDetailsViewModel?> GetElectionByIdAsync(int? id)
        {
            if (id == null) return null; 

            var electionData = await _context.Elections
                                .AsNoTracking().Where(e => e.ElectionId == id && e.IsArchived == false)
                                .Select(e => new ElectionDetailsViewModel
                                {
                                    id = e.ElectionId,
                                    ElectionName = e.ElectionName,
                                    StartDate = e.StartDate,
                                    EndDate = e.EndDate,
                                    StartTime = e.StartTime,
                                    EndTime = e.EndTime,
                                    partyLists = e.partyLists.Select(p => new PartyListsViewModel
                                    {
                                        Id = p.PartyListId,
                                        PartyListName = p.PartyName,
                                        PartyListDescription = p.PartyListDescription,

                                        candidates = p.candidates.Select(c => new CandidateViewModel
                                        {
                                            CandidateId = c.CandidateId,
                                            Positions = c.position,
                                            PositionName = "",
                                            FullName = c.Member.FullName,
                                        }).ToList()
                                    }).ToList()
                                }).FirstOrDefaultAsync();

            if (electionData == null) return null;
            foreach(var party in electionData.partyLists)
            {
                foreach(var candidate in party.candidates)
                {
                    candidate.PositionName = candidate.Positions.ToString();
                }
            }
            return electionData;                   
        }

        public async Task<int> CreateElectionAsync(CreateElectionViewModel vm)
        {
            var NewElection = new Election
            {
                ElectionName = vm.ElectionName,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                IsArchived = false,
            };
            _context.Elections.Add(NewElection);
            await _context.SaveChangesAsync();
            return NewElection.ElectionId;

        }

        public async Task<EditElectionViewModel?> GetEditElectionByIdAsync(int id)
        {
            return await _context.Elections
                            .AsNoTracking()
                            .Where(e => e.ElectionId == id)
                            .Select(e => new EditElectionViewModel
                            {
                                ElectionId = id,
                                ElectionName = e.ElectionName,
                                StartDate = e.StartDate,
                                EndDate = e.EndDate,
                                StartTime = e.StartTime,
                                EndTime = e.EndTime,
                            }).FirstOrDefaultAsync();
        }

        public async Task EditElectionAsync(EditElectionViewModel vm)
        {
            var existingElectionId = await _context.Elections
                                    .Include(e => e.partyLists)
                                      .FirstOrDefaultAsync(e => e.ElectionId == vm.ElectionId);

            if (existingElectionId == null) return;

            existingElectionId.ElectionName = vm.ElectionName;
            existingElectionId.StartDate = vm.StartDate;
            existingElectionId.EndDate = vm.EndDate;
            existingElectionId.StartTime = vm.StartTime;
            existingElectionId.EndTime = vm.EndTime;

            await _context.SaveChangesAsync();
        }


        public async Task ArchiveElectionAsync(int? id)
        {
            var election = await _context.Elections.FirstOrDefaultAsync(e => e.ElectionId == id);
            if (election == null) return;
            election.IsArchived = true;
            election.ArchivedDate = DateTime.Now;

            await _context.SaveChangesAsync();
        }


        public async Task<int> CreateElectionWithPartyListsAsync(CreateElectionWizardViewModel vm)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var election = new Election
            {
                ElectionName = vm.ElectionName,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                IsArchived = false,
            };
            _context.Elections.Add(election);
            await _context.SaveChangesAsync();

            var partyLists = await _context.PartyLists
                .Where(p => vm.SelectedPartyListIds.Contains(p.PartyListId) && p.ElectionId == null)
                .ToListAsync();

            if (partyLists.Count != 2)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Exactly 2 unassigned party lists must be selected.");
            }

            foreach (var party in partyLists)
            {
                party.ElectionId = election.ElectionId;
            }

            foreach (var assignment in vm.PartyListAssignments)
            {
                foreach (var slot in assignment.Candidates)
                {
                    if (slot.CandidateId == null) continue;

                    _context.Candidates.Add(new Candidates
                    {
                        PartyId = assignment.PartyListId,
                        position = slot.Positions,
                        MemberId = slot.MemberId.Value
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return election.ElectionId;
        }
    }

    
}
