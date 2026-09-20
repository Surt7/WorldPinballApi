using WorldPinball.Bff.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddBffSecurity(); 
builder.Services.AddHealthChecks(); 

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Security hardening
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});
app.UseSecurityHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AngularDevPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Health check endpoint for Docker / K8s
app.MapHealthChecks("/health");

app.MapReverseProxy();

app.Run();