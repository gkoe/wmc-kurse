using Microsoft.EntityFrameworkCore;

// =============================================================================
//  STAND 011 — Minimal API. Quick and dirty.
//
//  Eine Datei. Kein Muster. Datenbankzugriff direkt im Endpoint.
//  Es funktioniert, und es ist getestet worden, indem jemand F5 gedrueckt hat.
//
//  Genau so faengt man an, wenn man schnell etwas zeigen will - und genau hier
//  entstehen die Schmerzen, aus denen die naechsten Staende folgen.
// =============================================================================

var builder = WebApplication.CreateBuilder(args);

// Der DI-Container: Wer CourseDb braucht, bekommt eine Instanz je Anfrage.
builder.Services.AddDbContext<CourseDb>(o => o.UseSqlite("Data Source=kurse.db"));

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
    // ---- Validierung, erstes Vorkommen --------------------------------------
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(course.Title))
        errors["title"] = ["Titel darf nicht leer sein."];
    else if (course.Title.Length > 120)
        errors["title"] = ["Titel darf hoechstens 120 Zeichen haben."];
    else if (await db.Courses.AnyAsync(c => c.Title == course.Title))
        errors["title"] = ["Ein Kurs mit diesem Titel existiert bereits."];

    if (string.IsNullOrWhiteSpace(course.Trainer))
        errors["trainer"] = ["Trainer darf nicht leer sein."];

    if (course.PriceEuro < 0)
        errors["priceEuro"] = ["Preis darf nicht negativ sein."];

    if (course.Seats < 1 || course.Seats > 200)
        errors["seats"] = ["Plaetze muessen zwischen 1 und 200 liegen."];

    if (course.StartsOn <= DateOnly.FromDateTime(DateTime.Today))
        errors["startsOn"] = ["Kursbeginn muss in der Zukunft liegen."];

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);
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

    // ---- Validierung, zweites Vorkommen - kopiert, mit einer Abweichung ------
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(input.Title))
        errors["title"] = ["Titel darf nicht leer sein."];
    else if (input.Title.Length > 120)
        errors["title"] = ["Titel darf hoechstens 120 Zeichen haben."];
    else if (await db.Courses.AnyAsync(c => c.Title == input.Title && c.Id != id))
        errors["title"] = ["Ein Kurs mit diesem Titel existiert bereits."];

    if (string.IsNullOrWhiteSpace(input.Trainer))
        errors["trainer"] = ["Trainer darf nicht leer sein."];

    if (input.PriceEuro < 0)
        errors["priceEuro"] = ["Preis darf nicht negativ sein."];

    if (input.Seats < 1 || input.Seats > 200)
        errors["seats"] = ["Plaetze muessen zwischen 1 und 200 liegen."];

    // ACHTUNG: Die Pruefung "Beginn muss in der Zukunft liegen" fehlt hier.
    // Beim Kopieren uebersehen. Genau so passiert es in echt - und genau
    // deshalb gehoert eine Regel an eine Stelle und nicht an zwei.

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);
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
//  Sie steht in einem Lambda, mitten in einer Datei voller Web-Zeug.
//  Ohne HTTP kommt niemand an sie heran: kein zweiter Zugang, kein Unit-Test.
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
