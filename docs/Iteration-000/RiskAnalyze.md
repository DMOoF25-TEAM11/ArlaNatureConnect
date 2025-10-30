# Risikoanalyse — Iteration000

Denne risikoanalyse opsummerer hovedrisici for projektet opdelt efter kategori. Hver risiko har et entydigt ID, en vurdering af sandsynlighed (1-5), alvorlighed (1-5), en beregnet risikoscore (Likelihood × Severity) og anbefalede afhjælpende tiltag.

## Risikoniveau (legend)
| Score | Niveau |
|---:|---|
|15–25 | Kritisk |
|10–14 | Høj |
|5–9 | Moderat |
|1–4 | Lav |

---

##1) Teknisk risiko

Tekniske risici omfatter problemer i systemets arkitektur, kode eller infrastruktur (fx sikkerhedssårbarheder, afhængigheder og databaseforbindelser), som kan føre til datatab, nedetid eller kompromitteret integritet.

### Risiko
| ID | Risiko | Beskrivelse |
|---|---|---|
| RT1 | Sikkerhedssårbarheder | Potentiel risiko for SQL-injektion, XSS, CSRF m.fl. |
| RT2 | Databrud | Uautoriseret adgang pga. svag autentifikation eller sessionsstyring |
| RT3 | Afhængighed | Sårbarheder i tredjepartsbiblioteker eller frameworks |
| RT4 | Databaseforbindelsesproblemer | Potentiale for nedetid eller ydeevneproblemer, især ved tunneling |

### Risikoniveau
| ID | Likelihood (1-5) | Severity (1-5) | Score | Risiko-niveau |
|---|:---:|:---:|:---:|---|
| RT1 |4 |5 |20 | Kritisk |
| RT2 |3 |5 |15 | Kritisk |
| RT3 |3 |4 |12 | Høj |
| RT4 |2 |4 |8 | Moderat |

### Mitigation
| ID | Mitigation |
|---|---|
| RT1 | Brug parameteriserede forespørgsler/ORM, inputvalidering, Content Security Policy, CSRF-tokens, sikkerhedstest (SAST/DAST). |
| RT2 | Implementer stærk autentifikation (MFA), kryptering i hvile og transit, adgangskontrol og rotation af nøgler/hemmeligheder. |
| RT3 | Brug afhængighedsscanning (Dependabot/Snyk), regelmæssige opdateringer og sikkerhedsrevisioner. |
| RT4 | Implementer connection pooling, retry-logik, overvågning og redundans samt performance-tuning. |

---

##2) Operationelle risici

Operationelle risici vedrører interne processer, personale og driftsprocedurer (fx dårlig UI/UX, mangelfuld dokumentation eller utilstrækkelig support), som kan påvirke adoption, kvalitet og leverancer.

### Risiko
| ID | Risiko | Beskrivelse |
|---|---|---|
| RO1 | Brugeroplevelse | Dårligt UI/UX kan føre til lav adoption |

### Risikoniveau
| ID | Likelihood (1-5) | Severity (1-5) | Score | Risiko-niveau |
|---|:---:|:---:|:---:|---|
| RO1 |3 |3 |9 | Moderat |

### Mitigation
| ID | Mitigation |
|---|---|
| RO1 | Gennemfør brugerundersøgelser, usability-tests, iterer på design og mål KPI'er for adoption. |

---

##3) Compliance risici

Compliance risici omfatter risiko for manglende overholdelse af love, regler og standarder (fx GDPR, kontraktmæssige krav eller branchestandarder), hvilket kan medføre juridiske sanktioner, bøder eller tab af omdømme.

### Risiko
| ID | Risiko | Beskrivelse |
|---|---|---|
| RC1 | Regulativ overholdelse (GDPR) | Risiko for manglende overholdelse af databeskyttelsesregler |

### Risikoniveau
| ID | Likelihood (1-5) | Severity (1-5) | Score | Risiko-niveau |
|---|:---:|:---:|:---:|---|
| RC1 |3 |5 |15 | Kritisk |

### Mitigation
| ID | Mitigation |
|---|---|
| RC1 | Implementer dataminimering, samtykke-styring, datapolitikker, Data Protection Impact Assessment (DPIA) og klare retention/ sletteprocedurer. |

---

##4) Projektledelsesrisici

Projektledelsesrisici dækker risici i forbindelse med planlægning, scope, ressourcer og kommunikation, som kan føre til forsinkelser, budgetoverskridelser eller reduceret kvalitet i leverancerne.

### Risiko
| ID | Risiko | Beskrivelse |
|---|---|---|
| RP1 | Scope Creep | Ændringer i scope der fører til forsinkelser og overskridelser |
| RP2 | Ressource-tilgængelighed | Nøglepersoner kan blive utilgængelige |

### Risikoniveau
| ID | Likelihood (1-5) | Severity (1-5) | Score | Risiko-niveau |
|---|:---:|:---:|:---:|---|
| RP1 |3 |4 |12 | Høj |
| RP2 |2 |4 |8 | Moderat |

### Mitigation
| ID | Mitigation |
|---|---|
| RP1 | Indfør change control, klare acceptkriterier, sprintplanlægning og prioriteringsgennemgang. |
| RP2 | Kryds-træning, dokumentation, back-up planer og fleksibel ressourceplanlægning. |

---

##5) Performance risici

Performance risici omfatter problemer med systemets ydeevne, responstid og skalerbarhed (fx langsomme forespørgsler, utilstrækkelig kapacitet eller ineffektiv caching), som kan føre til dårlig brugeroplevelse eller nedetid.

### Risiko
| ID | Risiko | Beskrivelse |
|---|---|---|
| RPE1 | Skalerbarhed / belastning | Applikationen håndterer ikke øget brugerbelastning effektivt |

### Risikoniveau
| ID | Likelihood (1-5) | Severity (1-5) | Score | Risiko-niveau |
|---|:---:|:---:|:---:|---|
| RPE1 |3 |4 |12 | Høj |

### Mitigation
| ID | Mitigation |
|---|---|
| RPE1 | Design for skalerbarhed, caching, asynkrone job, load-testing og overvågning; plan for auto-scaling. |

---

## Periodiseret oversigt og anbefalinger
- Prioriter hurtig indsats på kritiske risici: RT1 (Sikkerhed), RT2 (Databrud) og RC1 (GDPR-overholdelse).
- Implementer automatiserede scanninger (SAST/DAST, SCA) og continuous monitoring.
- Indfør klare processer for change control og incident response.

## Test og verifikation
- Regelmæssige sikkerhedstest (penetrationstest), belastningstest og audits.
- Verificer mitigations ved automatiserede pipelines og manuelle audits.

## Konklusion
Kritiske risici: Sikkerhedssårbarheder (RT1), Databrud (RT2), Regulativ overholdelse (RC1).
Høje risici: Afhængigheder (RT3), Scope Creep (RP1), Skalerbarhed (RPE1).
Moderate risici: Databaseforbindelser (RT4), Brugeroplevelse (RO1), Ressource-tilgængelighed (RP2).

---

Dokumentet skal løbende opdateres efter nye fund fra sikkerhedsscanninger, test og projektets fremdrift.
