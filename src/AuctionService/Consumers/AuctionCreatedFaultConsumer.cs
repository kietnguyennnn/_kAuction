using System;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine("AuctionCreatedFaultConsumer:" + context.Message.Message);

        var e = context.Message.Exceptions.First();

        if (e.ExceptionType =="System.ArgumentException")
        {
            context.Message.Message.Model ="vinfast"; // thay đổi khi người dùng tạo 1 model là vin thành vinfast
            await context.Publish(context.Message.Message); // dùng context.Message.Message để truy cập vào message gốc tức là của AuctionCreated thay vì message của Fault
        }
        else
        {
            Console.WriteLine("It's not an ArgumentException");
        } 
    }
}
