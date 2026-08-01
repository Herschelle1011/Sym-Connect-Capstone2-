using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels;
using Sym_Connect__Capstone2_.Models.Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    public class ElectionsController : Controller
    {
        private readonly IElectionService _electionService;

        public ElectionsController(IElectionService electionService)
        {
            _electionService = electionService;
        }

        // GET: /Member/Elections
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var memberIdStr = HttpContext.Session.GetString("CurrentMemberId");
            if (string.IsNullOrEmpty(memberIdStr))
            {
                TempData["Error"] = "Please verify your identity first.";
                return RedirectToAction("VerifyMember");
            }

            var elections = await _electionService.getAllElectionList();
            return View(elections);
        }


        [HttpPost]
        public async Task<IActionResult> VerifyMember(string firstName, string email, int? electionId)
        {
            var member = await _electionService.FindMemberForElectionAsync(firstName, email);

            if (member == null)
            {
                ModelState.AddModelError("", "No matching member found.");
                return View();
            }

            HttpContext.Session.SetString("CurrentMemberId", member.Id.ToString());
            HttpContext.Session.SetString("CurrentMemberName", $"{member.FirstName} {member.LastName}");
            HttpContext.Session.SetString("CurrentMemberEmail", member.Email ?? "");

            if (electionId.HasValue)
            {
                return RedirectToAction(nameof(TakeVoteElection), new
                {
                    id = electionId.Value
                });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Member/Elections/TakeVote/{id}
        [HttpGet]
        public async Task<IActionResult> TakeVoteElection(int id)
        {
            var memberIdStr = HttpContext.Session.GetString("CurrentMemberId");


            if (string.IsNullOrEmpty(memberIdStr))
            {
                TempData["Error"] = "Please verify your identity first.";
                return RedirectToAction("VerifyMember", new { electionId = id });
            }

            bool ifAlreadyVoted = await _electionService.HasMemberVotedAsync(memberIdStr, id);
            if (ifAlreadyVoted)
            {
                return RedirectToAction(nameof(AlreadyVoted));
            }

            // 2. Fetch the segmented candidates and group layout structures from service
            var positionsGroup = await _electionService.GetBallot(id);
            if (positionsGroup == null || positionsGroup.Count == 0)
            {
                return NotFound("No candidates found or this election layout is unavailable.");
            }

            // 3. Build and pre-allocate the ViewModel for index data binding matching inside the Razor View
            var viewModel = new TakeVoteViewModel
            {
                electionId = id,
                memberId = memberIdStr,
                selectedVotes = new List<SelectedCandidateViewModel>()
            };

            // Pre-fill the array index paths so Razor can bind radio collections cleanly back to the server
            foreach (var positionGroup in positionsGroup)
            {
                viewModel.selectedVotes.Add(new SelectedCandidateViewModel
                {
                    position = positionGroup.Position,
                    CandidateId = 0 // Initial default state before selection
                });
            }

            // Keep a temporary visual representation available to render the HTML structure cards
            ViewBag.BallotLayout = positionsGroup;

            return View(viewModel);
        }


        // POST: /Member/Elections/TakeVote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeVoteElection(TakeVoteViewModel model)
        {
            // 1. If any basic schema attributes violate rules, rebuild view layout and show message
            if (!ModelState.IsValid)
            {
                ViewBag.BallotLayout = await _electionService.GetBallot(model.electionId);
                return View(model);
            }

            // 2. Clear out any unselected/blank lines if a user intentionally left radio fields empty
            model.selectedVotes.RemoveAll(v => v.CandidateId <= 0);

            // 3. Fire the secure service execution method
            var result = await _electionService.CastVote(model);

            if (result == VoteResult.AlreadyVoted)
            {
                // Handle potential failures (e.g., Already Voted, Invalid Status, Tampered Candidates)
                ModelState.AddModelError(string.Empty, "Unable to cast your ballot. You may have already voted, chosen multiple candidates for the same job, or select details have closed.");
                ViewBag.BallotLayout = await _electionService.GetBallot(model.electionId);
                return View(model);
            }

            // Successfully committed transaction! Redirect safely back to the dash dashboard
            return RedirectToAction(nameof(ThankYouForVoting));
        }

        [HttpGet]
        public IActionResult ThankYouForVoting()
        {
            return View();
        }

        [HttpGet]
        public IActionResult VerifyMember(int? electionId)
        {
            ViewBag.ElectionId = electionId;
            return View();
        }

        [HttpGet]
        public IActionResult AlreadyVoted()
        {
            return View();
        }

    }
}
