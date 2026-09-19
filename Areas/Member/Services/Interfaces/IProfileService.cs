using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Points;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Profile;
using Sym_Connect__Capstone2_.ViewModels.Profile;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(int? memberId);
        Task<MemberPointDetailsViewModel?> GetMemberPointDetailsAsync(
    int attendanceId,
    int memberId);

        Task<EditPersonalInfoViewModel?> GetEditPersonalInfoAsync(int memberId);

        Task<bool> UpdatePersonalInfoAsync(
            int memberId,
            EditPersonalInfoViewModel model);
    }
}
