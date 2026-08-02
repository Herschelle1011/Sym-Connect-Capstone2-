using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.Models.Enums
{
    public enum Positions
    {
        [Display(Name = "President")]
        President,

        [Display(Name = "Internal")]
        Internal,

        [Display(Name = "External Vice President")]
        ExternalVP,

        [Display(Name = "Secretary")]
        Secretary,

        [Display(Name = "Treasurer")]
        Treasurer,

        [Display(Name = "Auditor")]
        Auditor,

        [Display(Name = "PIO")]
        PIO,
    }
}