using Common.MessageQueue.Services;
using OnaxTools.Dto.Http;
using Common.MessageQueue.Services;
using System.Runtime.CompilerServices;

namespace Common.MessageQueue.Interfaces
{
    public interface IMessageQueueService
    {
        Task<GenResponse<string>> FetchAndProcessMsgFromQueue(FetchQueueProps props, CancellationToken ct = default, [CallerMemberName] string caller = "");
        Task<GenResponse<bool>> RabbitMqPublish<T>(T data, MqPublisherProps props, CancellationToken ct = default, [CallerMemberName] string caller = "");
    }
}