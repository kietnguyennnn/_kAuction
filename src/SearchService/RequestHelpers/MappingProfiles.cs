using System;
using AutoMapper;
using Contracts;
using SearchService.Consumers;
using SearchService.Entities;

namespace SearchService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<AuctionCreated, Item>();
        CreateMap<AuctionUpdated, Item>();
    }
}
