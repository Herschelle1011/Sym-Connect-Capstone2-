using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Group
{
    public class GroupEditViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int? LeaderId { get; set; }

        public string? CurrentLeaderName { get; set; }
        public string? CurrentLeaderEmail { get; set; }
        public int? CurrentLeaderPoints { get; set; }
        public string? CurrentLeaderPhotoUrl { get; set; }

        public string Status { get; set; } = "Active";
        public string? Description { get; set; }
        public List<SelectListItem> LeaderOptions { get; set; } = new(); // no longer needed once picker replaces dropdown — can remove
    }
}
