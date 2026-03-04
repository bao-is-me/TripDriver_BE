using TripDriver_BE.Api.Auth;
using TripDriver_BE.Api.Extensions;
using TripDriver_BE.Api.Middlewares;
using TripDriver_BE.Repo.Data;
using TripDriver_BE.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AvailabilityService>();
builder.Services.AddScoped<BookingWorkflowService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<SeedService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSwaggerWithJwt();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => JwtOptions.Configure(options, builder.Configuration));

builder.Services.AddAuthorization();

builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed data (admin/owner/customer + sample cars) if DB is empty
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seeder.SeedAsync();
}

app.Run();
