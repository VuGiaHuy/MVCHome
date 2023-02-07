using App.Data;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddOptions();

builder.Services.AddDbContext<AppDbContext>(options=>{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContextConnectionStrings"));
});

builder.Services.AddSingleton<IEmailSender,SendMailService>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));


builder.Services.AddSingleton<IdentityErrorDescriber,AppIdentityErrorDescriber>();



builder.Services.AddIdentity<AppUser,IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
                
builder.Services.Configure<IdentityOptions> (options => {
    options.Password.RequireDigit = false; 
    options.Password.RequireLowercase = false; 
    options.Password.RequireNonAlphanumeric = false; 
    options.Password.RequireUppercase = false; 
    options.Password.RequiredLength = 3; 
    options.Password.RequiredUniqueChars = 1; 

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes (5); 
    options.Lockout.MaxFailedAccessAttempts = 5; 
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters = 
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
    options.User.RequireUniqueEmail = true; 
    
    options.SignIn.RequireConfirmedEmail = true;           
    options.SignIn.RequireConfirmedPhoneNumber = false; 
});                
builder.Services.AddAuthentication()
                .AddGoogle(options=>{
                    IConfigurationSection  google =  builder.Configuration.GetSection("Authentication:Google");
                    options.ClientId = google["ClientId"];
                    options.ClientSecret = google["ClientSecret"];
                    options.CallbackPath = "/login-google";
                });
builder.Services.AddAuthorization(options=>{
    options.AddPolicy("ViewManageMenu",builder=>{
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);
    });
});











var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
        {
          endpoints.MapControllerRoute(
            name : "areas",
            pattern : "{area:exists}/{controller=Home}/{action=Index}/{id?}"
          );
        });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
