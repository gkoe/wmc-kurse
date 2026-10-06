using Microsoft.EntityFrameworkCore;

// =============================================================================
//  STAND 011 — Minimal API. ANGABE: Die Validierung fehlt.
//
//  Die Anwendung laeuft, alle sechs Endpunkte funktionieren - aber POST und PUT
//  nehmen alles an: leere Titel, negative Preise, null Plaetze, einen Beginn
//  im Jahr 2020. Ihre Aufgabe: die Stellen mit TODO ausfuellen.
//
//  Welche Regeln gelten, steht in der Angabe (011_Validierung_Angabe.pdf).
//  Pruefen mit courses.http.
// =============================================================================

var builder = WebApplication.CreateBuilder(args);

// Der DI-Container: Wer CourseDb braucht, bekommt eine Instanz je Anfrage.
builder.Services.AddDbContext<CourseDb>(o => o.UseSqlite("Data Source=courses.db"));

var app = builder.Build();

// Datenbank beim Start anlegen, falls sie fehlt.
// EnsureCreated, nicht Migrate: Es gibt noch keine Migrationen. Kommt spaeter.
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<CourseDb>().Database.EnsureCreated();


// -----------------------------------------------------------------------------
//  Alle Kurse, nach Beginn sortiert
// -----------------------------------------------------------------------------
app.MapGet("/courses", async (CourseDb db) =>
    await db.Courses.OrderBy(c => c.StartsOn).ToListAsync());


// -----------------------------------------------------------------------------
//  Ein Kurs
//  {id:int} ist eine Routen-Constraint: /courses/abc trifft diese Route nicht,
//  der Aufrufer bekommt 404 statt eines Bindungsfehlers.
// -----------------------------------------------------------------------------
app.MapGet("/courses/{id:int}", async (int id, CourseDb db) =>
{
    var course = await db.Courses.FindAsync(id);
    return course is null ? Results.NotFound() : Results.Ok(course);
});


// -----------------------------------------------------------------------------
//  Neuer Kurs
// -----------------------------------------------------------------------------
app.MapPost("/courses", async (Course course, CourseDb db) =>
{
    // ---- TODO: Validierung --------------------------------------------------
    //  Alle Regeln aus der Angabe pruefen und die Fehler sammeln:
    //      var errors = new Dictionary<string, string[]>();
    //  Ist mindestens eine Regel verletzt:
    //      return Results.ValidationProblem(errors);
    // -------------------------------------------------------------------------

    // Zuruecksetzen, sonst schickt ein Client den Anmeldestand einfach mit.
    course.EnrolledCount = 0;

    db.Courses.Add(course);
    await db.SaveChangesAsync();

    // 201 mit Location-Header: Wo liegt das, was gerade entstanden ist?
    return Results.Created($"/courses/{course.Id}", course);
});


// -----------------------------------------------------------------------------
//  Kurs aendern
// -----------------------------------------------------------------------------
app.MapPut("/courses/{id:int}", async (int id, Course input, CourseDb db) =>
{
    var course = await db.Courses.FindAsync(id);
    if (course is null) return Results.NotFound();

    // ---- TODO: Validierung --------------------------------------------------
    //  Dieselben Regeln wie beim Anlegen - mit einer Besonderheit beim Titel
    //  (siehe Angabe). Geprueft wird input, nicht course.
    // -------------------------------------------------------------------------

    // Change Tracking: Der DbContext kennt course bereits aus FindAsync.
    // Die Zuweisungen genuegen, SaveChangesAsync schreibt das UPDATE.
    course.Title = input.Title;
    course.Trainer = input.Trainer;
    course.PriceEuro = input.PriceEuro;
    course.Seats = input.Seats;
    course.StartsOn = input.StartsOn;

    await db.SaveChangesAsync();
    return Results.Ok(course);
});


// -----------------------------------------------------------------------------
//  Kurs loeschen
// -----------------------------------------------------------------------------
app.MapDelete("/courses/{id:int}", async (int id, CourseDb db) =>
{
    var course = await db.Courses.FindAsync(id);
    if (course is null) return Results.NotFound();

    db.Courses.Remove(course);
    await db.SaveChangesAsync();

    // 204: erledigt, und es gibt nichts zurueckzugeben.
    return Results.NoContent();
});


// -----------------------------------------------------------------------------
//  Anmelden - die einzige echte Geschaeftsregel im ganzen Projekt.
//  Keine Validierung im Sinn dieser Aufgabe: Die Eingabe ist nur die Id, die
//  Pruefungen betreffen den Zustand des Kurses (409, nicht 400).
// -----------------------------------------------------------------------------
app.MapPost("/courses/{id:int}/enrol", async (int id, CourseDb db) =>
{
    var course = await db.Courses.FindAsync(id);
    if (course is null) return Results.NotFound();

    if (course.StartsOn <= DateOnly.FromDateTime(DateTime.Today))
        return Results.Conflict("Der Kurs hat bereits begonnen.");

    if (course.EnrolledCount >= course.Seats)
        return Results.Conflict("Der Kurs ist ausgebucht.");

    course.EnrolledCount++;
    await db.SaveChangesAsync();
    return Results.Ok(course);
});


app.Run();


// =============================================================================
//  Datenmodell und DbContext - hier unten, weil sie ja irgendwo hin muessen.
// =============================================================================

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Trainer { get; set; } = "";
    public decimal PriceEuro { get; set; }
    public int Seats { get; set; }
    public int EnrolledCount { get; set; }
    public DateOnly StartsOn { get; set; }
}

public class CourseDb(DbContextOptions<CourseDb> options) : DbContext(options)
{
    public DbSet<Course> Courses => Set<Course>();
}
