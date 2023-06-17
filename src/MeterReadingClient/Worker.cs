using Grpc.Net.Client;
using MeterReader.gRPC;
using System.Diagnostics.Contracts;
using static MeterReader.gRPC.MeterReadingService;

namespace MeterReadingClient
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ReadingGenerator _generator;
        private readonly int _customerId;
        private readonly string _serviceUrl;
        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger,ReadingGenerator generator, IConfiguration config)
        {
            _logger = logger;
            _generator = generator;
            _customerId = config.GetValue<int>("CustomerId");
            _serviceUrl = config.GetValue<string>("ServiceUrl");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var channel = GrpcChannel.ForAddress(_serviceUrl);
                var client = new MeterReadingServiceClient(channel);

                var packet = new ReadingPacket()
                {
                    Successful = ReadingStatus.Success
                };

                for (int i = 0; i < 5; ++i)
                {
                    var reading = await _generator.GenerateAsync(_customerId);
                    packet.Readings.Add(reading);
                }

                var status = client.AddReading(packet);
                if(status.Status == ReadingStatus.Success)
                {
                    _logger.LogInformation("Successfully called GRPC");
                }
                else
                {
                    _logger.LogError("Failed to call GRPC");
                }
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}