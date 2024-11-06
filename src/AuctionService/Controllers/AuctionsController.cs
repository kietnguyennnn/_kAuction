using System;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionsService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    public AuctionsController(AuctionDbContext context, IMapper mapper,IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    } 

    [HttpGet]
    //trả về danh sách AuctionDto, ánh xạ từ danh sách Auction sang danh sách AuctionDto thông qua _mapper 
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date) 
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable(); //AsQueryable() chuyển một IEnumerable thành IQueryable. IQueryable là một tập hợp các phương thức mở rộng cho việc truy vấn dữ liệu từ nguồn dữ liệu.
        if(!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0); //đổi date sang datetime và có định dạng chuẩn UTC
            //so sánh ngày cập nhật của phiên đấu giá với ngày được truyền vào và mặc định UpdatedAt > date
        }
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        //projecTo ánh xạ trực tiếp từ các thực thể trong cơ sở dữ liệu sang các đối tượng DTO (Data Transfer Object) mà không cần phải tải toàn bộ thực thể vào bộ nhớ trước.
        //loại các UpdatedAt nhỏ hơn date
    }
    
    [HttpGet("{id}")] //ví dụ api/auctions/id 
    //AuctionDto là kiểu dữ liệu trả về, ánh xạ từ Auction sang AuctionDto thông qua _mapper
    //khi 
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x=>x.Item)
            .FirstOrDefaultAsync(x=>x.Id == id);
        if(auction == null)
        {
            return NotFound();
        }
        return _mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    //phương thức bất đồng bộ CreateAuction, trả về 1 task chứa kết quả ActionResult<AuctionDto> với tham số CreateAuctionDto autionDto 
    //ActionResult<AuctionDto> nghĩa là kết quả khi return sẽ có kiểu dữ liệu là AuctionDto, và có thể trả về các trạng thái khác nhau như BadRequest, CreatedAtAction
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto autionDto)
    {
        var auction = _mapper.Map<Auction>(autionDto);// chuyển từ CreateAuctionDto sang Auction
        //thêm người dùng như người bán 
        auction.Seller ="seller";
        _context.Auctions.Add(auction); //thêm dữ liệu của auction entities vào bảng Auctions trong database

        var newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction)); //publish thông tin phiên đấu giá mới tạo

        var result = await _context.SaveChangesAsync() > 0; //lưu thay đổi vào database

        if(!result) return BadRequest("Failed to create auction");
        return CreatedAtAction(nameof(GetAuctionById), new {id = auction.Id}, newAuction);
        //CreatedAtAction(string actionName, object routeValues, object value);
        //actionName: Tên của hành động (phương thức) sẽ được gọi để lấy tài nguyên mới được tạo. Thường sử dụng nameof để lấy tên của phương thức GetAuction dưới dạng chuỗi
        //routeValues: Các giá trị định tuyến cần thiết để xây dựng URL cho tài nguyên mới được tạo. Thường là một đối tượng ẩn danh chứa các tham số định tuyến.
        //value: Dữ liệu của tài nguyên mới được tạo. Thường là đối tượng DTO của tài nguyên mới được tạo.
        //truyền vào phương thức GetAuction nên phải truyền vào id của auction mới được tạo
    }

    [HttpPut("{id}")]
    //Phương thức này chỉ cập nhật thông tin của phiên đấu giá và không trả về đối tượng AuctionDto sau khi cập nhật 
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
    {
        //tìm phiên đấu giá với ID tương ứng 
        var auction = await _context.Auctions.Include(x=>x.Item).FirstOrDefaultAsync(x=>x.Id == id);

        if(auction == null)
        {
            return NotFound();
        }
        //set người dùng 

        auction.Item.Make = auctionDto.Make ?? auction.Item.Make; //nếu không cập nhật gì thì giữ nguyên giá trị cũ(bên trái) 
        auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
        auction.Item.Year = auctionDto.Year ?? auction.Item.Year;
        auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
        
        var result = await _context.SaveChangesAsync() > 0;// SaveChangesAsync trả về 1 số nguyên đại diện cho số lượng các đối tượng đã bị thay đổi (thêm, sửa, xóa) trong cơ sở dữ liệu.
        if(!result) return BadRequest("Nothing has been updated");
        
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id); //chỉ cần tìm kiếm theo ID(khoá chính) là ra được đối tượng cần xóa
        if(auction == null)
        {
            return NotFound();
        }
        _context.Auctions.Remove(auction);

        await _publishEndpoint.Publish<AuctionDeleted>(new {Id = auction.Id.ToString()});

        var result = await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("Failed to delete auction");
        return Ok();
    }
}
