using System;
using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore; // DbContext
namespace AuctionService.Data;

public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {

    }
    public DbSet<Auction> Auctions { get; set; }
    //không cần khai báo cho item bởi vì nó sẽ tự động tạo ra khi chúng ta tạo bảng Auction vì có navigation properties 
    //lý do là vì khi chúng ta tạo bảng Auction thì nó sẽ tạo ra bảng Item với tên là Items vì chúng ta đã đặt tên bảng trong class Item.cs
    //và nó sẽ tự động tạo ra cột AuctionId trong bảng Items để tham chiếu đến bảng Auction
    //lý do khai báo là auctions vì nó sẽ tạo ra bảng có tên là Auctions trong csdl
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }



}
