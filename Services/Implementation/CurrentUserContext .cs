using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Services.Interfaces;
using System.Security.Claims;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly UserManager<User> _userManager;
        private readonly IMemberService _memberService;
        private readonly IGroupService _groupService;

        public CurrentUserContext(UserManager<User> userManager, IMemberService memberService, IGroupService groupService)
        {
            _userManager = userManager;
            _memberService = memberService;
            _groupService = groupService;
        }

        public async Task<(int? MemberId, bool IsAdmin, bool IsLeader)> ResolveAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null) return (null, false, false);

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (user.MemberId == null) return (null, isAdmin, false);

            var member = await _memberService.GetMemberByIdAsync(user.MemberId.Value);
            if (member == null) return (null, isAdmin, false);

            bool isLeader = await _groupService.IsMemberALeaderAsync(member.MemberId);

            return (member.MemberId, isAdmin, isLeader);
        }
    }
}
