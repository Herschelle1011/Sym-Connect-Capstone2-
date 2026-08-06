using AspNetCoreGeneratedDocument;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.ElectionModels;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.Election;
using Sym_Connect__Capstone2_.ViewModels.PartyList;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;


namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class PartyListService : IPartyListService
    {
        private readonly AppDbContext _context;

        public PartyListService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PartyListsViewModel>> GetAllPartyListsAsync()
        {
            return await _context.PartyLists
                .AsNoTracking()
                        .Where(p => p.isArchived == false)
                        .Select(p => new PartyListsViewModel
                        {
                            Id = p.PartyListId,
                            PartyListName = p.PartyName,
                            PartyListDescription = p.PartyListDescription,
                            //DateCreated = p.DateCreated ?? DateTime.Today 

                        }).ToListAsync();
        }




        public async Task<List<PartyListsViewModel>> GetUnassignedPartyListsAsync()
        {
            return await _context.PartyLists
                .AsNoTracking() 
                .Where(p => p.ElectionId == null && p.isArchived == false) 
                .Select(p => new PartyListsViewModel
                {
                    Id = p.PartyListId,
                    PartyListName = p.PartyName,
                    PartyListDescription = p.PartyListDescription,
 
                    candidates = new List<CandidateViewModel>()
                })
                .ToListAsync(); 
                }

       public async Task<PartyListDetailsViewModel?> GetPartyListByIdAsync(int id)
        {
            var existingPartyList = await _context.PartyLists
                                    .AsNoTracking()
                        .Where(p => p.PartyListId == id && p.isArchived == false)
        .Select(p => new PartyListDetailsViewModel
        {
            partyListid = p.PartyListId,
            partyListName = p.PartyName,
            partyDescription = p.PartyListDescription,
            ElectionId = p.ElectionId,
            Candidates = p.candidates.Select(c => new CandidateViewModel
            {
                CandidateId = c.CandidateId,
                PositionId = c.positionId,
                PositionName = c.Position.PositionName,
                FullName = c.Member.FirstName + " " + c.Member.LastName,
                PhotoUrl = string.IsNullOrEmpty(c.Member.ProfilePictureUrl)
                    ? "/images/default-avatar.png"
                    : c.Member.ProfilePictureUrl
            }).ToList()
        })
      .FirstOrDefaultAsync();
            return existingPartyList;
        }


        public async Task<GetPartyListPositionViewModel> GetPositionsForPartyListAsync(int partyListId)
        {
            var partyList = await _context.PartyLists.FindAsync(partyListId);

            var positions = await _context.positions
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();

            var candidates = await _context.Candidates
                .Where(c => c.PartyId == partyListId)
                .Include(m => m.Member)
                .ToListAsync();


            var viewModel = new GetPartyListPositionViewModel
            {
                partyListId = partyList.PartyListId,
                PartyListName = partyList.PartyName,
                Positions = positions.Select(p =>
                {

                    var positionCandidates = candidates.Where(c => c.positionId == p.PositionId);

                    var slots = positionCandidates.Select(c => new CandidateSlotDisplayViewModel
                    {
                        Candidate = new CandidateViewModel
                        {
                            CandidateId = c.CandidateId,
                            PositionId = c.positionId,
                            PositionName = p.PositionName,
                            FullName = c.Member.FullName,
                            MemberId = c.MemberId,
                            PhotoUrl = string.IsNullOrEmpty(c.Member.ProfilePictureUrl)
                              ? "/images/default-avatar.png"
                              : c.Member.ProfilePictureUrl
                        }
                    }).ToList();
    
                    while(slots.Count < p.MaxCandidates)
                    {
                        slots.Add(new CandidateSlotDisplayViewModel { Candidate = null });
                    }
                    return new PartyListPositionItemViewModel
                    {
                        positionId = p.PositionId,
                        positionName = p.PositionName,
                        DisplayOrder = p.DisplayOrder,
                        maxCandidates = p.MaxCandidates,
                        Slots = slots                   
                    };
                }).ToList()
            };

            return viewModel;
        }



        public async Task AssignPartyListsToElectionAsync(int electionId, List<int> partyListIds)
        {
            var election = await _context.Elections
                        .FirstOrDefaultAsync(e => e.ElectionId == electionId);
            if (election == null) return;
            var partyListsToAssign = await _context.PartyLists
                       .Where(p => partyListIds.Contains(p.PartyListId) && p.ElectionId == null)
                       .ToListAsync();

            foreach (var party in partyListsToAssign)
            {
                party.ElectionId = electionId;
            }
            await _context.SaveChangesAsync();
        }

        public async Task CreatePartyListAsync(CreatePartyListViewModel vm)
        {
            var NewPartyList = new PartyList
            {
                PartyName = vm.PartyListName,
                PartyListDescription = vm.PartyListDescription,
                //DateCreated = DateTime.Now
            };
            _context.PartyLists.Add(NewPartyList);
            await _context.SaveChangesAsync();
        }

        public async Task<BulkAddCandidatesResult> AddOrUpdateCandidatesAsync(BulkAddCandidatesViewModel model)
        {
            var Result = new BulkAddCandidatesResult { Success = true };

            var partyListExists = await _context.PartyLists.AnyAsync(p => p.PartyListId == model.partyListId);
            if (!partyListExists) {
                Result.Success = false;
                Result.Message = "Party list not found.";
                return Result;
            }
            ;

            foreach (var assignment in model.Assignments)
            {

                var memberExists = await _context.Members.AnyAsync(m => m.MemberId == assignment.MemberId);
                if (!memberExists) continue;

                if (assignment.ExistingCandidateId.HasValue)
                {
                    var currentCandidate = await _context.Candidates
                        .FirstOrDefaultAsync(c => c.CandidateId == assignment.ExistingCandidateId.Value);

                    if (currentCandidate != null && currentCandidate.MemberId == assignment.MemberId)
                    {
                        continue; 
                    }
                }


                var alreadyAssignedElsewhere = await _context.Candidates
                 .AnyAsync(c => c.PartyId == model.partyListId
                     && c.MemberId == assignment.MemberId
                     && c.positionId != assignment.PositionId);

                var alreadyInThisPosition = await _context.Candidates
                .AnyAsync(c => c.PartyId == model.partyListId
                    && c.positionId == assignment.PositionId
                    && c.MemberId == assignment.MemberId);

                if (alreadyInThisPosition)
                {
                    Result.Success = false;
                    if(!Result.Errors.Contains("This member is already assigned to this position."))
                    Result.Errors.Add("This member is already assigned to this position.");
                    continue;
                }



                if (alreadyAssignedElsewhere)
                {
                    Result.Success = false;
                    if (!Result.Errors.Contains("This member is already assigned to this position."))
                    Result.Errors.Add($"This member is already assigned to another position in this party list.");
                    continue; 
                }

                if (assignment.ExistingCandidateId.HasValue)
                {
                    var existingCandidate = await _context.Candidates
                        .FirstOrDefaultAsync(c => c.PartyId == model.partyListId && c.positionId == assignment.PositionId);

                    if (existingCandidate != null) existingCandidate.MemberId = assignment.MemberId;
                }
                else
                {
                    var position = await _context.positions.FindAsync(assignment.PositionId);

                    var existingCount = await _context.Candidates.Where(c => c.PartyId == model.partyListId && c.positionId == assignment.PositionId).CountAsync();

                    if (existingCount >= position.MaxCandidates)
                    {
                        Result.Success = false;
                        Result.Errors.Add($"{position.PositionName} is already full.");
                        continue;
                    }

                    _context.Candidates.Add(new Candidates
                    {
                        PartyId = model.partyListId,
                        positionId = assignment.PositionId,
                        MemberId = assignment.MemberId

                    });
                }            
            }
            if (Result.Success)
            {
                await _context.SaveChangesAsync();
            }
            return Result;

        }

        public async Task AddCandidateAsync(AddCandidateViewModel vm)
        {
            var existingPartyList = await _context.PartyLists.AnyAsync(p => p.PartyListId == vm.partyListId);
            if (!existingPartyList) return;
            var existingMember = await _context.Members.AnyAsync(m => m.MemberId == vm.memberId);
            if (!existingMember) return;

            var newCandidate = new Candidates
            {
                PartyId = vm.partyListId,
                positionId = vm.positionId,
                MemberId = vm.memberId
            };
            _context.Candidates.Add(newCandidate);
            await _context.SaveChangesAsync();
        }

        public async Task EditPartyListAsync(EditPartyListViewModel vm)
        {
            var existingPartyList = await _context.PartyLists.FirstOrDefaultAsync(p => p.PartyListId == vm.partyListId);
            if (existingPartyList == null) return;
            existingPartyList.PartyName = vm.PartyListName;
            existingPartyList.PartyListDescription = vm.PartyListDescription;

            await _context.SaveChangesAsync();
        }


        public async Task<ArchiveModalViewModel> GetArchiveModalAsync(int partyListId)
        {
            var partyList = await _context.PartyLists
                .FirstOrDefaultAsync(p => p.PartyListId == partyListId); // adjust PK name

            if (partyList == null) return null;

            var candidateNames = await _context.Candidates
                .Where(c => c.PartyId == partyListId)
                .Include(c => c.Member)
                .Select(c => c.Member.FirstName + " " + c.Member.LastName) // adjust to your naming
                .ToListAsync();

            return new ArchiveModalViewModel
            {
                partylistId = partyList.PartyListId,
                partyListName = partyList.PartyName,
                partyDescription = partyList.PartyListDescription,
                CandidateNames = candidateNames
            };
        }



        public async Task ArchivePartyListAsync(int id)
        {
            var candidates = await _context.Candidates.Where(c => c.PartyId == id).ToListAsync();

            var candidateIds = candidates.Select(c => c.CandidateId).ToList();

            var vote = await _context.Votes.Where(v => candidateIds.Contains(v.CandidateId)).ToListAsync();

            _context.Votes.RemoveRange(vote); //remove connected fk first
            _context.Candidates.RemoveRange(candidates);
            await _context.SaveChangesAsync();

            var existingPartyList = await _context.PartyLists.FindAsync(id);
            if (existingPartyList == null) return;
            if(existingPartyList != null)
            {
                existingPartyList.isArchived = true;
                existingPartyList.ArchivedDate = DateTime.Now;

                await _context.SaveChangesAsync();
            }
         
        }
        public async Task<List<CandidateMemberSearchViewModel>> SearchActiveMembersAsync(string term, int partyListId)
        {
            if (string.IsNullOrWhiteSpace(term)) return new List<CandidateMemberSearchViewModel>();

            string cleanTerm = term.Trim().ToLower();

            var currentElectionId = await _context.PartyLists
                .Where(p => p.PartyListId == partyListId)
                .Select(p => p.ElectionId)
                .FirstOrDefaultAsync();

            return await _context.Members
                .AsNoTracking()
                .Where(m => m.Status == "Active" &&
                            !_context.Candidates.Any(c => c.MemberId == m.MemberId && c.PartyList.ElectionId == currentElectionId))
                .Select(m => new
                {
                    Member = m,
                    NormalizedFullName = (m.FirstName + " " + (m.MiddleName ?? "") + " " + m.LastName)
                        .Replace("  ", " ").Trim()
                })
                .Where(x => x.NormalizedFullName.ToLower().Contains(cleanTerm))
                .Select(x => new CandidateMemberSearchViewModel
                {
                    MemberId = x.Member.MemberId,
                    FullName = x.Member.FirstName + " " + x.Member.LastName,
                    PhotoUrl = x.Member.ProfilePictureUrl
                })
                .ToListAsync();
        }
    }
}
