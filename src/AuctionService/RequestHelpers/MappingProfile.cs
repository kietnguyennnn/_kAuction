using System;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Auction,AuctionDto>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionDto>();
        // ánh xạ từ CreateAuctionDto sang Auction vì dữ liệu của CreateAuctionDto có trước rồi mới ánh xạ sang Auction
        CreateMap<CreateAuctionDto, Auction>()
            .ForMember(d=>d.Item, opt=>opt.MapFrom(s=>s)); // cấu hình AutoMapper để ánh xạ từ CreateAuctionDto sang Auction, và ánh xạ toàn bộ đối tượng CreateAuctionDto sang thuộc tính Item của Auction.
            //d => d.Item: Đây là biểu thức lambda chỉ định thuộc tính Item của đối tượng đích (Auction) mà bạn muốn tùy chỉnh ánh xạ.
            //opt Là một đối tượng của kiểu IMemberConfigurationExpression, được sử dụng để cấu hình cách ánh xạ thuộc tính của đối tượng đích (Auction)
            //opt => opt.MapFrom(s => s): Đây là biểu thức lambda chỉ định cách ánh xạ từ đối tượng nguồn (CreateAuctionDto) sang thuộc tính Item của đối tượng đích (Auction).
            //opt.MapFrom(s => s): Phương thức MapFrom được sử dụng để chỉ định rằng thuộc tính Item của đối tượng đích (Auction) sẽ được ánh xạ từ toàn bộ đối tượng nguồn (CreateAuctionDto). Điều này có nghĩa là toàn bộ đối tượng CreateAuctionDto sẽ được ánh xạ sang thuộc tính Item của Auction.
        CreateMap<CreateAuctionDto, Item>();
        CreateMap<AuctionDto, AuctionCreated>();
        CreateMap<Auction, AuctionUpdated>().IncludeMembers(x => x.Item);
        CreateMap<Item, AuctionUpdated>();
    }
}
