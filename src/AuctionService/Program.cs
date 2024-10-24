using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
    //đăng ký dịch vụ AuctionDbContext
builder.Services.AddDbContext<AuctionDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));  //kết nối đến database
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //vì sao là AppDomain.CurrentDomain.GetAssemblies()? vì nó sẽ tìm kiếm tất cả các assembly trong project hiện tại

var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseAuthorization();

app.MapControllers();

try 
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
app.Run();
