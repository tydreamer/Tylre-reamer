using System.Text;
using FoodDelivery.API.Data;
using FoodDelivery.API.Hubs;
using FoodDelivery.API.Options;
using FoodDelivery.API.Services.Images;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
        options.UseInMemoryDatabase("IntegrationTests");
    else
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<GoogleAuthService>();
builder.Services.AddScoped<OrderPricingService>();
builder.Services.Configure<ImageUploadOptions>(builder.Configuration.GetSection(ImageUploadOptions.SectionName));
builder.Services.AddSingleton<IImageStorageService, LocalImageStorageService>();

var googleClientId = builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Google:ClientSecret"];
var googleConfigured = !string.IsNullOrWhiteSpace(googleClientId)
    && !string.IsNullOrWhiteSpace(googleClientSecret)
    && !googleClientId.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);

var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

if (googleConfigured)
{
    var webBaseUrl = builder.Configuration["App:WebBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5196";

    authBuilder
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, cookie =>
        {
            cookie.Cookie.HttpOnly = true;
            cookie.Cookie.SameSite = SameSiteMode.Lax;
            cookie.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = googleClientId!;
            options.ClientSecret = googleClientSecret!;
            options.CallbackPath = "/api/auth/google/callback";
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Events.OnRemoteFailure = context =>
            {
                context.Response.Redirect($"{webBaseUrl}/auth/google-callback?error=google_failed");
                context.HandleResponse();
                return Task.CompletedTask;
            };
        });
}

var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]!
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials()));

builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "text/plain";

    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    if (feature?.Error is not null)
        logger.LogError(feature.Error, "Unhandled exception.");

    await context.Response.WriteAsync("An unexpected error occurred.");
}));

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseHttpsRedirection();

var uploadOptions = app.Configuration.GetSection(ImageUploadOptions.SectionName).Get<ImageUploadOptions>()
    ?? new ImageUploadOptions();
var uploadsRoot = Path.IsPathRooted(uploadOptions.RootPath)
    ? uploadOptions.RootPath
    : Path.Combine(app.Environment.ContentRootPath, uploadOptions.RootPath);
Directory.CreateDirectory(uploadsRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = uploadOptions.PublicPathPrefix
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

app.Run();

public partial class Program;
