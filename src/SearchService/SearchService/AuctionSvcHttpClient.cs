using System;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.SearchService;

public class AuctionSvcHttpClient
{
	private readonly HttpClient _httpClient;
	private readonly IConfiguration _config;

	public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
	{
		_httpClient = httpClient;
		_config = config;
	}

	public async Task<List<Item>> GetItemForSearchDb()
	{
		var lastUpdated = await DB.Find<Item, string>() //chuyển các mục từ kiểu Item sang kiểu string
			.Sort(x=>x.Descending(x=>x.UpdatedAt))
			.Project(x=>x.UpdatedAt.ToString()) 
			.ExecuteFirstAsync(); 
		
		 return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] + "/api/auctions?date=" +lastUpdated);
		//gửi yêu cầu GET đến dịch vụ đấu giá và trả về danh sách các mục được cập nhật sau thời gian cuối cùng.
	}
}
