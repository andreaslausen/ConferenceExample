## Backend

- [ ] Parallele Ausführung von Akzeptanztests
- [x] Architekturregeln überarbeiten: - Handler rufen keine anderen Handler auf, usw.
- [x] Autorisierung überarbeiten: Ist sichergestellt, dass nicht auf Daten anderer Benutzer zugegriffen werden kann?
- [ ] Commands, die mehr als ein Event auslösen (z.B. EditTalk), hinterfragen.
- [ ] Ein ReadModel einführen, das nur für eine spezielle Ansicht da ist.
- [x] Paging überarbeiten: Services laden aktuell immer alles
- [x] Mutation Scores auf 100% bringen
- [x] Exception Handling implementieren
- [ ] Sind die Building Blocks der Event Sourcing / CQRS Architektur ausreichend in der Architekturdokumentation beschrieben (z.B. Query, QueryHandler, Event, EventHandler, ReadModel, usw.)
## Product
- [ ] Testen und Ist-Stand aufnehmen. Danach weitere Features planen.