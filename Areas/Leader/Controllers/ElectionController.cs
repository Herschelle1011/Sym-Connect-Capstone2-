using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Election;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sym_Connect__Capstone2_.Areas.Leader.Controllers
{
    [Area("Leader")]
    [Authorize(Roles = "Leader")]
    public class ElectionController : Controller
    {
        private readonly IMemberElectionService _electionService;
        private readonly UserManager<User> _userManager;

        public ElectionController(
            IMemberElectionService electionService,
            UserManager<User> userManager)
        {
            _electionService = electionService;
            _userManager = userManager;
        }


        // ==========================================
        // ELECTION LIST
        // GET: /Leader/Elections
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (user.MemberId == null)
                return Forbid();

            var activeElections = await _electionService.getAllElectionList();
            var votingHistory = await _electionService.GetVotingHistoryAsync(user.Id);

            var viewModel = new ElectionIndexViewModel
            {
                ActiveElections = activeElections,
                VotingHistory = votingHistory
            };

            return View(viewModel);
        }


        // ==========================================
        // OPEN BALLOT
        // GET: /Leader/Elections/TakeVoteElection/5
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> TakeVoteElection(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (user.MemberId == null)
                return Forbid();

            // Check if this leader already voted.
            bool alreadyVoted =
                await _electionService.HasMemberVotedAsync(
                    user.Id,
                    id
                );

            if (alreadyVoted)
            {
                return RedirectToAction(nameof(AlreadyVoted));
            }

            // Get all positions and candidates.
            var positionsGroup =
                await _electionService.GetBallot(id);

            if (positionsGroup == null ||
                positionsGroup.Count == 0)
            {
                return NotFound(
                    "No candidates found or this election layout is unavailable.");
            }

            var viewModel = new TakeVoteViewModel
            {
                electionId = id,

                // Your voting service currently uses the
                // Identity User ID when recording a vote.
                memberId = user.Id,

                selectedVotes =
                    new List<SelectedCandidateViewModel>()
            };

            // Create one selection slot for each position.
            foreach (var positionGroup in positionsGroup)
            {
                viewModel.selectedVotes.Add(
                    new SelectedCandidateViewModel
                    {
                        positionId = positionGroup.PositionId,
                        CandidateId = 0
                    });
            }

            ViewBag.BallotLayout = positionsGroup;

            return View(viewModel);
        }


        // ==========================================
        // SUBMIT BALLOT
        // POST: /Leader/Elections/TakeVoteElection
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeVoteElection(
            TakeVoteViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (user.MemberId == null)
                return Forbid();

            // Never trust the User ID sent from the browser.
            // Always use the currently logged-in user.
            model.memberId = user.Id;

            if (!ModelState.IsValid)
            {
                ViewBag.BallotLayout =
                    await _electionService.GetBallot(
                        model.electionId);

                return View(model);
            }

            // Remove positions where no candidate was selected.
            model.selectedVotes.RemoveAll(
                v => v.CandidateId <= 0
            );

            var result =
                await _electionService.CastVote(model);

            switch (result)
            {
                case VoteResult.Success:

                    return RedirectToAction(
                        nameof(ThankYouForVoting));


                case VoteResult.AlreadyVoted:

                    ModelState.AddModelError(
                        string.Empty,
                        "You have already voted in this election.");

                    ViewBag.BallotLayout =
                        await _electionService.GetBallot(
                            model.electionId);

                    return View(model);


                default:

                    ModelState.AddModelError(
                        string.Empty,
                        "Something went wrong while submitting your ballot. Please try again.");

                    ViewBag.BallotLayout =
                        await _electionService.GetBallot(
                            model.electionId);

                    return View(model);
            }
        }


        // ==========================================
        // THANK YOU PAGE
        // ==========================================
        [HttpGet]
        public IActionResult ThankYouForVoting()
        {
            return View();
        }


        // ==========================================
        // ALREADY VOTED PAGE
        // ==========================================
        [HttpGet]
        public IActionResult AlreadyVoted()
        {
            return View();
        }

        public async Task<IActionResult> Results(int electionId)
        {
            var result = await _electionService.GetElectionResultAsync(electionId);
             
            if (result == null)
                return NotFound();

            return View(result);
        }

    }
}