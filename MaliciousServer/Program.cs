WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(c=>c.AddDefaultPolicy(p=>p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
WebApplication app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors();
app.MapPost("api/data", async (HttpRequest r) =>
{
    using StreamReader streamReader = new(r.Body);
    string body = await streamReader.ReadToEndAsync();
    Console.WriteLine(body);
    return Results.NoContent();
});

app.Run();
