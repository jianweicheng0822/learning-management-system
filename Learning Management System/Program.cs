using Amazon.S3;
using LMS.Data;
using LMS.Middleware;
using LMS.Models;
using LMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting LMS application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    // ----- Database -----
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    // ----- Identity -----
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // ----- Authentication (Cookie) -----
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(24);
        });

    builder.Services.AddAuthorization();

    // ----- AWS S3 -----
    builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("AWS:S3"));
    builder.Services.AddSingleton<IAmazonS3>(sp =>
    {
        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(
                builder.Configuration["AWS:S3:Region"] ?? "us-east-1")
        };

        var accessKey = builder.Configuration["AWS:AccessKeyId"];
        var secretKey = builder.Configuration["AWS:SecretAccessKey"];

        if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            return new AmazonS3Client(accessKey, secretKey, config);

        return new AmazonS3Client(config);
    });
    builder.Services.AddScoped<IFileStorageService, S3FileStorageService>();

    // ----- Upload size limits -----
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 25 * 1024 * 1024; // 25 MB
    });
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 30 * 1024 * 1024; // 30 MB
    });

    // ----- Application Services -----
    builder.Services.AddScoped<ICourseService, CourseService>();
    builder.Services.AddScoped<IAssignmentService, AssignmentService>();
    builder.Services.AddScoped<ISubmissionService, SubmissionService>();
    builder.Services.AddScoped<IGradeService, GradeService>();

    // ----- MVC -----
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // ----- Middleware Pipeline -----
    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // ----- Database Migration & Seeding -----
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();

        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = ["Admin", "Instructor", "Student"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (builder.Configuration["SEED_DEMO_DATA"] == "true")
        {
            await SeedData.InitializeAsync(services);
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
