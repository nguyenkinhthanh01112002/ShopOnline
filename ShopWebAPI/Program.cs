using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShopWebAPI.Data;
using ShopWebAPI.Interfaces.Auth;
using ShopWebAPI.Interfaces.User;
using ShopWebAPI.Models;
using ShopWebAPI.Services.Auth;
using ShopWebAPI.Services.User;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // This will use the property names as defined in the C# model
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
}); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Shop Online", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


//configure Entity FrameWore Core
var connectionString = builder.Configuration.GetConnectionString("DbContext") ?? throw new InvalidOperationException("Connection string 'DbContext' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

//configure Identity Service
builder.Services.AddIdentity<ApplicationUser,IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    // Thi?t l?p khóa tài kho?n
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // Th?i gian khóa
    options.Lockout.MaxFailedAccessAttempts = 3; // S? l?n th? t?i ?a
    options.Lockout.AllowedForNewUsers = true; // Cho phép khóa tài kho?n m?i
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
// Configure JWT authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            RoleClaimType = ClaimTypes.Role // Ensure the JWT token role claim is recognized correctly
        };
    });
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

//Configuration Authentication Middleware to The Request Pipeline
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
