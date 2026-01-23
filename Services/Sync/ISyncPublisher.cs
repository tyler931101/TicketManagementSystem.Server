using System.Threading.Tasks;
using TicketManagementSystem.Server.DTOs.Sync;
namespace TicketManagementSystem.Server.Services.Sync
{
    public interface ISyncPublisher
    {
        Task PublishUserRegisteredAsync(UserRegisteredSyncDto dto);
        Task PublishTicketCreatedAsync(TicketCreatedSyncDto dto);
        Task PublishTicketUpdatedAsync(TicketUpdatedSyncDto dto);
        Task PublishTicketDeletedAsync(TicketDeletedSyncDto dto);
        Task PublishTicketStatusChangedAsync(TicketStatusChangedSyncDto dto);
        Task PublishUserAvatarUpdatedAsync(Guid userId, byte[] data, string mimeType);
    }
}
