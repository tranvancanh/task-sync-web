using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using task_sync_web.Filters;

var builder = WebApplication.CreateBuilder(args);

// Razor �t�@�C���̃R���p�C���L����
// https://learn.microsoft.com/ja-jp/aspnet/core/mvc/views/view-compilation?view=aspnetcore-6.0&tabs=visual-studio
var mvcBuilder = builder.Services.AddRazorPages()
     .AddSessionStateTempDataProvider();
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();

// Program.cs�Ƀt�H�[���o�b�N�F�|���V�[��ǋL����(���ׂẴR���g���[���[��[Authorize]�ɂȂ�)
// https://qiita.com/mkuwan/items/bd5ff882108998d76dca
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
});

// �Z�b�V�����̗��p
builder.Services.AddSession();

// Cookie �ɂ��F�؃X�L�[����ǉ�����
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {

        // ���_�C���N�g���郍�O�C��URL���������ɕς���
        // ~/Account/Login =�� ~/account/login
        options.LoginPath = CookieAuthenticationDefaults.LoginPath.ToString().ToLower();
        //options.Cookie.IsEssential = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
        //options.Cookie.MaxAge = TimeSpan.FromMinutes(1440);
        options.LoginPath = "/Login/index";
        options.SlidingExpiration = false;
    });


builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(MyFilter));
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

// Cookie�F�؎����ɕK�v
app.UseAuthentication();
app.UseAuthorization();

// �Z�b�V�����̗��p
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
