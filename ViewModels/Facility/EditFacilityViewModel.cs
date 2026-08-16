using Sym_Connect__Capstone2_.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Facility
{
    public class EditFacilityViewModel : FormFacilityViewModel
    {
        public int FacilityId { get; set; }

        public List<ExistingPhotoViewModel> ExistingPhotos { get; set; } = new List<ExistingPhotoViewModel>();
        public List<int> RemovedPhotoIds { get; set; } = new List<int>();
    }
}
