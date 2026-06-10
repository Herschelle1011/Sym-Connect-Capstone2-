using Microsoft.AspNetCore.Http;
using Sym_Connect__Capstone2_.Models;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels
{
    public class FacilityViewModel
    {

        public class FacilityListViewModel
        {
            public int FacilityId { get; set; }

            public string FacilityName { get; set; } = string.Empty;

            public string FacilityDescription { get; set; } = string.Empty;

            public int Capacity { get; set; }

            public Facility.FacilityStatus Status { get; set; }

            public List<string> FacilityTypes { get; set; } = new();

            public List<string> Photos { get; set; } = new();
        }




        public class CreateFacilityViewModel
        {
            [Required(ErrorMessage = "Facility name is required.")]
            [StringLength(100, ErrorMessage = "Facility name cannot exceed 100 characters.")]
            public string FacilityName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Description is required.")]
            [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
            public string FacilityDescription { get; set; } = string.Empty;

            [Required(ErrorMessage = "Capacity is required.")]
            [Range(2, 500, ErrorMessage = "Capacity must be between 2 and 500.")]
            public int Capacity { get; set; }

            [Required(ErrorMessage = "Status is required.")]
            public Facility.FacilityStatus Status { get; set; }
                = Facility.FacilityStatus.Active;

            [Required(ErrorMessage = "Facility type is required.")]
            public string FacilityTypes { get; set; } = string.Empty;

            // For uploading photos from the form
            public List<IFormFile> Photos { get; set; } = new List<IFormFile>();
        }

        public class EditFacilityViewModel
        {
            public int FacilityId { get; set; }

            [Required(ErrorMessage = "Facility name is required.")]
            [StringLength(100, ErrorMessage = "Facility name cannot exceed 100 characters.")]
            public string FacilityName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Description is required.")]
            [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
            public string FacilityDescription { get; set; } = string.Empty;

            [Required(ErrorMessage = "Capacity is required.")]
            [Range(2, 500, ErrorMessage = "Capacity must be between 2 and 500.")]
            public int Capacity { get; set; }

            [Required(ErrorMessage = "Status is required.")]
            public Facility.FacilityStatus Status { get; set; }
                = Facility.FacilityStatus.Active;

            [Required(ErrorMessage = "Facility type is required.")]
            public string FacilityTypes { get; set; } = string.Empty;

            // For uploading new photos during edit
            public List<IFormFile> Photos { get; set; } = new List<IFormFile>();

            // To display existing photos already saved
            public List<ExistingPhotoViewModel> ExistingPhotos { get; set; }
            = new List<ExistingPhotoViewModel>();

            public List<int> RemovedPhotoIds { get; set; } = new List<int>();
        }

        public class ArchiveFacilityViewModel
        {
            public int facilityId { get; set; }
            public string FacilityName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int Capacity { get; set; }
        }

        public class MaintenanceFacilityViewModel
        {
            public int facilityId { get; set; }
            public string FacilityName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int Capacity { get; set; }
        }

        public class ExistingPhotoViewModel
        {
            public int PhotoId { get; set; }
            public string PhotoUrl { get; set; } = string.Empty;

        }

        public class FacilityDetailsViewModel
        {
            public int FacilityId { get; set; }

            public string FacilityName { get; set; } = string.Empty;

            public string FacilityDescription { get; set; } = string.Empty;

            public string FacilityType { get; set; } = string.Empty;

            public int Capacity { get; set; }

            public Facility.FacilityStatus Status { get; set; }

            public List<string> Photos { get; set; } = new();
        }


    }
}