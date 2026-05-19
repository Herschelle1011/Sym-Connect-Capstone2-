using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
namespace Sym_Connect__Capstone2_.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadProfilePicture(IFormFile file, string userId)
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = $"profiles/{userId}",   // organized folder in Cloudinary
                Overwrite = true,                    // replaces old photo automatically
                Transformation = new Transformation()
                                .Width(200)
                                .Height(200)
                                .Crop("fill")         // auto-crop to a square
                                .Gravity("face")      // focuses on the face!
            };

            var result = await _cloudinary.UploadAsync(uploadParams);


            return result.Error == null ? result.SecureUrl.ToString() : null;

        }

        public async Task DeleteProfilePictureAsync(string userId)
        {
            var deleteParams = new DeletionParams($"profiles/{userId}");
            await _cloudinary.DestroyAsync(deleteParams);
        }

    }
}
