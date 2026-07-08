using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models.Enums
{
    public enum FacilityStatus
    {
        Unknown = 0,
        Active = 1,
        Inactive = 2,
        [Display(Name = "Under Maintenance")]
        UnderMaintenance = 3
    }
}
