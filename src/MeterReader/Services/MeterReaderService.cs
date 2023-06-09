﻿using Grpc.Core;
using MeterReader.gRPC;
using static MeterReader.gRPC.MeterReadingService;

namespace MeterReader.Services
{
    public class MeterReaderService : MeterReadingServiceBase
    {
        private readonly IReadingRepository _repository;
        private readonly ILogger<MeterReaderService> _logger;

        public MeterReaderService(IReadingRepository repository,ILogger<MeterReaderService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public override async Task<StatusMessage> AddReading(ReadingPacket request, ServerCallContext context)
        {
            if (request.Successful == ReadingStatus.Success) 
            {
                foreach (var item in request.Readings) 
                {
                    var readingValue = new MeterReading()
                    {
                        CustomerId = item.CustomerId,
                        Value = item.ReadingValue,
                        ReadingDate = item.ReadingTime.ToDateTime(),
                    };

                    _repository.AddEntity(readingValue);
                }

                if(await _repository.SaveAllAsync())
                {
                    return new StatusMessage()
                    {
                        Notes = "Successfully added to database",
                        Status = ReadingStatus.Success
                    };
                }
                else
                {
                    return new StatusMessage()
                    {
                        Notes = "Failed to add to database",
                        Status = ReadingStatus.Failure
                    };
                }
            }
            else
            {
                return new StatusMessage()
                {
                    Notes = "Failed to add to database",
                    Status = ReadingStatus.Failure
                };
            }
        }
    }
}
