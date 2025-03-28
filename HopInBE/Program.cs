using HopInBE.DataAccess;
using HopInBE.DataAccess.IDataProvider;
using HopInBE.DataAccess.MongoDB;
using HopInBE.Hubs;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

ConfigurationManager configuration = builder.Configuration;

builder.Services.AddSingleton<IDbSettings>(x => new MongoDbSettings()
{
    ConnectionString = ConfigHelper.AppSetting("DatabaseConnection", "AppSettings"),
    DatabaseName = ConfigHelper.AppSetting("DatabaseName", "AppSettings"),
    DatabaseType = ConfigHelper.AppSetting("DatabaseType", "AppSettings")

});

builder.Services.AddSingleton<IMongoClient, MongoClient>(x =>
{
    var ConnectionString = ConfigHelper.AppSetting("DatabaseConnection", "AppSettings");
    return new MongoClient(ConnectionString);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<GpsHub>("/gpsHub");

app.Run();
