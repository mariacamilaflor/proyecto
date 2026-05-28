using EduDirectory3.Data;
using EduDirectory3.Models;
using EduDirectory3.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// BASE DE DATOS
// =============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddHttpClient<IaService>();
builder.Services.AddScoped<NlpService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// =============================================
// IDENTITY — solo uno, con ApplicationUser y roles
// =============================================
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        var returnUrl = context.Request.Path + context.Request.QueryString;
        var mensaje = "Para usar esta funcionalidad debes iniciar sesión.";

        context.Response.Redirect($"/Identity/Account/Login?mensaje={Uri.EscapeDataString(mensaje)}&returnUrl={Uri.EscapeDataString(returnUrl)}");
        return Task.CompletedTask;
    };

    options.Events.OnSignedIn = async context =>
    {
        var userManager = context.HttpContext.RequestServices
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.GetUserAsync(context.HttpContext.User);
        if (user != null)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
                context.Response.Redirect("/Admin/Dashboard/Index");
            else if (roles.Contains("Institucion"))
                context.Response.Redirect("/Panel/Dashboard/Index");
        }
        
    };
});
// =============================================
// MVC + SESIÓN
// =============================================
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

// =============================================
// PIPELINE
// =============================================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    // Si el usuario está autenticado y está en la página principal
    if (context.User.Identity?.IsAuthenticated == true &&
        context.Request.Path == "/")
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Response.Redirect("/Admin/Dashboard/Index");
            return;
        }

        if (context.User.IsInRole("Institucion"))
        {
            context.Response.Redirect("/Panel/Dashboard/Index");
            return;
        }
    }
    await next();
});
// =============================================
// RUTAS POR ÁREA
// =============================================
// 1. Primero las áreas
app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "Panel",       
    areaName: "Panel",
    pattern: "Panel/{controller=Dashboard}/{action=Index}/{id?}");

// 2. Al final el default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// =============================================
// SEEDER — roles y admin inicial
// =============================================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Institucion", "Usuario" };
    foreach (var rol in roles)
        if (!await roleManager.RoleExistsAsync(rol))
            await roleManager.CreateAsync(new IdentityRole(rol));
    /*
     mariacf@gmail.com Ma1234* institucion
     mariaev@gmail.com Ma1234* institucion
     anad@gmail.com An1234* usuario
     adamd@gmail.com Ad1234* usuario
     admin@gmail.com Ad1234* admin
     */
}

app.Run();