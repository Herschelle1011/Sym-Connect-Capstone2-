using Sym_Connect__Capstone2_.Models.MediaAndDocuments;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.MediaAndDocuments;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IMediaService
    {
        Task<List<MediaFolderViewModel>> GetAllFoldersAsync(int viewerMemberId, bool isAdmin);
        Task<MediaFile> GetFileByIdAsync(int id);
        Task<List<MediaFileViewModel>> RecentFiles(int viewerMemberId, bool isAdmin);
        Task<OperationalResult> CreateFolderAsync(string folderName, int createdByMemberId, bool isPrivate);
        Task<OperationalResult> DeleteFolderAsync(int folderId, int requestingMemberId, bool isAdmin);
        Task<bool> CanViewFolderAsync(int folderId, int viewerMemberId, bool isAdmin);

        Task<List<MediaFileViewModel>> GetFilesInFolderAsync(int? folderId);
        Task<OperationalResult> UploadFilesAsync(int? folderId, List<IFormFile> files, int uploadByMemberId, bool IsAdmin);

        Task<OperationalResult> RenameFolderAsync(int id, string newname);
        Task<MediaFolder> GetFolderByIdAsync(int id);

        Task<OperationalResult> RenameFileAsync(int id, string newname);

        Task<MediaFileViewModel?> GetFileForDownloadAsync(int mediaFileId, int? viewerMemberId, bool isAdmin);

        Task<OperationalResult> DeleteFileAsync(int mediaFileId, int requestingMemberId, bool isAdmin);

        Task<MediaArchiveViewModel?> GetFolderArchive(
         int id,
         int requestingMemberId,
          bool isAdmin);

        Task<OperationalResult> ArchiveAsync(int id, int requestingMemberId, bool isAdmin);

        Task<FileInformationViewModel?> FileInformation(int fileId);

        Task<List<MediaFolderViewModel>> GetAvailableFolderAsync(int exludeFolderId);

        Task<OperationalResult> MoveFile(int fileid, int newFolderId, int requestingMemberId, bool isAdmin);

        Task<OperationalResult> ApproveFileAsync(int fileId, int approvedByMemberId);

        Task<OperationalResult> RejectFileAsync(int fileId, string reason, int rejectedByMemberId);

        Task<PendingFilesViewModel> GetPendingFiles();
        Task<PendingFileItemViewModel?> GetPendingFilesDetails(int id);

        Task<OperationalResult> ChangeVisibility(
                     int folderId,
                     bool isPrivate);

        Task<int> GetPendingFilesCountAsync();

        Task<FileInformationViewModel?> GetDeleteFile(int id); 
     }
}
