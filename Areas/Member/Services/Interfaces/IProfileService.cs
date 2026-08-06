using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Profile;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(int? memberId);
    }
}
