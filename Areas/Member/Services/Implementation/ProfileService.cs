using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Areas.Member.ViewModels.Profile;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Services.Interfaces;

namespace Sym_Connect__Capstone2_.Areas.Member.Services.Implementation
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly IMemberService _memberService;


        public ProfileService(AppDbContext context, IMemberService memberService)
        {
            _context = context;
            _memberService = memberService;
        }


        public async Task<ProfileViewModel?> GetProfileAsync(int? memberId)
        {
            if (memberId == null) return null;

           var profile = await _context.Members
                .Where(m => m.MemberId == memberId)
                .Select(m => new ProfileViewModel
                {
                    memberId = m.MemberId,
                    FirstName = m.FirstName ?? "",
                    MiddleName = m.MiddleName ?? "",
                    LastName = m.LastName ?? "",
                    Address = m.Address,
                    ContactNumber = m.ContactNumber,
                    PhotoUrl = m.ProfilePictureUrl
                }).FirstOrDefaultAsync();

            if (profile == null) return null;

            profile.MyQrToken = await _memberService.GetOrCreateQrTokenAsync(memberId.Value);

            return profile;

        }
    }
}
