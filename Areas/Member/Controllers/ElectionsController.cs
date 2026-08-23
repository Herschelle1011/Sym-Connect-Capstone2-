using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Election;
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

            HttpContext.Session.SetString("CurrentMemberId", member.MemberId.ToString());
            HttpContext.Session.SetString("CurrentMemberName", $"{member.FirstName} {member.LastName}");
            HttpContext.Session.SetString("CurrentMemberEmail", member.User.Email ?? "");

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

            var positionsGroup = await _electionService.GetBallot(id);
            if (positionsGroup == null || positionsGroup.Count == 0)
            {
                return NotFound("No candidates found or this election layout is unavailable.");
            }

            var viewModel = new TakeVoteViewModel
            {
                electionId = id,
                memberId = memberIdStr,
                selectedVotes = new List<SelectedCandidateViewModel>()
            };

            foreach (var positionGroup in positionsGroup)
            {
                viewModel.selectedVotes.Add(new SelectedCandidateViewModel
                {
                    positionId = positionGroup.PositionId,
                    CandidateId = 0 
                });
            }

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

            var result = await _electionService.CastVote(model);

            switch (result)
            {
                case VoteResult.Success:
                    return RedirectToAction(nameof(ThankYouForVoting));

                case VoteResult.AlreadyVoted:
                    ModelState.AddModelError(string.Empty, "You have already voted in this election.");
                    ViewBag.BallotLayout = await _electionService.GetBallot(model.electionId);
                    return View(model);

                case VoteResult.InvalidSelection:
                default:
                    ModelState.AddModelError(string.Empty, "Something went wrong while submitting your ballot. Please try again.");
                    ViewBag.BallotLayout = await _electionService.GetBallot(model.electionId);
                    return View(model);
            }

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
