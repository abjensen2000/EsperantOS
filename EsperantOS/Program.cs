using EsperantOS.Data;
using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.BusinessLogic;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<EsperantOSContext>();

// Register UnitOfWork and Business Logic Layer
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<VagtBLL>();
builder.Services.AddScoped<MedarbejderBLL>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EsperantOSContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();

    if (!context.Vagter.Any())
    {
        var m1 = new EsperantOS.Models.Medarbejder { Name = "Simon", Bestyrelsesmedlem = false };
        var m2 = new EsperantOS.Models.Medarbejder { Name = "Mads", Bestyrelsesmedlem = true };
        var m3 = new EsperantOS.Models.Medarbejder { Name = "Oliver", Bestyrelsesmedlem = false };
        var m4 = new EsperantOS.Models.Medarbejder { Name = "Emma", Bestyrelsesmedlem = true };
        
        context.Medarbejdere.AddRange(m1, m2, m3, m4);
        context.SaveChanges();

        // Næste 10 fredage
        var dates = new System.Collections.Generic.List<DateTime>();
        var date = DateTime.Today;
        while (date.DayOfWeek != DayOfWeek.Friday)
        {
            date = date.AddDays(1);
        }
        
        var rand = new Random();
        var allM = new System.Collections.Generic.List<EsperantOS.Models.Medarbejder> { m1, m2, m3, m4 };

        for (int i = 0; i < 10; i++)
        {
            var fDate = date.AddDays(i * 7).AddHours(16); // fredag kl 16
            
            // Opret 3 vagt-tider pr. fredag, og giv dem 1-2 medarbejdere hver (hver får sin egen vagt)
            for (int k = 0; k < 3; k++)
            {
                var vagtDate = fDate.AddHours(k * 4);
                
                int numM = rand.Next(1, 3); // 1 eller 2 medarbejdere
                var shuffled = allM.OrderBy(x => rand.Next()).Take(numM).ToList();

                foreach (var medarbejder in shuffled)
                {
                    var v = new EsperantOS.Models.Vagt
                    {
                        Dato = vagtDate,
                        Ædru = (k == 2), // Kun sidste vagt (lukke) kræver ædru
                        Frigivet = rand.NextDouble() > 0.7,
                        Medarbejdere = new System.Collections.Generic.List<EsperantOS.Models.Medarbejder> { medarbejder }
                    };

                    context.Vagter.Add(v);
                }
            }
        }
        
        context.SaveChanges();
    }
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
