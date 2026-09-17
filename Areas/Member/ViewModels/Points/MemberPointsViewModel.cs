namespace Sym_Connect__Capstone2_.Areas.Member.ViewModels.Points
{
    public class MemberPointsViewModel
    {
        // Top summary cards
        public int TotalPoints { get; set; }

        public int AttendanceCount { get; set; }

        public int AbsentCount { get; set; }


        // Activity cards underneath
        public List<MemberPointActivityViewModel> Activities { get; set; }
            = new List<MemberPointActivityViewModel>();
    }
}
