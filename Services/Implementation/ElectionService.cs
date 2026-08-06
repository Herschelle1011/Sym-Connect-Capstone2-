using AspNetCoreGeneratedDocument;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.ElectionModels;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Election;
using Sym_Connect__Capstone2_.ViewModels.PartyList;


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
                                ElectionId = e.ElectionId,
                                ElectionName = e.ElectionName,
                                StartDate = e.StartDate,
                                EndDate = e.EndDate,
                                StartTime = e.StartTime,
                                EndTime = e.EndTime,
                                candidateCount = e.partyLists.SelectMany(p => p.candidates).Count(),
                                VoteCount = _context.Votes.Count(v => v.ElectionId == e.ElectionId),
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
                                            PositionId = c.positionId,
                                            PositionName = "",
                                            FullName = c.Member.FullName,
                                        }).ToList()
                                    }).ToList()
                                }).FirstOrDefaultAsync();

            if (electionData == null) return null;
            foreach (var party in electionData.partyLists)
            {
                foreach (var candidate in party.candidates)
                {
                    candidate.PositionName = candidate.PositionName;
                }
            }


            var now = DateTime.Now;
            var election = await _context.Elections.AsNoTracking().FirstOrDefaultAsync(e => e.ElectionId == id);
            electionData.Status = now < election.StartDate ? "Incoming" : (now <= election.EndDate ? "Open" : "Closed");

            electionData.TotalCandidates = electionData.partyLists.Sum(p => p.candidates.Count);

            electionData.TotalPositions = electionData.partyLists
                .SelectMany(p => p.candidates)
                .Select(c => c.PositionId)
                .Distinct()
                .Count();
            electionData.TotalVotes = await _context.Votes.CountAsync(v => v.ElectionId == id);

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
                        positionId = slot.PositionId,
                        MemberId = slot.MemberId.Value
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return election.ElectionId;
        }

        public async Task<ResultViewModel> GetElectionResultAsync(int electionId)
        {
            var existingelection = await _context.Elections.FirstOrDefaultAsync(e => e.ElectionId == electionId);
            if (existingelection == null) return null;
            var votes = await _context.Votes.Where(e => e.ElectionId == electionId)
                            .Include(v => v.Candidate)
                                .ThenInclude(m => m.Member)
                                .Include(c => c.Candidate)
                                    .ThenInclude(p => p.PartyList).ToListAsync();

            var distinctVoters = votes.Select(v => v.UserId).Distinct().Count();
            var eligibleVoters = await _context.Members.CountAsync(m => m.Status == "Active");
            var positionNames = await _context.positions
                   .ToDictionaryAsync(p => p.PositionId, p => p.PositionName);



            var positions = votes.GroupBy(v => v.positionId)
                                     .OrderBy(g => (int)g.Key)
                                .Select(g => new PositionResultViewModel
                                {
                                    positionId = g.Key,
                                    PositionName = positionNames.TryGetValue((int)g.Key, out var name) ? name : "Unknown Position",
                                    Candidates = g.GroupBy(v => v.CandidateId)
                                    .Select(cg => new CandidateResultViewModel
                                    {
                                        CandidateId = cg.Key,
                                        Name = cg.First().Candidate.Member.FullName,      
                                        PhotoUrl = cg.First().Candidate.Member.ProfilePictureUrl,
                                        PartyName = cg.First().Candidate.PartyList.PartyName, 
                                        Votes = cg.Count()
                                    })
                                    .OrderByDescending(c => c.Votes)
                                    .ToList()
                                })
                                .ToList();

            foreach (var pos in positions)
            {
                var total = pos.Candidates.Sum(c => c.Votes);
                for (int i = 0; i < pos.Candidates.Count; i++)
                {
                    pos.Candidates[i].Rank = i + 1;
                    pos.Candidates[i].Percentage = total == 0 ? 0 : (double)pos.Candidates[i].Votes / total * 100;
                }
            }

            return new ResultViewModel
            {
                ElectionId = existingelection.ElectionId,
                ElectionTitle = existingelection.ElectionName,
                StartDate = existingelection.StartDate,
                EndDate = existingelection.EndDate,
                Status = existingelection.Status.ToString(),     
                TotalVotes = distinctVoters,
                TotalCandidates = votes.Select(v => v.CandidateId).Distinct().Count(),
                ParticipationPercentage = eligibleVoters == 0 ? 0 : (double)distinctVoters / eligibleVoters * 100,
                Positions = positions,
                LastUpdated = DateTime.Now,
                IsFinalized = existingelection.Status == "Closed"
            };
        }


        public async Task<ArchiveElectionViewModel?> GetArchiveElectionAsync(int? electionid)
        {
            var existingElection = await _context.Elections.FirstOrDefaultAsync(e => e.ElectionId == electionid);
            if (existingElection == null) return null;
            var votes = await _context.Votes.Where(v => v.ElectionId == electionid).ToListAsync();

            var candidateIds = votes.Select(v => v.CandidateId).Distinct().ToList();
            var positions = votes.Select(v => v.Position?.ToString()).Distinct().ToList();

            return new ArchiveElectionViewModel
            {
                electionId = existingElection.ElectionId,
                electionTitle = existingElection.ElectionName,
                electionStatus = existingElection.Status.ToString(),
                TotalVotes = votes.Count,
                TotalCandidates = candidateIds.Count,
                PositionsContested = positions
            };

        }

        public async Task<bool> ArchiveElectionAsync(int? id)
        {
            if (id == null) return false;
            var existingElection = await _context.Elections.FindAsync(id);
            if (existingElection == null) return false;

            var partylist = await _context.PartyLists.Where(p => p.ElectionId == id).ToListAsync();

            foreach(var remove in partylist)
            {
                remove.ElectionId = null; 
            }

            await _context.SaveChangesAsync();

           
            existingElection.IsArchived = true;
            existingElection.ArchivedDate = DateTime.Now;
         
            await _context.SaveChangesAsync();
            return true;

        }
    }

    
}
