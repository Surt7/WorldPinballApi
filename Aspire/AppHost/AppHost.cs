var builder = DistributedApplication.CreateBuilder(args);


var api = builder.AddProject<Projects.WorldPinball_Api>("api");
var bff = builder.AddProject<Projects.WorldPinball_Bff>("bff")
    .WithReference(api);



builder.Build().Run();