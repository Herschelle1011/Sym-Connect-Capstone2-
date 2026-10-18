using AspNetCoreGeneratedDocument;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.ElectionModels;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
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

            var elections = await _context.Elections
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

                    candidateCount = e.partyLists
                        .SelectMany(p => p.candidates)
                        .Count(),

                    VoteCount = _context.Votes
                        .Count(v => v.ElectionId == e.ElectionId),

                    Status = ""
                })
                .ToListAsync();


            foreach (var election in elections)
            {
                var startDateTime =
                    election.StartDate.Date +
                    election.StartTime;

                var endDateTime =
                    election.EndDate.Date +
                    election.EndTime;


                if (now < startDateTime)
                {
                    election.Status = "Incoming";
                }
                else if (now <= endDateTime)
                {
                    election.Status = "Open";
                }
                else
                {
                    election.Status = "Closed";
                }
            }


            return elections;
        }

        public async Task<ElectionDetailsViewModel?> GetElectionByIdAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }


            var electionData = await _context.Elections
                .AsNoTracking()
                .Where(e =>
                    e.ElectionId == id &&
                    e.IsArchived == false)
                .Select(e => new ElectionDetailsViewModel
                {
                    id = e.ElectionId,

                    ElectionName = e.ElectionName,

                    StartDate = e.StartDate,
                    EndDate = e.EndDate,

                    StartTime = e.StartTime,
                    EndTime = e.EndTime,

                    partyLists = e.partyLists
                        .Select(p => new PartyListsViewModel
                        {
                            Id = p.PartyListId,

                            PartyListName =
                                p.PartyName,

                            PartyListDescription =
                                p.PartyListDescription,

                            candidates = p.candidates
                                .Select(c => new CandidateViewModel
                                {
                                    CandidateId =
                                        c.CandidateId,

                                    PositionId =
                                        c.positionId,

                                    PositionName =
                                        c.Position.PositionName,

                                    FullName =
                                        c.Member.FullName
                                })
                                .ToList()

                        })
                        .ToList()

                })
                .FirstOrDefaultAsync();


            if (electionData == null)
            {
                return null;
            }


            // =========================================
            // STATUS
            // =========================================

            var now = DateTime.Now;

            var startDateTime =
                electionData.StartDate.Date +
                electionData.StartTime;

            var endDateTime =
                electionData.EndDate.Date +
                electionData.EndTime;


            if (now < startDateTime)
            {
                electionData.Status = "Incoming";
            }
            else if (now <= endDateTime)
            {
                electionData.Status = "Open";
            }
            else
            {
                electionData.Status = "Closed";
            }


            // =========================================
            // COUNTS
            // =========================================

            electionData.TotalCandidates =
                electionData.partyLists
                    .Sum(p => p.candidates.Count);


            electionData.TotalPositions =
                electionData.partyLists
                    .SelectMany(p => p.candidates)
                    .Select(c => c.PositionId)
                    .Distinct()
                    .Count();


            electionData.TotalVotes =
                await _context.Votes
                    .CountAsync(v =>
                        v.ElectionId == id);


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
            var vm = await _context.Elections
                .AsNoTracking()
                .Where(e => e.ElectionId == id)
                .Select(e => new EditElectionViewModel
                {
                    ElectionId = e.ElectionId,
                    ElectionName = e.ElectionName,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,

                    SelectedPartyListIds = e.partyLists
                        .Select(ep => ep.PartyListId)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (vm == null)
                return null;

            vm.AvailablePartyLists = await _context.PartyLists
                .AsNoTracking()
                .Where(p =>
                    p.isArchived != true &&
                    (p.ElectionId == null || p.ElectionId == id))
                .OrderBy(p => p.PartyName)
                .Select(p => new PartyListsViewModel
                {
                    Id = p.PartyListId,
                    PartyListName = p.PartyName,
                    PartyListDescription = p.PartyListDescription
                })
                .ToListAsync();

            return vm;
        }

        public async Task<OperationalResult> EditElectionAsync(
     EditElectionViewModel vm)
        {
            // ==========================================
            // 1. FIND ELECTION
            // ==========================================

            var election = await _context.Elections
                .Include(e => e.partyLists)
                .FirstOrDefaultAsync(e =>
                    e.ElectionId == vm.ElectionId &&
                    e.IsArchived == false);

            if (election == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Election not found."
                };
            }


            // ==========================================
            // 2. DON'T EDIT AFTER ELECTION STARTS
            // ==========================================

            var currentStart =
                election.StartDate.Date +
                election.StartTime;


            if (DateTime.Now >= currentStart)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message =
                        "This election can no longer be edited because voting has already started."
                };
            }


            // ==========================================
            // 3. EXACTLY TWO PARTY LISTS
            // ==========================================

            if (vm.SelectedPartyListIds == null ||
                vm.SelectedPartyListIds.Count != 2)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message =
                        "Please select exactly 2 PartyLists."
                };
            }


            // Prevent duplicate IDs.
            if (vm.SelectedPartyListIds
                .Distinct()
                .Count() != 2)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message =
                        "Please select two different PartyLists."
                };
            }


            // ==========================================
            // 4. CHECK NEW START / END
            // ==========================================

            var newStart = vm.StartDate.Date + vm.StartTime;
            var newEnd = vm.EndDate.Date + vm.EndTime;

            bool startChanged = Math.Abs((newStart - currentStart).TotalMinutes) >= 1;

            if (startChanged && newStart <= DateTime.Now)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Election start date and time must be in the future."
                };
            }

            if (newEnd <= newStart)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Election end date and time must be after the start date and time."
                };
            }


            // ==========================================
            // 5. VALIDATE SELECTED PARTY LISTS
            // ==========================================

            var selectedPartyLists =
                await _context.PartyLists
                    .Where(p =>
                        vm.SelectedPartyListIds
                            .Contains(p.PartyListId) &&

                        p.isArchived != true &&

                        (
                            p.ElectionId == null ||
                            p.ElectionId == election.ElectionId
                        ))
                    .ToListAsync();


            if (selectedPartyLists.Count != 2)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message =
                        "One or more selected PartyLists are unavailable."
                };
            }


            // ==========================================
            // 6. UPDATE ELECTION INFORMATION
            // ==========================================

            election.ElectionName =
                vm.ElectionName.Trim();

            election.StartDate =
                vm.StartDate.Date;

            election.EndDate =
                vm.EndDate.Date;

            election.StartTime =
                vm.StartTime;

            election.EndTime =
                vm.EndTime;


            // ==========================================
            // 7. DISCONNECT REMOVED PARTY LISTS
            // ==========================================

            var partyListsToRemove =
                election.partyLists
                    .Where(p =>
                        !vm.SelectedPartyListIds
                            .Contains(p.PartyListId))
                    .ToList();


            foreach (var party in partyListsToRemove)
            {
                party.ElectionId = null;
            }


            // ==========================================
            // 8. CONNECT NEW PARTY LISTS
            // ==========================================

            foreach (var party in selectedPartyLists)
            {
                party.ElectionId =
                    election.ElectionId;
            }


            // ==========================================
            // 9. SAVE
            // ==========================================

            await _context.SaveChangesAsync();


            return new OperationalResult
            {
                Success = true,
                Message =
                    "Election updated successfully."
            };
        }

        public async Task<int?> GetOpenElectionMemberHasNotVotedAsync(string userId)
        {
            var now = DateTime.Now;

            var elections = await _context.Elections
                .Where(e => e.IsArchived == false)
                .ToListAsync();

            foreach (var election in elections)
            {
                var startDateTime =
                    election.StartDate.Date +
                    election.StartTime;

                var endDateTime =
                    election.EndDate.Date +
                    election.EndTime;

                bool isOpen =
                    now >= startDateTime &&
                    now <= endDateTime;

                if (!isOpen)
                {
                    continue;
                }

                bool alreadyVoted = await _context.Votes
                    .AnyAsync(v =>
                        v.ElectionId == election.ElectionId &&
                        v.UserId == userId);

                if (!alreadyVoted)
                {
                    return election.ElectionId;
                }
            }

            return null;
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


            var now = DateTime.Now;

            var candidatePartyLists = await _context.PartyLists
                .Include(p => p.Election)
                .Where(p => vm.SelectedPartyListIds.Contains(p.PartyListId))
                .ToListAsync();

            var partyLists = candidatePartyLists
                .Where(p =>
                    p.ElectionId == null ||
                    (p.Election != null &&
                     now > p.Election.EndDate.Date + p.Election.EndTime))
                .ToList();

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
                                    .ThenInclude(u => u.User)
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
                                        PhotoUrl = cg.First().Candidate.Member.User.ProfilePictureUrl,
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
            if (id == null)
            {
                return false;
            }

            var existingElection = await _context.Elections
                .Include(e => e.partyLists)
                .FirstOrDefaultAsync(e => e.ElectionId == id);

            if (existingElection == null)
            {
                return false;
            }

            existingElection.IsArchived = true;
            existingElection.ArchivedDate = DateTime.Now;

            foreach (var partyList in existingElection.partyLists)
            {
                partyList.ElectionId = null;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<OperationalResult> RestoreElectionAsync(int id)
        {
            var existingElection = await _context.Elections.FirstOrDefaultAsync(e => e.ElectionId == id);
            if (existingElection == null)
                return new OperationalResult { Success = false, Message = "Election not found." };

            existingElection.IsArchived = false;
            existingElection.ArchivedDate = null;

            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = "Restored successfully." };
        }

        public async Task<OperationalResult> HardDeleteElectionAsync(int id)
        {
            var existingElection = await _context.Elections
                .FirstOrDefaultAsync(e => e.ElectionId == id);

            if (existingElection == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Election not found."
                };
            }

            // Check first if this election already has recorded votes.
            var voteCount = await _context.Votes
                .CountAsync(v => v.ElectionId == id);

            if (voteCount > 0)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = $"This election cannot be permanently deleted because it already has {voteCount} recorded vote(s). Keep it archived instead."
                };
            }

            // Party lists are not deleted.
            // They are only disconnected from this election
            // so they can be used again later.
            var partyLists = await _context.PartyLists
                .Where(p => p.ElectionId == id)
                .ToListAsync();

            foreach (var party in partyLists)
            {
                party.ElectionId = null;
            }

            // Permanently remove the election.
            _context.Elections.Remove(existingElection);

            await _context.SaveChangesAsync();

            return new OperationalResult
            {
                Success = true,
                Message = "Election permanently deleted."
            };
        }


        public async Task<ArchivedElectionDetailsViewModel?> GetArchivedDetailsAsync(int id)
        {
            var vm = await _context.Elections
                .AsNoTracking()
                .Where(e => e.ElectionId == id && e.IsArchived == true)
                .Select(e => new ArchivedElectionDetailsViewModel
                {
                    ElectionId = e.ElectionId,
                    ElectionName = e.ElectionName,

                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,

                    ArchiveDate = e.ArchivedDate,

                    TotalVotes = _context.Votes
                        .Count(v => v.ElectionId == e.ElectionId),

                    PartyListNames = e.partyLists
                        .Select(p => p.PartyName)
                        .ToList(),

                    TotalPartyLists = e.partyLists.Count(),

                    TotalCandidates = e.partyLists
                        .SelectMany(p => p.candidates)
                        .Count(),

                    Status = ""
                })
                .FirstOrDefaultAsync();

            if (vm == null)
                return null;

            vm.VoteCount = vm.TotalVotes;

            if (vm.TotalPartyLists == 0)
            {
                var voted = await _context.Votes
                    .AsNoTracking()
                    .Where(v => v.ElectionId == id)
                    .Select(v => new
                    {
                        v.CandidateId,
                        PartyName = v.Candidate.PartyList.PartyName
                    })
                    .Distinct()
                    .ToListAsync();

                vm.TotalCandidates = voted.Select(x => x.CandidateId).Distinct().Count();

                vm.PartyListNames = voted
                    .Select(x => x.PartyName)
                    .Distinct()
                    .ToList();

                vm.TotalPartyLists = vm.PartyListNames.Count;
            }

            var now = DateTime.Now;
            var start = vm.StartDate.Date + vm.StartTime;
            var end = vm.EndDate.Date + vm.EndTime;

            vm.Status = now < start ? "Incoming"
                      : now <= end ? "Open"
                      : "Closed";

            return vm;
        }

        public async Task<List<VotingHistoryViewModel>> GetVotingHistoryAsync(string userId)
        {
         

            var history = await _context.Votes
                .AsNoTracking()
                .Where(v => v.UserId == userId)
                .GroupBy(v => v.ElectionId)
                .Select(g => new VotingHistoryViewModel
                {
                    ElectionId = g.Key,

                    // g.First() just grabs one vote from the bucket to read
                    // the election info off of — every vote in this bucket
                    // points to the same election, so any one of them works.
                    ElectionName = g.First().Election.ElectionName,
                    StartDate = g.First().Election.StartDate,
                    EndDate = g.First().Election.EndDate,
                    StartTime = g.First().Election.StartTime,
                    EndTime = g.First().Election.EndTime,

                    // The earliest vote timestamp in this bucket = when they
                    // started voting in this election.
                    VotedDate = g.Min(v => v.DateVoted),

                    Status = "" // filled in below, same reason as GetAllElectionListAsync
                })
                .ToListAsync();


            // ==========================================
            // 2. COMPUTE STATUS + CAN-VIEW-RESULTS
            // ==========================================
            // Same rule as GetAllElectionListAsync — can't run DateTime.Now
            // comparisons inside .Select() because EF Core can't translate
            // that to SQL, so we do it in a normal C# loop afterward.

            var now = DateTime.Now;

            foreach (var entry in history)
            {
                var startDateTime = entry.StartDate.Date + entry.StartTime;
                var endDateTime = entry.EndDate.Date + entry.EndTime;

                if (now < startDateTime)
                {
                    entry.Status = "Incoming";
                }
                else if (now <= endDateTime)
                {
                    entry.Status = "Open";
                }
                else
                {
                    entry.Status = "Closed";
                }

                // Movie-ticket-stub rule: you can only walk into the results
                // room once the show is over.
                entry.CanViewResults = entry.Status == "Closed";
            }


            // ==========================================
            // 3. MOST RECENT ELECTION FIRST
            // ==========================================
            // Like a bank statement — newest transaction on top.

            return history
                .OrderByDescending(h => h.VotedDate)
                .ToList();
        }




    }


}
