using Microsoft.EntityFrameworkCore; 
using CastFlow.Api.Data;          

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
  
    Console.WriteLine("Erreur: Chaîne de connexion 'DefaultConnection' non trouvée dans appsettings.json");
}


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // Utilisation de UseSqlServer

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "CastFlow API", Version = "v1" });
});



var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); 
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CastFlow API v1");
        c.RoutePrefix = string.Empty; 
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error"); 
    app.UseHsts(); 
}

app.UseHttpsRedirection();

app.UseAuthorization();  
app.MapControllers();


app.Run();