using Microsoft.EntityFrameworkCore;
using CastFlow.Api.Data;
using CastFlow.Api.Repository;
using CastFlow.Api.Services;
using CastFlow.Api.Services.Interfaces;
// --- AJOUTER CES USING ---
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
// --- FIN AJOUT USING ---

var builder = WebApplication.CreateBuilder(args);

// --- Configuration des Services ---

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Erreur: Chaîne de connexion 'DefaultConnection' non trouvée.");
    // throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "CastFlow API", Version = "v1" });
    // Configurer Swagger pour utiliser l'authentification Bearer (optionnel mais utile)
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrez 'Bearer ' suivi de votre token JWT",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// --- >>> AJOUTER CE BLOC DE CONFIGURATION JWT <<< ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
        // ClockSkew = TimeSpan.Zero // Optionnel: être strict sur l'expiration
    };
});
// --- >>> FIN DU BLOC AJOUTÉ <<< ---


// --- Enregistrement des Repositories et Services ---
builder.Services.AddScoped<IUserTalentRepository, UserTalentRepository>();
builder.Services.AddScoped<IUserAdminRepository, UserAdminRepository>();
builder.Services.AddScoped<ITalentService, TalentService>();
// ... autres services/repositories ...


var app = builder.Build();

// --- Configuration du Pipeline HTTP ---

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

// --- >>> AJOUTER app.UseAuthentication() ICI, AVANT UseAuthorization <<< ---
app.UseAuthentication();
// --- >>> FIN AJOUT <<< ---

app.UseAuthorization(); // Doit venir APRÈS UseAuthentication

app.MapControllers();

app.Run();