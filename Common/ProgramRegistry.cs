using Microsoft.Extensions.Configuration;
using OnaxTools.Dto.MessageQueue;

namespace Common
{
    public class ProgramRegistry
    {
        private readonly IConfigurationRoot _config;
        public ProgramRegistry()
        {
            if (_config == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false);
                _config = builder.Build();
            }
        }
        public QueueSettings QueueSettings()
        {
            QueueSettings objResp = new()
            {
                QueueConfig = new QueueConfig
                {
                    QueueName = _config.GetSection("QueueSettings").GetSection("QueueConfig").GetSection("QueueName").Value,
                    QueueConString = _config.GetSection("QueueSettings").GetSection("QueueConfig").GetSection("QueueConString").Value
                }
            };
            return objResp;
        }

    }
}
