using System;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.RequestHelpers;
using ZstdSharp.Unsafe;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearhController : ControllerBase  
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams searchParams) //pagenumber là trang tìm kiếm, pagesize là số lượng item trên mỗi trang
    {
        //Tạo một đối tượng truy vấn tìm kiếm phân trang cho kiểu Item bằng cách gọi phương thức PagedSearch<Item>() của lớp DB.
        var query = DB.PagedSearch<Item, Item>();  //khởi tạo với DB.PagedSearch<Item, Item>() để đảm bảo rằng kiểu dữ liệu của query tương thích với kết quả của các biểu thức trong switch.
        //Thiết lập sắp xếp kết quả theo thuộc tính Make của Item theo thứ tự tăng dần.

        if (!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }
        
        query = searchParams.OrderBy switch{
            "make" => query.Sort(x => x.Ascending(a => a.Make)),
            "new"  => query.Sort(x => x.Descending(a => a.CreatedAt)),
             _     => query.Sort(x => x.Ascending(a => a.AuctionEnd)) //mặc định các phiên đấu giá sắp kết thúc sẽ được hiển thị trước.
        };

        query = searchParams.FilterBy switch{
            "finished"   => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
            "endingsoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(24) && x.AuctionEnd > DateTime.UtcNow),
            _            => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
        };

        if (!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }

        //Thiết lập số trang và kích thước trang cho truy vấn.
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);
        
        //Thực hiện truy vấn bất đồng bộ và chờ kết quả trả về. Kết quả là một đối tượng chứa các thuộc tính Results, PageCount, và TotalCount.
        var result = await query.ExecuteAsync(); 
        return Ok(new
                {
                    results = result.Results,
                    pageCount = result.PageCount,
                    totalCount = result.TotalCount,
                });
            
    }               
}
