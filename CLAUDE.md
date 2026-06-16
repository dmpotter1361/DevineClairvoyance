# Devine Clairvoyance — notes for Claude Code

A C# / .NET 10 WinForms Tarot app (ported from an older VB.NET version). Windows-only.

## Run / build

```powershell
dotnet run                 # launch the app
dotnet build -c Release    # release build
```

No tests yet. There is no Visual Studio dependency — plain `dotnet` is enough.

## Layout

- **`cards.json`** — single source of truth for all card/suit text: a JSON object
  keyed by name. Suits (`Major Arcana`, `Cups`, `Pentacles`, `Swords`, `Wands`) have
  a `long` field; the 78 cards have `long` + `short`. **To change card meanings, edit
  this file** — no code change required. Copied next to the exe at build.
- **`CardData.cs`** — loads `cards.json`; exposes deck ordering, `GetMeaning(name)`,
  `GetShortTerm(name)`, and `AllCards`.
- **`Assets/`** — 78 card PNGs (named exactly like the card, e.g. `The Fool.png`),
  spread/stack art, `tarot sheet.png` background, Harrington font, `Icon1.ico`.
  Loaded by name through **`Assets.cs`**.
- **`MainForm.cs`** — main window: suit/card tree, meaning pane, See Card / 3 Card
  Spread / Play (text-to-speech) buttons.
- **`CardViewForm.cs`** — shows one card's art.
- **`SpreadForm.cs`** — the three-card spread game (draw 3 random cards, read each).
- **`extract.ps1`** — one-off generator that built `cards.json` from the original
  `Module1.vb`; kept only for reference (reads from the old Google Drive path).

## Conventions

- Forms are hand-written in code (no `.Designer.cs` / `.resx`); match that style.
- Text-to-speech uses the `System.Speech` NuGet package (Windows only).
- Keep `cards.json` as the source of truth — don't hard-code card text in C#.
