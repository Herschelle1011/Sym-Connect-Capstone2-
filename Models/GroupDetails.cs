namespace Sym_Connect__Capstone2_.Models
{
    public class GroupDetails
    {
        public int GroupDetailsId { get; set; }
        public int GroupId { get; set; }
        public int MemberId { get; set; }


        //NAVIGATION PROPERTIES (FK)
        public Member? Member { get; set; }
        public Group? Group { get; set; }

    }
}
