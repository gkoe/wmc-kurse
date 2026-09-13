# wmc-kurse — von der Minimal API zum Result-Pattern

Unterrichtsbeispiel für **Web & Mobile Computing**, HTL Leonding, Abendschule 2026/27.

Eine einzige Anwendung — eine kleine Kursverwaltung — in mehreren Ständen. Jeder Stand
liegt in einem **eigenen Ordner**: Sie können zwei davon nebeneinander öffnen und
vergleichen, statt zwischen Git-Tags hin und her zu springen.

## Die Stände

| Ordner | Was passiert | Stand |
|---|---|---|
| [`011_Minimal_Api`](011_Minimal_Api) | Alles in einer Datei. DbContext im Endpoint. Validierung kopiert. | **fertig** |
| `012_Domain` | Die Regeln wandern in die Entity | folgt |
| `013_Repository` | Persistenz hinter Repository und Unit of Work | folgt |
| `014_Application` | Anwendungsschicht: Services und DTOs | folgt |
| `015_Cqrs` | Commands und Queries statt Services | folgt |
| `016_Validation_Pipeline` | Validierung als Behavior, an einer Stelle | folgt |
| `017_Result_Pattern` | Result statt Exceptions für erwartbare Fehler | folgt |

## Der rote Faden

Wir bauen zuerst **absichtlich schlecht**. Dann spüren wir, warum das weh tut. Erst dann
bauen wir um — Schritt für Schritt, jeder Schritt beauftragt und geprüft.

Nach jedem Schritt zwei Fragen:

> **Was ist besser geworden?**
> Und — mindestens so wichtig — **was ist schlechter geworden?**

Saubere Architektur kostet Dateien, Indirektion und Einarbeitungszeit. Wer das nicht
benennen kann, hat sie nicht verstanden. Genau danach wird auch gefragt.

## Starten

```bash
git clone https://github.com/gkoe/wmc-kurse.git
cd wmc-kurse

dotnet run --project 011_Minimal_Api
```

Voraussetzung: **.NET 10 SDK**. Die Datenbank ist SQLite und wird beim Start angelegt.

Vergleichen, sobald ein zweiter Stand da ist:

```bash
# unter Windows
fc /n 011_Minimal_Api\Program.cs 012_Domain\Program.cs
```

## Wenn der Build scheitert

**`NU1008` — „Projekte, die die zentrale Paketverwaltung verwenden, müssen …"**
Die Paketversionen stehen zentral in `Directory.Packages.props`; eine `csproj` darf
deshalb `Version="…"` nicht selbst setzen. Ohne Version eintragen:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
```

**Der Ordner liegt unterhalb eines anderen mit eigener `Directory.Packages.props`.**
MSBuild sucht nach oben und nimmt die **erste** Datei, die es findet. Deshalb liegt hier
eine eigene — sie darf nicht gelöscht werden.

## Stoff und Prüfung

Der Lehrstoff steht im Wiki: **https://github.com/htl-leo/wmc26**
Prüfungsrelevant ist ausschließlich, was dort steht.

Dieses Repository ist das **Beispiel**, an dem der Stoff entsteht — kein eigener
Prüfungsstoff. Die Begriffe dazu (Abhängigkeitsrichtung, CQRS, Result-Pattern …)
bekommen ihr Kapitel im Wiki, sobald wir sie gebaut haben.

## Lizenz

MIT — verwenden, verändern und weitergeben ausdrücklich erlaubt.
