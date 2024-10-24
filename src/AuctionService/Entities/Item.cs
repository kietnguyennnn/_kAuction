using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionService.Entities;

[Table("Items")] //tên bảng trong database, ánh xạ tới bảng tên là items trong csdl 
public class Item
{
       //tên bảng trong database
    public Guid Id { get; set; }
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public string Color { get; set; }
    public int Mileage { get; set; }
    public string ImageUrl { get; set; }
    
    //navigation progerties (thuộc tính điều hướng)
    public Auction auction { get; set; }
    public Guid AuctionId { get; set; }
}
