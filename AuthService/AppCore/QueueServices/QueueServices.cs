using AuthService.AppCore.Interfaces;
using AuthService.Domain.Dtos;
using Common.Enums;
using OnaxTools.Dto.Http;
using Common.MessageQueue.Interfaces;
using Common.MessageQueue.Services;
using System.Text.Json;

namespace AuthService.AppCore.QueueServices
{
    public class QueueServices : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QueueServices> _logger;
        private readonly IConfiguration _configuration;

        public QueueServices(IServiceProvider serviceProvider, ILogger<QueueServices> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<GenResponse<bool>> RabbitMqProcessRequests(CancellationToken ct = default)
        {
            GenResponse<bool> objResp = new();
            var msgQueue = _configuration.GetSection("MsgQueue");
            var msqSecret = msgQueue.GetSection("IsAutoAcknowledged").Value;

            FetchQueueProps MqServiceOptions = new()
            {
                ClientProvidedName = ClientProvidedNameEnum.AuthSvc,
                ExchangeName = ExchangeNameEnum.AuthSvc_Exchange,
                QueueName = QueueNameOrRouteKeyEnums.Service.ToString(),
                RoutingKey = QueueNameOrRouteKeyEnums.ServiceKey.ToString(),
                IsAutoAcknowledged = Convert.ToBoolean(msqSecret),
            };

            try
            {
                var scope = _serviceProvider.CreateScope();
                var msgQueueSvc = scope.ServiceProvider.GetRequiredService<IMessageQueueService>();
                var authRepoSvc = scope.ServiceProvider.GetRequiredService<IAuthRepository>();

                GenResponse<string> emailObj = await msgQueueSvc.FetchAndProcessMsgFromQueue(MqServiceOptions, ct: ct);
                objResp.IsSuccess = emailObj.IsSuccess; objResp.Error = emailObj.Error; objResp.Message = emailObj.Message;

                if (emailObj != null && emailObj.IsSuccess)
                {
                    ChangePasswordDto requestModel = JsonSerializer.Deserialize<ChangePasswordDto>(emailObj.Result);
                    if (requestModel != null)
                    {
                        var objResult = await authRepoSvc.ChangePassword(requestModel);
                        if (objResult.IsSuccess == false)
                        {
                            MqPublisherProps MqSendServiceOptions = new()
                            {
                                ClientProvidedName = ClientProvidedNameEnum.AuthSvc,
                                ExchangeName = ExchangeNameEnum.AuthSvc_Exchange,
                                QueueName = QueueNameOrRouteKeyEnums.Service.ToString(),
                                RoutingKey = $"{QueueNameOrRouteKeyEnums.ServiceKey.ToString()}"
                            };
                            _ = await msgQueueSvc.RabbitMqPublish<ChangePasswordDto>(requestModel, MqSendServiceOptions);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return GenResponse<bool>.Failed(ex.Message);
            }
            return objResp;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var msgQueue = _configuration.GetSection("MsgQueue");
            var msqSecret = msgQueue.GetSection("DelayInMilliseconds").Value;
            while (!stoppingToken.IsCancellationRequested)
            {
                System.Diagnostics.Stopwatch stopwatch = new();
                stopwatch.Start();

                await this.RabbitMqProcessRequests(stoppingToken);

                Thread.Sleep(Convert.ToInt32(msqSecret));

                stopwatch.Stop();
                Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
