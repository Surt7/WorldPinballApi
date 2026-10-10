using WorldPinball.Logistics.Api.Endpoints;
using WorldPinball.Logistics.Application;
using WorldPinball.Logistics.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddLogisticsApplication();
builder.Services.AddLogisticsInfrastructure(builder.Configuration);


var app = builder.Build();
app.MapDefaultEndpoints();
app.MapShipmentEndpoints();

app.Run();
