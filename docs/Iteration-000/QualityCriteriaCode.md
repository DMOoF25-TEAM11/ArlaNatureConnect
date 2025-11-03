# Kvalitetskriterier for programkode (OOP)

- **Encoding**: Alle C#-filer skal bruge UTF-8 encoding uden BOM (Byte Order Mark).
- **Sprogbrug**: Koden skal skrives på engelsk, inkluderet kommentarer, variabelnavn, metodenavn og klassenavn.

## CSharp Kvalitetskriterier
- **Filnavn**: Filnavnet skal bruge PascalCase og samsvaret med navnet på klassen eller interfacet som er definert i filen. For eksempel, hvis klassen hedder `Customer`, skal filen navngives `Customer.cs`.
- **Læsbarhed**: Hver af fisse (`Fields`, `Properties`, `Methods`) skal gruppers logisk og i hvert deres "#region".
    - `Methods` skal grupperes efter funktionalitet og have en beskrivende kommentar.

### Navngivningskonventioner
- **Interfaces**: Navne på interfaces skal begynde med et stort "I" efterfulgt af et beskrivende navn i PascalCase (f.eks. `IUserRepository`).
- **Namepaces**: Navne på namespaces skal bruge PascalCase og afspejle projektets struktur (f.eks. `ArlaNatureConnect.Core.Services`).
- **Types**: Navne på typer (f.eks. klasser, strukturer, enum'er) skal bruge PascalCase og være beskrivende for typen (f.eks. `FarmStatus`).
- **Attribute**: Navne på attributter skal bruge PascalCase og være beskrivende for attributtens formål (f.eks. `Required`, `MaxLength`). 
- **Klasser**: Navne på klasser skal også bruge PascalCase og være beskrivende for klassens formål (f.eks. `UserService`).
- **Parametre**: Navne på parametre skal bruge camelCase og være beskrivende for parameterens formål (f.eks. `userId`). 
- **Variabler**: Navne på variabler skal bruge camelCase og være beskrivende for variablens formål (f.eks. `totalAmount`). 
- **private fields**: Navne på private fields skal bruge camelCase og starte med en underscore (f.eks. `_firstName`). 

### Kodekvalitet
- **Metodelængde**: Metoder bør ikke overstige 30 linjer kode. Hvis en metode bliver for lang, bør den opdeles i mindre, mere fokuserede metoder.
- **Kommentarer**: Kommentarer skal bruges til at forklare "hvorfor" noget gøres, ikke "hvad" der gøres. Koden skal være selvforklarende så vidt muligt.
- **Fejlhåndtering**: Brug undtagelser til fejlhåndtering i stedet for returneringskoder. Sørg for at fange og håndtere undtagelser på passende steder. 
- **Indrykning**: Brug konsekvent indrykning med 4 mellemrum pr. niveau.
- **Brug af var**: Undgå brug `var` brug eksplicit.

## MSSQL Kvalitetskriterier
- **Navngivningskonventioner**: Brug PascalCase til navngivning af tabeller, kolonner, procedure og andre databaseobjekter (f.eks. `CustomerOrders`, `GetCustomerById`).
    - Tabeller skal navngives i ental (f.eks. `Customer`, ikke `Customers`).
    - Databaseobjekter skal have præfikset `usp` for stored procedurer og `vw` for views.
    - Alle tabeller skal have en primær nøgle.

### Kodekvalitet
- **dbo**: Alle objekter indkapsles  []
- **Indrykning**: Brug konsekvent indrykning med 4 mellemrum pr. niveau.
- **Kommentarer**: Brug kommentarer til at forklare komplekse forespørgsler eller logik i SQL-koden.

