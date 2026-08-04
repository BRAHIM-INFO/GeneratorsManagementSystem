using GeneratorsManagementSystem.Data;
using GeneratorsManagementSystem.Hubs;
using GeneratorsManagementSystem.Models.Identity;
using GeneratorsManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// ═══ دعم رفع الملفات الكبيرة (10 MB) ═══
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 15 * 1024 * 1024; // 15 MB للاحتياط
    options.ValueLengthLimit = 15 * 1024 * 1024;
    options.MemoryBufferThreshold = 15 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 15 * 1024 * 1024;
});

// ─── SignalR ───
builder.Services.AddSignalR();

// ─── Services ───
builder.Services.AddScoped<ISubscriberService, SubscriberService>();
builder.Services.AddScoped<IGeneratorService, GeneratorService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
builder.Services.AddScoped<IFuelService, FuelService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IGeographyService, GeographyService>();
// ─── الخدمات الحالية ───
builder.Services.AddScoped<ISubscriberService, SubscriberService>();
builder.Services.AddScoped<IGeneratorService, GeneratorService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddScoped<IAccountingService, AccountingService>();

// 🆕 أضف هذا:
builder.Services.AddScoped<IAuditService, AuditService>();

// 🆕 أضف هذا السطر:
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
// 🆕 أضف هذا:
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// ─── الخدمات الجديدة (سنبنيها في الرسائل التالية) ───
//builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
//builder.Services.AddScoped<IInvoiceService, InvoiceService>();
//builder.Services.AddScoped<IPaymentService, PaymentService>();
//builder.Services.AddScoped<IExpenseService, ExpenseService>();
//builder.Services.AddScoped<IAccountingService, AccountingService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
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

// ─── SignalR Hub ───
app.MapHub<GeneratorsHub>("/generatorsHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

using (var scope = app.Services.CreateScope())
{
    try { await SeedData.InitializeAsync(scope.ServiceProvider); }
    catch (Exception ex)
    {
        var log = scope.ServiceProvider
            .GetRequiredService<ILogger<Program>>();
        log.LogError(ex, "خطأ في تهيئة البيانات");
    }
}

app.Run(); 