using AuctionService.Data;
using AuctionService.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
    //đăng ký dịch vụ AuctionDbContext
builder.Services.AddDbContext<AuctionDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));  //kết nối đến database
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //vì sao là AppDomain.CurrentDomain.GetAssemblies()? vì nó sẽ tìm kiếm tất cả các assembly trong project hiện tại
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<AuctionDbContext>(p=>
    {
        p.QueryDelay = TimeSpan.FromSeconds(5);
        p.UsePostgres();
        p.UseBusOutbox();
    });
    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

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
