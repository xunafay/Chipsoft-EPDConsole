# ChipSoft Patient Dossier Opdracht

## Inleiding

Dit is mijn implementatie voor Chipsoft hun Technisch Interview opdracht

## Wat is geïmplementeerd

- Alle gevraagde functionaliteit
- [Spectre](https://spectreconsole.net/) als TUI framework
  - Het wiel opnieuw uitvinden is leuk als hobby programmeren maar voor production code is het beter om battle tested libraries te gebruiken
- In debug zal een patiënt en arts automatisch aangemaakt worden wanneer je de database reset
- Een test project om de logistiek van afspraak planning te testen
- Er is een devcontainer voor Visual Studio Code toegevoegd moest je system .NET Core versie niet overeenkomen met de versie van het project
- Er is een simpele github actions workflow toegevoegd die de tests uitvoert en het project bouwt

## Wat kan er nog verbeterd worden

Een aantal verbeteringen kunnen toegevoegd worden maar om onder de gerecommendeerde 4 uur werk te blijven heb ik volgende zaken niet geïmplementeerd:

- Afspraken inkijken op een specifieke datum
- Code kan wat beter georganizeerd worden
- Extra testen toevoegen
- Betere error handling
