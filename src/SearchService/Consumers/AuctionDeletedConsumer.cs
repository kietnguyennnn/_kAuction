using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly IMapper _mapper;

    public AuctionDeletedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("AuctionDeleteConsumer:" + context.Message.Id);
        var item = _mapper.Map<Item>(context.Message);

        var result = await DB.DeleteAsync<Item>(context.Message.Id);
        if (!result.IsAcknowledged) 
            throw new MessageException(typeof(AuctionDeleted), "Problem deleting auction");
    }

}

