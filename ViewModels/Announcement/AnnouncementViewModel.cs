using Microsoft.AspNetCore.Mvc.Rendering;
using Sym_Connect__Capstone2_.Models.Enums.AnnouncementEnums;
using System.ComponentModel.DataAnnotations;

namespace Sym_Connect__Capstone2_.ViewModels.Announcement
{
    public class AnnouncementViewModel : IValidatableObject
    {
        public int AnnouncementId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public TargetAudience TargetAudience { get; set; }

        public int? GroupId { get; set; }   

        public PublishOption PublishOption { get; set; } = PublishOption.Now;
        public DateTime? PublishDate { get; set; }   

        public ExpirationOption ExpirationOption { get; set; } = ExpirationOption.NoExpiration;
        public DateTime? CustomExpirationDate { get; set; }   

        public List<SelectListItem> GroupOptions { get; set; } = new();

        public bool IsPinned { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (TargetAudience == TargetAudience.SpecificGroup && GroupId is null)
                yield return new ValidationResult(
                    "Please select a group.", new[] { nameof(GroupId) });

            if (PublishOption == PublishOption.Scheduled && PublishDate is null)
                yield return new ValidationResult(
                    "Please pick a schedule date and time.", new[] { nameof(PublishDate) });

            if (ExpirationOption == ExpirationOption.Custom && CustomExpirationDate is null)
                yield return new ValidationResult(
                    "Please pick a custom expiration date.", new[] { nameof(CustomExpirationDate) });

            if (PublishOption == PublishOption.Scheduled)
            {
                if (PublishDate is null)
                    yield return new ValidationResult(
                        "Please pick a schedule date and time.", new[] { nameof(PublishDate) });
                else if (PublishDate.Value <= DateTime.Now)
                    yield return new ValidationResult(
                        "Scheduled date must be in the future.", new[] { nameof(PublishDate) });
            }

            if (ExpirationOption == ExpirationOption.Custom)
            {
                if (CustomExpirationDate is null)
                    yield return new ValidationResult(
                        "Please pick a custom expiration date.", new[] { nameof(CustomExpirationDate) });
                else
                {
                    var effectivePublish = PublishOption == PublishOption.Scheduled && PublishDate.HasValue
                        ? PublishDate.Value
                        : DateTime.Now;

                    if (CustomExpirationDate.Value <= effectivePublish)
                        yield return new ValidationResult(
                            "Expiration must be after the publish date.", new[] { nameof(CustomExpirationDate) });
                }
            }
        }


    }
}
