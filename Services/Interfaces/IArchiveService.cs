using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.ViewModels;
using Sym_Connect__Capstone2_.ViewModels.Archive;

namespace Sym_Connect__Capstone2_.Services.Interfaces
{
    public interface IArchiveService
    {
        Task<List<ArchiveItemViewModel>> GetAllArchivedAsync();
        Task<OperationalResult> RestoreAsync(int id, ArchiveCategory category);
        Task<OperationalResult> HardDeleteAsync(int id, ArchiveCategory category);
    }
}
