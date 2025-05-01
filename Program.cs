using Microsoft.EntityFrameworkCore;
using CastFlow.Api.Data;
using CastFlow.Api.Repository;          
using CastFlow.Api.Services;             
using CastFlow.Api.Services.Interfaces; 

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Erreur: Chaîne de connexion 'DefaultConnection' non trouvée dans appsettings.json");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "CastFlow API", Version = "v1" });
});

builder.Services.AddScoped<IUserTalentRepository, UserTalentRepository>();
builder.Services.AddScoped<IUserAdminRepository, UserAdminRepository>();

builder.Services.AddScoped<ITalentService, TalentService>(); 


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