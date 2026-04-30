using EsperantOS.DataAccess.Context;
using EsperantOS.DataAccess.UnitOfWork;
using EsperantOS.BusinessLogic;
using Microsoft.AspNetCore.Authentication.Cookies;

// ──────────────────────────────────────────────────────────
// PROGRAM.CS – Applikationens startpunkt
// Her konfigureres alle services (dependency injection),
// middleware-pipelinen og testdata ved opstart.
// ──────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// Registrér MVC-controllere og Razor-views
builder.Services.AddControllersWithViews();

// Registrér databasekonteksten (EF Core).
// AddDbContext giver én instans pr. HTTP-request (Scoped livstid).
builder.Services.AddDbContext<EsperantOSContext>();

// Registrér Unit of Work og BLL-klasser som Scoped.
// Scoped betyder: én ny instans oprettes pr. HTTP-request og deles inden for samme request.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<VagtBLL>();
builder.Services.AddScoped<MedarbejderBLL>();

// Konfigurér cookie-baseret autentifikation.
// Når en bruger logger ind, gemmes deres identitet i en krypteret cookie i browseren.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";   // Send ikke-loggede brugere hertil
        options.LogoutPath = "/Account/Logout"; // Brugt ved logout
    });

var app = builder.Build();

// ──────────────────────────────────────────────────────────
// DATABASE NULSTILLING OG TESTDATA
// Ved hver opstart slettes og genskabes databasen.
// Derefter indsættes frisk testdata (medarbejdere og 10 ugers vagter).
// OBS: Dette er kun til udvikling – må aldrig køre i produktion!
// ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EsperantOSContext>();

    // Slet den eksisterende database og opret en ny fra bunden
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();

    // Indsæt testdata kun hvis databasen er tom (undgår duplikater)
    if (!context.Vagter.Any())
    {
        // Opret fire testmedarbejdere
        var m1 = new EsperantOS.Models.Medarbejder { Name = "Simon", Bestyrelsesmedlem = false };
        var m2 = new EsperantOS.Models.Medarbejder { Name = "Mads",  Bestyrelsesmedlem = true };
        var m3 = new EsperantOS.Models.Medarbejder { Name = "Oliver", Bestyrelsesmedlem = false };
        var m4 = new EsperantOS.Models.Medarbejder { Name = "Emma",  Bestyrelsesmedlem = true };

        context.Medarbejdere.AddRange(m1, m2, m3, m4);
        context.SaveChanges();

        // Find den næste fredag som startdato for vagtplanen
        var date = DateTime.Today;
        while (date.DayOfWeek != DayOfWeek.Friday)
        {
            date = date.AddDays(1);
        }

        var rand = new Random();
        var allM = new System.Collections.Generic.List<EsperantOS.Models.Medarbejder> { m1, m2, m3, m4 };

        // Opret 10 ugers fredagsvagter (3 vagter pr. fredag: 16:00, 20:00, 00:00)
        for (int i = 0; i < 10; i++)
        {
            var fDate = date.AddDays(i * 7).AddHours(16); // Startdato for denne fredag kl. 16

            // Tre vagter pr. fredag med 4 timers mellemrum
            for (int k = 0; k < 3; k++)
            {
                var vagtDate = fDate.AddHours(k * 4); // 16:00, 20:00, 00:00

                // Vælg tilfældigt 1-2 medarbejdere til vagten
                int numM = rand.Next(1, 3);
                var shuffled = allM.OrderBy(x => rand.Next()).Take(numM).ToList();

                foreach (var medarbejder in shuffled)
                {
                    var v = new EsperantOS.Models.Vagt
                    {
                        Dato = vagtDate,
                        Ædru = (k == 2),               // Kun lukketidsvagten (00:00) kræver ædru
                        Frigivet = rand.NextDouble() > 0.7, // Ca. 30% af vagterne er frigivede
                        Medarbejdere = new System.Collections.Generic.List<EsperantOS.Models.Medarbejder> { medarbejder }
                    };

                    context.Vagter.Add(v);
                }
            }
        }

        context.SaveChanges();
    }
}

// ──────────────────────────────────────────────────────────
// MIDDLEWARE-PIPELINE
// Rækkefølgen her er vigtig – hver middleware behandler
// requesten i den rækkefølge de er registreret.
// ──────────────────────────────────────────────────────────

// I produktion: vis en brugervenlig fejlside ved ubehandlede fejl
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // HSTS fortæller browseren at den kun må bruge HTTPS i 30 dage
    app.UseHsts();
}

// Redirect HTTP-requests til HTTPS automatisk
app.UseHttpsRedirection();

// Gør URL-routing tilgængeligt for efterfølgende middleware
app.UseRouting();

// Aktiver cookie-autentifikation (læser login-cookien og bygger User-objektet)
app.UseAuthentication();

// Tjek at brugeren har rettigheder til den anmodede ressource
app.UseAuthorization();

// Servér statiske filer (CSS, JS, billeder) fra wwwroot-mappen
app.MapStaticAssets();

// Definer standard URL-mønster: /Controller/Action/Id
// Standard: Home/Index hvis intet andet er angivet
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Start webserveren og begynd at modtage requests
app.Run();
