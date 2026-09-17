using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Points;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces
{
    public interface IMemberPointsService
    {
        Task<MemberPointsViewModel> GetMemberPointsAsync(int memberId);

        Task<MemberPointDetailsViewModel?> GetMemberPointDetailsAsync(
   int attendanceId,
   int memberId);


    }
}
