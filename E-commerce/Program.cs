using System.Text;
using E_commerce.Data;
using E_commerce.Identity;
using E_commerce.Middleware;
using E_commerce.Repositories;
using E_commerce.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter JWT token only without 'Bearer ' prefix (Swagger adds it automatically)."
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<StoreContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("No connection string was found"));
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<E_commerce.Services.Interfaces.IAuthService, E_commerce.Services.AuthService>();
builder.Services.AddScoped<E_commerce.Services.Interfaces.IImageService, E_commerce.Services.ImageService>();
builder.Services.AddScoped<E_commerce.Services.Interfaces.IProductService, E_commerce.Services.ProductService>();
builder.Services.AddScoped<E_commerce.Services.Interfaces.ICategoryService, E_commerce.Services.CategoryService>();
builder.Services.AddScoped<E_commerce.Services.Interfaces.IShipperService, E_commerce.Services.ShipperService>();
builder.Services.AddScoped<E_commerce.Services.Interfaces.IOrderStatusService, E_commerce.Services.OrderStatusService>();
builder.Services.AddScoped<E_commerce.Services.Interfaces.ICustomerService, E_commerce.Services.CustomerService>();
builder.Services.AddScoped<E_commerce.Services.Interfaces.IOrderService, E_commerce.Services.OrderService>();
builder.Services.AddScoped<E_commerce.Services.Interfaces.IPaymentService, E_commerce.Services.PaymentService>();


// ============ Identity Configuration ============
// Registers UserManager<ApplicationUser> and RoleManager<IdentityRole>
// bound to StoreContext to manage AspNetUsers and AspNetRoles tables.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Basic password complexity options
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<StoreContext>()
    .AddDefaultTokenProviders();

// ============ JWT Authentication ============
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing from configuration");

builder.Services
    .AddAuthentication(options =>
    {
        // Set JWT Bearer as default authentication scheme
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
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot (e.g. product images stored in wwwroot/images/products)
app.UseStaticFiles();

// Middleware execution order is critical: Authentication must precede Authorization
// Authentication identifies who the caller is via the JWT token
// Authorization verifies permissions/roles for the identified user
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============ Seed Roles ============
// Ensure essential application roles exist in the database upon startup.
// Creates missing default roles to prevent crashes during user assignment.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Customer", "Admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.Run();