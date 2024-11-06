using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine("AuctionCreatedConsumer:" +  context.Message.Id);
        var item = _mapper.Map<Item>(context.Message);
        
        if (item.Model == "vin") throw new ArgumentException("vin is not allowed");

        await item.SaveAsync();
    }
}


