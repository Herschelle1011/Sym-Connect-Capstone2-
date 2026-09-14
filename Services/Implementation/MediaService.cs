using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Hubs;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.MediaAndDocuments;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments;
using Sym_Connect__Capstone2_.ViewModels.Notification;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class MediaService : IMediaService
    {
        private readonly AppDbContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;


        private static readonly HashSet<string> AllowedExtensions = new()
        {
               ".jpg", ".jpeg", ".png", ".gif", ".webp",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx"
        };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;
        public MediaService(AppDbContext context, Cloudinary cloudinary, INotificationService notificationService, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _cloudinary = cloudinary;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        //======= FOLDERS ===== // 

        public async Task<List<MediaFolderViewModel>> GetAllFoldersAsync(int viewerMemberId, bool isAdmin)
        {
            return await _context.MediaFolders
                            .AsNoTracking()
                            .Where(f => f.isArchived == false && (!f.IsPrivate || isAdmin || f.CreatedByMemberId == viewerMemberId))
                            .OrderBy(f => f.FolderName)
                            .Select(f => new MediaFolderViewModel
                            {
                                MediaFolderId = f.MediaFolderId,
                                FolderName = f.FolderName,
                                CreatedDate = f.CreatedDate,
                                IsPrivate = f.IsPrivate,
                               isArchived = false,
                                FileCount = f.Files.Count(file => file.IsArchived == false && file.MediaFileStatus == MediaFileStatus.Published)
                            }).ToListAsync();
        }

        public async Task<OperationalResult> CreateFolderAsync(string folderName, int createdByMemberId, bool isPrivate)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                return new OperationalResult { Success = false, Message = "Folder name is required" };

            bool exists = await _context.MediaFolders.AnyAsync(f => f.FolderName.Trim().ToLower() == folderName.Trim().ToLower());

            if (exists) return new OperationalResult { Success = false, Message = $"{folderName} already exist enter another folder name" };

            var folder = new MediaFolder
            {
                FolderName = folderName,
                CreatedDate = DateTime.Now,
                CreatedByMemberId = createdByMemberId,
                isArchived = false,
                IsPrivate = isPrivate
            };

       

            await _context.MediaFolders.AddAsync(folder);
            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = "Folder created successfully!" };
        }

        public async Task<MediaFolder> GetFolderByIdAsync(int id)
        {
            return await _context.MediaFolders.FirstOrDefaultAsync(f => f.MediaFolderId == id);
        }

        public async Task<MediaFile> GetFileByIdAsync(int id)
        {
            return await _context.MediaFiles.FirstOrDefaultAsync(f => f.MediaFileId == id);
        }



        public async Task<OperationalResult> DeleteFolderAsync(int folderId, int requestingMemberId, bool isAdmin)
        {
            var folder = await _context
                            .MediaFolders.Include(f => f.Files)
                            .FirstOrDefaultAsync(f => f.MediaFolderId == folderId);

            if (folder == null)
                return new OperationalResult { Success = false, Message = "Folder does not exist" };

            // admin can delete any folder

            if (!isAdmin && folder.CreatedByMemberId != requestingMemberId)
                return new OperationalResult { Success = false, Message = "You don't have permission to delete this folder." };


            foreach (var file in folder.Files)
            {
                await DestroyFromCloudinaryAsync(file.PublicId, file.ResourceType);
            }
            _context.MediaFiles.RemoveRange(folder.Files);
            _context.MediaFolders.Remove(folder);
            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = "Folder Deleted Successfully" };
        }

        //boolean who can view the files?
        public async Task<bool> CanViewFolderAsync(int folderId, int viewerMemberId, bool isAdmin)
        {
            var folder = await _context.MediaFolders.AsNoTracking().FirstOrDefaultAsync(f => f.MediaFolderId == folderId);
            if (folder == null) return false;

            //if its not private return true anyone can view(for members)
            if (!folder.IsPrivate) return true;

            //if false only the owner or admin can view the folder/file
            return isAdmin || folder.CreatedByMemberId == viewerMemberId;

        }



        //==== FILES ====
        public async Task<List<MediaFileViewModel>> GetFilesInFolderAsync(int? folderId)
        {
            return await _context.MediaFiles
                .AsNoTracking()
                .Where(f => f.MediaFolderId == folderId && f.IsArchived == false && f.MediaFileStatus == MediaFileStatus.Published)
                .OrderByDescending(f => f.UploadDate)
                .Select(f => new MediaFileViewModel
                {           
                    MediaFileId = f.MediaFileId,
                    folderName = f.Folder.FolderName,
                    FileName = f.FileName,
                    FileUrl = f.FileUrl,
                    IsFolderPrivate = f.Folder.IsPrivate,
                    FileExtension = f.FileExtension,
                    FileSizeBytes = f.FileSizeBytes,
                    Status = f.MediaFileStatus,
                    UploadedDate = f.UploadDate,
                    UploadedBy = f.Member.FirstName + " " + f.Member.LastName,
                    UploadedByMemberId = f.UploadedByMemberId
                }).ToListAsync();
        }

        public async Task<OperationalResult> UploadFilesAsync(int? folderId, List<IFormFile> files, int uploadedByMemberId, bool isAdmin)
        {
            if(files == null || !files.Any())
                     return new OperationalResult { Success = false, Message = "No Files were selected," };

            var folder = await _context.MediaFolders.FirstOrDefaultAsync(f => f.MediaFolderId == folderId);
            if(folder == null)
                return new OperationalResult { Success = false, Message = "Target folder does not exists" };


            if (folderId.HasValue)
            {
                bool folderExists = await _context.MediaFolders.AnyAsync(f => f.MediaFolderId == folderId);
                if (!folderExists) return new OperationalResult { Success = false, Message = "Target folder does not exists" };
            }


            bool canAccess = !folder.IsPrivate || isAdmin || folder.CreatedByMemberId == uploadedByMemberId;
            if (!canAccess)
                return new OperationalResult { Success = false, Message = "You don't have permission to upload to this folder." };

            //is admin or no approvalStatus
            var status = isAdmin ? MediaFileStatus.Published : MediaFileStatus.Pending;
            MediaFile? firstUploadedMediaFile = null;

            foreach (var file in files)
            {

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    return new OperationalResult { Success = false, Message = $"{file.FileName}: file type {ext} is not allowed." };
                }

                if(file.Length > MaxFileSizeBytes)
                    return new OperationalResult { Success = false, Message = $"{file.FileName} exceeds the 10MB limit." };

                var resourceType = (ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".pdf") ? "image" : "raw";

                await using var stream = file.OpenReadStream();
                RawUploadResult uploadResult;

                if(resourceType == "image")
                {
                    var imageParams = new ImageUploadParams
                    {
                        File = new CloudinaryDotNet.FileDescription(file.FileName, stream),
                        Folder = "media-library",
                        UseFilename = true,
                        UniqueFilename = true
                    };
                    uploadResult = await _cloudinary.UploadAsync(imageParams);
                }
                else
                {
                    var rawParams = new RawUploadParams
                    {
                        File = new CloudinaryDotNet.FileDescription(file.FileName, stream),
                        Folder = "media-library",
                        UseFilename = true,
                        UniqueFilename = true,
                    };
                    uploadResult = await _cloudinary.UploadAsync(rawParams, "raw"); 
                }

                if(uploadResult.Error != null)
                    return new OperationalResult { Success = false, Message = $"Upload failed for {file.FileName}: {uploadResult.Error.Message}" };


                var mediaFile = new MediaFile
                {
                    MediaFolderId = folderId,
                    FileName = file.FileName,
                    FileUrl = uploadResult.SecureUrl.ToString(),
                    PublicId = uploadResult.PublicId,
                    ResourceType = resourceType,
                    IsArchived = false,
                    FileExtension = ext,
                    FileSizeBytes = file.Length,
                    UploadDate = DateTime.Now,
                    UploadedByMemberId = uploadedByMemberId,


                    MediaFileStatus = status,
                    ApprovedByMemberId = isAdmin ? uploadedByMemberId : null,
                    ApprovalDate = isAdmin ? DateTime.Now : null

                };      
                _context.MediaFiles.Add(mediaFile);

                if (firstUploadedMediaFile == null)
                {
                    firstUploadedMediaFile = mediaFile;
                }
            }
            await _context.SaveChangesAsync();

            if (!isAdmin)
            {
                await _hubContext.Clients.Group("Admins")
                    .SendAsync("PendingFilesChanged");

                await _notificationService.NotifyAdminAsync(
                    new NotificationViewModel
                    {
                        Title = "New Files Pending Approval",

                        Message = files.Count == 1
                            ? $"A new file \"{files[0].FileName}\" was uploaded and is waiting for approval."
                            : $"{files.Count} new files were uploaded and are waiting for approval.",

                    Url = firstUploadedMediaFile != null
                    ? $"/MediaAndDocuments/PendingFiles?openFileId={firstUploadedMediaFile.MediaFileId}"
                    : "/MediaAndDocuments/PendingFiles"
                    }
                );

                return new OperationalResult
                {
                    Success = true,

                    Message = files.Count == 1
                        ? "File uploaded successfully and is waiting for approval."
                        : "Files uploaded successfully and are waiting for approval."
                };
            }

            return new OperationalResult { Success = true, Message = "Files Uploaded" };
        }


        public async Task<List<MediaFileViewModel>> RecentFiles(int viewerMemberId, bool isAdmin)
        {
            Console.WriteLine("RECENT FILES METHOD CALLED");

            var recent = await _context.MediaFiles
                .AsNoTracking()
             .Where(f =>
            f.IsArchived == false &&

            // Viewer must have access to the folder
            (f.MediaFolderId == null ||
             !f.Folder.IsPrivate ||
             isAdmin ||
             f.UploadedByMemberId == viewerMemberId) &&

            // Recent Files only shows published files
            f.MediaFileStatus == MediaFileStatus.Published
        )
                .OrderByDescending(e => e.UploadDate)
                .Take(5)
                .Select(f => new MediaFileViewModel
                {
                    MediaFileId = f.MediaFileId,
                    FileName = f.FileName,
                    FileExtension = f.FileExtension,
                    UploadedByMemberId = f.UploadedByMemberId,
                    folderName = f.Folder.FolderName,
                    fullName = f.Member.FirstName + " " + f.Member.LastName ?? "N/A",
                    UploadedDate = f.UploadDate,
                    Status = f.MediaFileStatus
                }).ToListAsync();

            return recent;
        }



        public async Task<OperationalResult> DeleteFileAsync(int mediaFileId, int requestingMemberId, bool isAdmin)
        {
            var file = await _context.MediaFiles.FirstOrDefaultAsync(f => f.MediaFileId == mediaFileId);
            if (file == null)
                return new OperationalResult { Success = false, Message = "File not found." };

            // Same rule as folders: Admin deletes anything, Leader deletes only what they uploaded.
            if (!isAdmin && file.UploadedByMemberId != requestingMemberId)
                return new OperationalResult { Success = false, Message = "You don't have permission to delete this file." };

            await DestroyFromCloudinaryAsync(file.PublicId, file.ResourceType);

            _context.MediaFiles.Remove(file);
            await _context.SaveChangesAsync();

            return new OperationalResult { Success = true, Message = "File deleted." };
        }

        private async Task DestroyFromCloudinaryAsync(string publicId, string resourceType)
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType == "image" ? ResourceType.Image : ResourceType.Raw
            };
            await _cloudinary.DestroyAsync(deleteParams);
        }

        public async Task<OperationalResult> RenameFolderAsync(int id, string newname)
        {
            var folder = await _context.MediaFolders.FirstOrDefaultAsync(f => f.MediaFolderId == id);
            if (folder == null)
            {
                return new OperationalResult { Success = false, Message = "Folder not found" };
            }

            if (folder.FolderName.Trim().ToLower() == newname.Trim().ToLower())
            {
                return new OperationalResult { Success = false, Message = "Folder name already exists" };
            }

            folder.FolderName = newname;
            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = "Renamed folder successfully" };

        }

        public async Task<OperationalResult> RenameFileAsync(int id, string newname)
        {
            var file = await _context.MediaFiles.FirstOrDefaultAsync(f => f.MediaFileId == id);
            if (file == null)
            {
                return new OperationalResult { Success = false, Message = "File not found" };
            }

            newname = newname.Trim();

            if (string.IsNullOrWhiteSpace(newname))
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "File name is required."
                };
            }

            newname = Path.GetFileNameWithoutExtension(newname);

            string completeFileName = newname + file.FileExtension;


            bool nameExists = await _context.MediaFiles
                .AnyAsync(f =>
             f.MediaFileId != id &&
             f.MediaFolderId == file.MediaFolderId &&
             f.FileName.ToLower() ==
                 completeFileName.ToLower() &&
             f.IsArchived == false);

            if (nameExists)
            {
                return new OperationalResult { Success = false, Message = "File name already exists" };
            }

            file.FileName = completeFileName;
            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = "File renamed successfully" };
        }


        public async Task<MediaFileViewModel?> GetFileForDownloadAsync(int mediaFileId, int? viewerMemberId, bool isAdmin)
        {
            var file = await _context.MediaFiles
                .AsNoTracking()
                .Where(f => f.MediaFileId == mediaFileId && f.IsArchived == false)
                .Select(f => new MediaFileViewModel
                {
                    MediaFileId = mediaFileId,
                    FileName = f.FileName,
                    FileUrl = f.FileUrl,
                    FileExtension = f.FileExtension,
                    IsFolderPrivate = f.MediaFolderId != null && f.Folder.IsPrivate,
                    FolderCreatedByMemberId = f.MediaFolderId != null ? f.Folder.CreatedByMemberId : null
                }).FirstOrDefaultAsync();

            if (file == null) return null;

            // ACCESS FILE > 

            //if folder is not private 
            if (!file.IsFolderPrivate)
                return file;

            //if admin
            if (isAdmin)
                return file;

            //if owner
            if (viewerMemberId != null && file.FolderCreatedByMemberId == viewerMemberId.Value)
                return file;

            //else null
            return null;

        }


        //GET ARCHIVE: 
        public async Task<MediaArchiveViewModel?> GetFolderArchive(
          int id,
          int requestingMemberId,
           bool isAdmin)
        {
            return await _context.MediaFolders
                .AsNoTracking()
                .Where(f =>
                    f.MediaFolderId == id &&
                    f.isArchived == false &&
                    (
                        isAdmin ||
                        f.CreatedByMemberId == requestingMemberId
                    ))
                .Select(f => new MediaArchiveViewModel
                {
                    MediaFolderId = f.MediaFolderId,
                    folderName = f.FolderName,
                    CreatedAt = f.CreatedDate,
                    IsPrivate = f.IsPrivate,
                    FileCount = f.Files.Count(fi => fi.IsArchived == false && fi.MediaFileStatus == MediaFileStatus.Published),
                    files = f.Files
                        .Where(fi => fi.IsArchived == false)
                        .Select(fi => new FilesArchiveViewModel
                        {
                            fileId = fi.MediaFileId,
                            fileName = fi.FileName,
                            Extension = fi.FileExtension
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }


        public async Task<OperationalResult> ArchiveAsync(int id, int requestingMemberId, bool isAdmin)
        {
            var folder = await _context.MediaFolders
                .Include(f => f.Files)
                .FirstOrDefaultAsync(f => f.MediaFolderId == id);
          
            if (folder == null)
            {
                return new OperationalResult { Success = false, Message = "Folder not found" };
            }

            if (!isAdmin && folder.CreatedByMemberId != requestingMemberId)
                return new OperationalResult { Success = false, Message = "You don't have permission to delete this file." };
            //Archive folder first
            folder.ArchiveDate = DateTime.Now;
            folder.isArchived = true;

            //then archive all its files 
            foreach (var file in folder.Files)
            {

                file.IsArchived = true;
                file.DateArchived = DateTime.Now;
            }
            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = "Folder archived successfully!" };
        }

        public async Task<List<MediaFolderViewModel>> GetAvailableFolderAsync(int exludeFolderId)
        {
            return await _context.MediaFolders.AsNoTracking()
                .Where(f => f.isArchived == false && f.MediaFolderId != exludeFolderId) 
                .Select(f => new MediaFolderViewModel
                {
                    MediaFolderId = f.MediaFolderId,
                    
                    FolderName = f.FolderName,
                    IsPrivate = f.IsPrivate,
                    FileCount = f.Files.Count(fi => fi.IsArchived == false),
                    createByMemberName = f.Member.FirstName + " " + f.Member.LastName ?? ""
                }).ToListAsync();
        }




        public async Task<OperationalResult> MoveFile(int fileid, int newFolderId ,int requestingMemberId, bool isAdmin)
        {
            var file = await _context.MediaFiles
                             .FirstOrDefaultAsync(f => f.MediaFileId == fileid);

            if (file == null) return new OperationalResult { Success = false, Message = "File doesn't exist." };

            var folder = await _context.MediaFolders
                .FirstOrDefaultAsync(f => f.MediaFolderId == newFolderId && f.isArchived == false);
           
            if (file.MediaFolderId == newFolderId)
                return new OperationalResult { Success = false, Message = "File is already inside this folder." };

            if (!isAdmin && file.UploadedByMemberId != requestingMemberId)
                return new OperationalResult { Success = false, Message = "You don't have permission to move this file." };

            if(folder == null)
                return new OperationalResult { Success = false, Message = "Folder doesn't exist" };

            file.MediaFolderId = folder.MediaFolderId;
            await _context.SaveChangesAsync();
            return new OperationalResult { Success = true, Message = $"File successfully moved to {folder.FolderName}" };

        }

        public async Task<FileInformationViewModel?> FileInformation(int fileId)
        {
            return await _context.MediaFiles
                .AsNoTracking()
                .Where(f => f.IsArchived == false && f.MediaFileId == fileId)
                .Select(f => new FileInformationViewModel
                {
                    FileId = f.MediaFileId,
                    FileName = f.FileName,
                    FileExtension = f.FileExtension,
                    Size = f.FileSizeBytes.ToString(),
                    UploadedBy = f.Member.FirstName + " " + f.Member.LastName,
                    CreatedAt = f.UploadDate,
                    Status = f.MediaFileStatus,
                    Visibility = f.Folder.IsPrivate ? "Private" : "Everyone"
                }).FirstOrDefaultAsync();
        }


        public async Task<OperationalResult> ApproveFileAsync(int fileId,int approvedByMemberId)
        {
            var file = await _context.MediaFiles
                .FirstOrDefaultAsync(f => f.MediaFileId == fileId);

            if (file == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "File doesn't exist."
                };
            }
            file.MediaFileStatus = MediaFileStatus.Published;
            file.ApprovedByMemberId = approvedByMemberId;
            file.ApprovalDate = DateTime.Now;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group("Admins")
                .SendAsync("PendingFilesChanged");

            var uploaderUserId = await _context.Users
                .Where(u => u.MemberId == file.UploadedByMemberId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (uploaderUserId != null)
            {
                string notificationUrl;

                if (file.MediaFolderId != null)
                {
                    notificationUrl =
                        $"/Leader/MediaAndDocuments/FolderDetails/{file.MediaFolderId}" +
                        $"?openFileId={file.MediaFileId}";
                }
                else
                {
                    notificationUrl =
                        $"/Leader/MediaAndDocuments?openFileId={file.MediaFileId}";
                }

                await _notificationService.NotifyUserAsync(
                    uploaderUserId,
                    "File Approved!",
                    $"Your file \"{file.FileName}\" has been approved and published.",
                    notificationUrl
                );
            }

            return new OperationalResult
            {
                Success = true,
                Message = "File approved and published."
            };
        }

        public async Task<OperationalResult> RejectFileAsync(int fileId, string reason, int rejectedByMemberId)
        {
            var file = await _context.MediaFiles.FirstOrDefaultAsync(f => f.MediaFileId == fileId);
            if (file == null) return new OperationalResult { Success = false, Message = "File doesn't exist." };
            if (string.IsNullOrWhiteSpace(reason))
                return new OperationalResult { Success = false, Message = "A rejection reason is required." };

            file.MediaFileStatus = MediaFileStatus.Rejected;
            file.ApprovedByMemberId = rejectedByMemberId;   // "who acted on it," approve or reject
            file.ApprovalDate = DateTime.Now;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group("Admins")
            .SendAsync("PendingFilesChanged");

            var uploaderUserId = await _context.Users
        .Where(u => u.MemberId == file.UploadedByMemberId)
        .Select(u => u.Id)
        .FirstOrDefaultAsync();

            Console.WriteLine($"File uploaded by MemberId: {file.UploadedByMemberId}");
            Console.WriteLine($"Rejected by MemberId: {rejectedByMemberId}");
            Console.WriteLine($"Uploader UserId: {uploaderUserId}");

            if (uploaderUserId != null)
            {
                await _notificationService.NotifyUserAsync(
                    uploaderUserId,
                    "File Rejected",
                    $"Your file \"{file.FileName}\" was rejected. Reason: {reason}",
                    "/Leader/MediaAndDocuments"
                );
            }

            return new OperationalResult
            {
                Success = true,
                Message = "File rejected."
            };
        }


        public async Task<PendingFilesViewModel> GetPendingFiles()
        {
            var files = await _context.MediaFiles
                .AsNoTracking()
                .Where(f => f.MediaFileStatus == MediaFileStatus.Pending && f.IsArchived == false)
                .OrderBy(f => f.UploadDate)
                .Select(f => new PendingFileItemViewModel
                {
                    MediaFileId = f.MediaFileId,
                    FileName = f.FileName,
                    FileUrl = f.FileUrl,
                    FileExtension = f.FileExtension,
                    UploadedBy = f.Member.FirstName + " " + f.Member.LastName ?? "N/A",
                    UploadedDate = f.UploadDate,
                   
                })
                .ToListAsync();

            return new PendingFilesViewModel { Files = files };
        }

        public async Task<PendingFileItemViewModel?> GetPendingFilesDetails(int id)
        {
            var files = await _context.MediaFiles
             .AsNoTracking()
             .Where(f => f.MediaFileId == id)
             .Select(f => new PendingFileItemViewModel
             {
                 MediaFileId = f.MediaFileId,
                 FileName = f.FileName,
                 FileUrl = f.FileUrl,
                 FileExtension = f.FileExtension,
                 FileSizeBytes = f.FileSizeBytes,
                 UploadedBy = f.Member.FirstName + " " + f.Member.LastName ?? "N/A",
                 UploadedDate = f.UploadDate,
                 DestinationFolderId = f.MediaFolderId,
                 DestinationFolderName = f.MediaFolderId != null ? f.Folder.FolderName : "Unfiled"
             })
             .FirstOrDefaultAsync();

            return files;
        }

        public async Task<OperationalResult> ChangeVisibility(
             int folderId,
             bool isPrivate)
        {
            var folder = await _context.MediaFolders
                .FirstOrDefaultAsync(f =>
                    f.MediaFolderId == folderId &&
                    f.isArchived == false);

            if (folder == null)
            {
                return new OperationalResult
                {
                    Success = false,
                    Message = "Folder not found."
                };
            }

            folder.IsPrivate = isPrivate;

            await _context.SaveChangesAsync();

            return new OperationalResult
            {
                Success = true,
                Message = isPrivate
                    ? "Folder changed to Private."
                    : "Folder changed to Public."
            };
        }


        public async Task<int> GetPendingFilesCountAsync()
        {
            return await _context.MediaFiles.CountAsync(f => f.IsArchived == false && f.MediaFileStatus == MediaFileStatus.Pending);
        }

       public async Task<FileInformationViewModel?> GetDeleteFile(int id)
        {
            var existingFile = await _context.MediaFiles
                .AsNoTracking()
                .Where(f => f.MediaFileId == id)
                .Select(f => new FileInformationViewModel
                {
                    FileId = f.MediaFileId,
                    FileExtension = f.FileExtension,
                    Size = f.FileSizeBytes.ToString(),
                    UploadedBy = f.Member.FullName ?? ""
                }).FirstOrDefaultAsync();

            return existingFile;
        }


        public async Task<ArchivedMediaDetailsViewModel?> GetArchivedDetailsAsync(int id)
        {
            var folder = await _context.MediaFolders
                .AsNoTracking()
                .Where(f =>
                    f.MediaFolderId == id &&
                    f.isArchived == true)
                .Select(f => new ArchivedMediaDetailsViewModel
                {
                    MediaFolderId = f.MediaFolderId,

                    FolderName = f.FolderName,

                    IsPrivate = f.IsPrivate,

                    CreatedDate = f.CreatedDate,

                    ArchiveDate = f.ArchiveDate,

                    CreatedBy = f.Member != null
                        ? f.Member.FullName
                        : "Unknown",

                    FileCount = _context.MediaFiles
                        .Count(file =>
                            file.MediaFolderId == f.MediaFolderId),

                    Files = _context.MediaFiles
                        .Where(file =>
                            file.MediaFolderId == f.MediaFolderId)
                        .Select(file => new ArchivedMediaFileViewModel
                        {
                            MediaFileId = file.MediaFileId,

                            FileName = file.FileName,

                            FileExtension = file.FileExtension ?? "",

                            FileUrl = file.FileUrl,

                            UploadDate = file.UploadDate,

                            UploadedBy = file.Member != null
                                ? file.Member.FullName
                                : "Unknown"
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return folder;
        }
    }
}
