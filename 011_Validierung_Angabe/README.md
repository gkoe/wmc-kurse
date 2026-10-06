# 011 — Angabe: Validierung

Der Stand [`011_Minimal_Api`](../011_Minimal_Api) — **ohne Validierung**. Alle sechs
Endpunkte laufen, aber `POST /courses` und `PUT /courses/{id}` nehmen alles an.

Die Aufgabe steht in [`011_Validierung_Angabe.pdf`](../011_Validierung_Angabe.pdf):
die beiden mit `TODO` markierten Stellen in `Program.cs` ausfüllen.

## Starten

```bash
dotnet run --project 011_Validierung_Angabe
```

Läuft auf `http://localhost:5080`. Vor dem Prüfen `courses.db` löschen.

## Prüfen

[`courses.http`](courses.http) der Reihe nach abschicken. Hinter jeder Anfrage steht, was
sie liefern muss. Solange die Validierung fehlt, kommt überall `201` bzw. `200`.

## Lösung

Die Vorlage [`011_Minimal_Api/Program.cs`](../011_Minimal_Api/Program.cs) — erst ansehen,
wenn Ihre Lösung alle Anfragen in `courses.http` besteht. Achtung: Die Vorlage enthält
absichtlich einen Fehler, den Ihre Lösung nicht haben darf.
