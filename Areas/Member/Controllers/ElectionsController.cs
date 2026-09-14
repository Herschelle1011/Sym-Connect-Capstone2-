using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Election;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Models.Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sym_Connect__Capstone2_.Areas.Member.Controllers
{
    [Area("Member")]
    [Authorize(Roles = "Member")]
    public class ElectionsController : Controller
    {
        private readonly IElectionService _electionService;
        private readonly UserManager<User> _userManager;


        public ElectionsController(IElectionService electionService, UserManager<User> userManager)
        {
            _electionService = electionService;
            _userManager = userManager;

        }

        // GET: /Member/Elections
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();



            if (user.MemberId == null)
                return Forbid();

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
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            if (user.MemberId == null)
                return Forbid();

            bool alreadyVoted =
                await _electionService.HasMemberVotedAsync(user.Id, id);

            if (alreadyVoted)
            {
                return RedirectToAction(nameof(AlreadyVoted));
            }

            var positionsGroup =
                await _electionService.GetBallot(id);

            if (positionsGroup == null || positionsGroup.Count == 0)
            {
                return NotFound(
                    "No candidates found or this election layout is unavailable.");
            }

            var viewModel = new TakeVoteViewModel
            {
                electionId = id,
                memberId = user.Id,

                selectedVotes = new List<SelectedCandidateViewModel>()
            };

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


        // POST: /Member/Elections/TakeVote
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

            // Always use the logged-in user's real Identity ID
            model.memberId = user.Id;

            if (!ModelState.IsValid)
            {
                ViewBag.BallotLayout =
                    await _electionService.GetBallot(model.electionId);

                return View(model);
            }

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
                        await _electionService.GetBallot(model.electionId);

                    return View(model);

                default:
                    ModelState.AddModelError(
                        string.Empty,
                        "Something went wrong while submitting your ballot. Please try again.");

                    ViewBag.BallotLayout =
                        await _electionService.GetBallot(model.electionId);

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
