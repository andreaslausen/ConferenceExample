# Unit Testing Guidelines

So schreiben wir Unit Tests:
- Benutze bitte xUnit und (wenn nötig) NSubstitute.
- Benenne die Methoden nach dem Schema: Methodenname_Bedingungen_ErwartetesErgebnis.
- Trenne bitte Arrange, Act und Assert sauber mit Kommentaren.
- Verwende bitte file scoped Namespaces.
- Verwende keine Setup Methoden und keine Fields und keine Properties.
- Teste alle Methoden und den Konstruktor vollständig. Berücksichtige sämtliche Randfälle. 
- Unit Tests werden in einem separaten Projekt geschrieben. Das Projekt hat den gleichen Namen wie das Hauptprojekt, aber mit dem Suffix ".UnitTests".
- Verwende NSubstitute nur, wenn es unbedingt nötig ist.
- Schreibe für jeden Aufruf einer Methode eines anderen Objekts eine eigene Testmethode.

# Allgemeine Hinweise
- Benutze bitte .NET9, wenn du neue Projekte erstellst.