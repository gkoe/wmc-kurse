# 011 — Minimal API

Der Ausgangspunkt. **Eine Datei, kein Muster, Datenbankzugriff direkt im Endpoint.**

Sechs Endpunkte, eine Entity, ein DbContext, 190 Zeilen. Es funktioniert — und es ist
getestet worden, indem jemand F5 gedrückt hat.

## Starten

```bash
dotnet run --project 011_Minimal_Api
```

Läuft auf `http://localhost:5080`. Die SQLite-Datei `kurse.db` wird beim Start angelegt,
falls sie fehlt. Zum Zurücksetzen einfach löschen.

Endpunkte ausprobieren: [`kurse.http`](kurse.http) im Editor öffnen und die Anfragen der
Reihe nach abschicken.

## Was drin ist

| Endpunkt | Antwortet mit |
|---|---|
| `GET /courses` | 200 — alle Kurse, nach Beginn sortiert |
| `GET /courses/{id:int}` | 200 oder 404 |
| `POST /courses` | 201 mit Location-Header, oder 400 mit den Feldfehlern |
| `PUT /courses/{id:int}` | 200, 400 oder 404 |
| `DELETE /courses/{id:int}` | 204 oder 404 |
| `POST /courses/{id:int}/enrol` | 200, 404 oder 409 |

Die Einzelheiten — Host und Pipeline, Parameter-Binding, Routen-Constraints, `Results.*`,
Change Tracking, `EnsureCreated` — stehen in der Präsentation **02_Minimal_Api**.

## Was hier schon weh tut

Nicht, weil es schlecht gemacht wäre. Weil es *so* gemacht ist:

1. **Die Validierung steht zweimal da**, in `POST` und in `PUT` — kopiert.
2. **Die einzige echte Geschäftsregel** („ausgebucht", „hat bereits begonnen") steht in
   einem Lambda, mitten im Web-Zeug. Ohne HTTP kommt niemand an sie heran.
3. **Es gibt keinen Test**, und es lässt sich auch keiner schreiben, ohne einen Webserver
   und eine Datenbank zu starten.
4. **Ein zweiter Zugang** — eine Blazor-Seite mit denselben Regeln — müsste alles
   kopieren.

> **In `Program.cs` steckt ein Fehler, der genau daraus entstanden ist.**
> Ein Kursbeginn in der Vergangenheit wird beim Anlegen abgelehnt und beim Ändern
> angenommen. Beim Kopieren übersehen. Genau so passiert es in echt — und genau deshalb
> gehört eine Regel an **eine** Stelle und nicht an zwei.
>
> Probieren Sie es aus: die beiden markierten Anfragen in `kurse.http`.

## Die drei Änderungswünsche

Bevor irgendetwas umgebaut wird — von Hand, ohne AI, Zeitrahmen 45 Minuten:

1. **Neue Regel.** Ein Kurs darf nur angelegt werden, wenn der Preis pro Platz unter
   500 € liegt. *Danach:* An wie vielen Stellen mussten Sie die Regel hinschreiben?
2. **Zweiter Zugang.** Neben der API soll eine Blazor-Seite Kurse anlegen können, mit
   denselben Regeln. *Danach:* Wie kommen Sie an die Validierung heran, ohne HTTP zu
   sprechen?
3. **Ein Test.** Schreiben Sie einen Unit-Test für „Kurs ist ausgebucht → Anmeldung wird
   abgelehnt". *Danach:* Was brauchen Sie alles, damit dieser Test läuft?

Wer alle drei gelöst hat, hat auch die Antwort darauf, warum die nächsten Stände kommen.
Wer sie nicht gelöst hat, erst recht.
