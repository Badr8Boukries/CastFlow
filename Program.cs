using Microsoft.EntityFrameworkCore;
using CastFlow.Api.Data;
using CastFlow.Api.Repository;
using CastFlow.Api.Services;
using CastFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;
using System.IO; 
using System.Collections.Generic; 
using Microsoft.OpenApi.Models; 
using Microsoft.AspNetCore.Http; 

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("ERREUR: Chaîne de connexion 'DefaultConnection' non trouvée.");
    // Gérer l'erreur ou throw ici si la BDD est indispensable au démarrage
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString!));

builder.Services.AddControllers();

// Enregistrement AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Enregistrement Repositories
builder.Services.AddScoped<IUserTalentRepository, UserTalentRepository>();
builder.Services.AddScoped<IUserAdminRepository, UserAdminRepository>();
builder.Services.AddScoped<IProjetRepository, ProjetRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICandidatureRepository, CandidatureRepository>();
// Enregistrer AdminInvitationTokenRepository si tu en crées un séparé, sinon DbContext suffit pour lui

// Enregistrement Services Métier
builder.Services.AddScoped<ITalentService, TalentService>();
builder.Services.AddScoped<IAdminManagementService, AdminManagementService>();
builder.Services.AddScoped<IProjetService, ProjetService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICandidatureService, CandidatureService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Enregistrement Service de Stockage Fichiers
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

// Enregistrement pour accéder à HttpContext (pour construire les URLs complètes)
builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CastFlow API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Entrez 'Bearer ' suivi de votre token JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement() {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
          Scheme = "oauth2", Name = "Bearer", In = ParameterLocation.Header, }, new List<string>() }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["Urls:FrontendBaseUrl"] ?? "http://localhost:5173") // Utilise config
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "CastFlow API v1"); c.RoutePrefix = string.Empty; });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error"); app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
if (!Directory.Exists(uploadsPath)) { Directory.CreateDirectory(uploadsPath); }

app.UseStaticFiles(); // Pour wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseCors("AllowFrontend"); // Appliquer la policy CORS
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();