using System;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.SearchService;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app){
        await DB.InitAsync("SearchDb",MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        //tạo chỉ mục cho các trường Make, Model, Color
        await DB.Index<Item>()
            .Key(a => a.Make, KeyType.Text)
            .Key(a => a.Model, KeyType.Text)
            .Key(a => a.Color, KeyType.Text)
            .CreateAsync();
        
        var count = await DB.CountAsync<Item>();

        using var scrope = app.Services.CreateScope();//tạo một phạm vi dịch vụ để giải phóng tài nguyên sau khi hoàn thành công việc
        var httpClient = scrope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        
        var items = await httpClient.GetItemForSearchDb();

        Console.WriteLine(items.Count + " items found from auction service.");
        
        if(items.Count > 0){
            await DB.SaveAsync(items);
        }
    }
}
