using System.Security.Claims;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface ICurrentUserContext
    {
        Task<(int? MemberId, bool IsAdmin, bool IsLeader)> ResolveAsync(ClaimsPrincipal user);
    }
}
