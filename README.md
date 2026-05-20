# DES - Desktop Event Simulator

.NET 10 aplikacija za avtomatsko zaznavanje slik na zaslonu in simulacijo klikov miške.

## Funkcionalnost

- 🔍 Zaznavanje slik na zaslonu (screenshot matching)
- 🖱️ Avtomatsko premikanje miške in klikanje
- ⚙️ XML konfiguracijska datoteka
- 🐛 Debug mode z shranjevanjem screenshotov
- 📦 Standalone executable (ne potrebuje .NET SDK)

## Namestitev

### Iz izvorne kode
```bash
git clone https://github.com/deanpodgornik/DES.git
cd DES
dotnet restore
dotnet build
```

### Standalone verzija
Prenesi zadnjo verzijo iz [Releases](https://github.com/deanpodgornik/DES/releases).

## Uporaba

1. **Kopiraj `config.example.xml` v `config.xml`**
2. **Uredi `config.xml`** in nastavi:
   - `SearchX`, `SearchY` - koordinate kjer iščeš sliko
   - `SearchWidth`, `SearchHeight` - velikost območja
   - `ClickX`, `ClickY` - kam naj klikne
   - `TemplateImagePath` - pot do template slike
   - `CheckIntervalMs` - interval preverjanja v ms
   - `MatchTolerance` - toleranca ujemanja (0-255)

3. **Ustvari template sliko**:
   - Uporabi Snipping Tool (Win+Shift+S)
   - Zajemi točno `SearchWidth × SearchHeight` px območje
   - Shrani kot `template.png`

4. **Zaženi aplikacijo**:
```bash
dotnet run
```

## Koordinatni sistem

Koordinata `(0, 0)` je v **zgornjem levem kotu** zaslona:
- **X os**: levo → desno
- **Y os**: zgoraj → dol

## Debug Mode

Za odpravljanje težav nastavi v `config.xml`:
```xml
<DebugMode>true</DebugMode>
```

Debug mode:
- Shranjuje vsak screenshot v `debug_screenshots/`
- Prikazuje maksimalne razlike RGB vrednosti
- Pomaga ugotoviti zakaj se slika ne najde

## Build Standalone Executable

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Executable bo v: `bin\Release\net10.0\win-x64\publish\ScreenAutoClicker.exe`

## Zahteve

- .NET 10 SDK (za development)
- Windows (uporablja Win32 API)

## Struktura projekta

```
DES/
├── Program.cs              # Glavna aplikacija in konfiguracija
├── ScreenCapture.cs        # Screenshot funkcionalnost
├── ImageMatcher.cs         # Primerjava slik
├── MouseController.cs      # Kontrola miške
├── config.example.xml      # Primer konfiguracije
└── ScreenAutoClicker.csproj
```

## Licenca

MIT
