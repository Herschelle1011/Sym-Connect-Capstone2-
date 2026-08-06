namespace Sym_Connect__Capstone2_.ViewModels.PartyList.Positions
{
    public class GetPositionListViewModel
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public bool IsActive = true;
        public int DisplayOrder { get; set; }
        public int maxCandidate { get; set; }

    }
}
