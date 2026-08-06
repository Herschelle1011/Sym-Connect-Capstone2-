using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models;
using Sym_Connect__Capstone2_.Seed;
using Sym_Connect__Capstone2_.Services;
using Sym_Connect__Capstone2_.Services.Implementation;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;








var builder = WebApplication.CreateBuilder(args);



// CONNECT TO SQL SERVER
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ADD IDENTITY
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

})
        .AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

//CONFIGURE CLOUDINARY SETTINGS
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

// Register Cloudinary as a singleton service
builder.Services.AddSingleton(serviceProvider =>
{
    var config = builder.Configuration.GetSection("Cloudinary")
                        .Get<CloudinarySettings>();

    var account = new Account(
        config.CloudName,
        config.ApiKey,
        config.ApiSecret
    );

    return new Cloudinary(account) { Api = { Secure = true } };
});





// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<CloudinaryService>(); //for upload photos through cloud
builder.Services.AddScoped<IGroupService, GroupService>(); //for groups
builder.Services.AddScoped<IMemberService, MemberService>(); // for members
builder.Services.AddScoped<IFacilityService, FacilityService>(); // for facility
builder.Services.AddScoped<IEventService, EventService>(); // for evaluation
builder.Services.AddScoped<IPositionService, PositionService>(); // for evaluation


builder.Services.AddScoped<Sym_Connect__Capstone2_.Services.Interfaces.IEvaluationService,
                            Sym_Connect__Capstone2_.Services.Implementation.EvaluationService>();



builder.Services.AddScoped<IPartyListService, PartyListService>(); // for Elections


builder.Services.AddScoped<Sym_Connect__Capstone2_.Services.Interfaces.IElectionService,
                            Sym_Connect__Capstone2_.Services.Implementation.ElectionService>();
//member
builder.Services.AddScoped<Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces.IElectionService,
                            Sym_Connect__Capstone2_.Areas.Member.Services.Implementation.ElectionService>();
builder.Services.AddScoped<Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces.IEvaluationService,
                            Sym_Connect__Capstone2_.Areas.Member.Services.Implementation.EvaluationService>();

builder.Services.AddScoped<Sym_Connect__Capstone2_.Areas.Member.Services.Interfaces.IProfileService,
                            Sym_Connect__Capstone2_.Areas.Member.Services.Implementation.ProfileService>();

builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//api for swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

//api for swagger

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // This generates the webpage layout
}



// SEED DEFAULT ROLES AND USERS
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Dashboard}/{id?}")
    .WithStaticAssets();

app.MapRazorPages(); 

app.Run();
