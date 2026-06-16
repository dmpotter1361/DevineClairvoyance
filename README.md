# Devine Clairvoyance

A small Windows desktop app for exploring the Tarot. Browse all 78 cards by suit
to read their meanings, view the card art, and try a simple three-card spread —
*Current Situation, Challenge, Advice* — with optional text-to-speech that reads
each card aloud.

> Personal project, for entertainment. Tarot readings are for reflection and fun,
> not advice.

## Features

- **Card library** — every card grouped by suit (Major Arcana, Cups, Pentacles,
  Swords, Wands) in a tree; select a suit for an overview or a card for its full
  meaning.
- **See the card** — open a window showing the card's artwork.
- **Three-card spread** — click the deck to draw three random cards into the
  *Current Situation / Challenge / Advice* positions, then click each to read its
  meaning and a short one-line interpretation.
- **Read aloud** — built-in text-to-speech (Windows `System.Speech`) speaks the
  selected meaning; toggle it on or off.

## Build & run from source

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/).

```powershell
# Run the app
dotnet run

# Build a Release binary
dotnet build -c Release
```

That's all — the card art, font, icon, and `cards.json` are copied next to the
executable automatically.

## How it works

- All card and suit text lives in **[`cards.json`](cards.json)** — the single
  source of truth (5 suit overviews + 78 cards, each with a long `long` meaning and,
  for cards, a short `short` phrase). [`CardData.cs`](CardData.cs) loads it.
- Card art, the Harrington font, and the icon live in **[`Assets/`](Assets)** and are
  loaded by name via [`Assets.cs`](Assets.cs) (e.g. `The Fool.png`).
- The UI is three WinForms windows: [`MainForm.cs`](MainForm.cs) (the library),
  [`CardViewForm.cs`](CardViewForm.cs) (card art), and
  [`SpreadForm.cs`](SpreadForm.cs) (the three-card spread).
- `cards.json` was generated from the original VB.NET project by
  [`extract.ps1`](extract.ps1); editing the JSON directly is the normal way to change
  card text now.

This app was ported from an earlier Visual Basic / WinForms version to C# on
.NET 10. To add or correct a card's meaning, just edit `cards.json` — no code change
needed.

## Continuing development with Claude Code

This project was built with AI assistance and is set up so you can keep going the
same way. To pick up where it left off on your own machine:

1. **Get the code onto your PC**

   ```bash
   git clone https://github.com/dmpotter1361/DevineClairvoyance.git
   cd DevineClairvoyance
   ```

2. **Install [Claude Code](https://claude.com/claude-code)** (Anthropic's coding CLI)
   and start it in the project folder:

   ```bash
   npm install -g @anthropic-ai/claude-code
   claude
   ```

   (You can also use the Claude Code extension for VS Code / JetBrains, or
   [claude.ai/code](https://claude.ai/code).)

3. **Point Claude at the project and ask for what you want.** A good first prompt:

   > Read the README, `CardData.cs`, and `SpreadForm.cs`, then run `dotnet build` so
   > you understand the project. I'd like to add &lt;your feature&gt;.

### Helpful map for a new contributor (human or AI)

- **`cards.json`** — all card/suit text. Edit here to change meanings; no rebuild of
  data needed.
- **`CardData.cs`** — loads `cards.json` and exposes the deck (card names by suit,
  meanings, short phrases).
- **`Assets.cs`** — loads images, the font, and the icon from `Assets/` by name.
- **`MainForm.cs` / `CardViewForm.cs` / `SpreadForm.cs`** — the three windows.
- **`extract.ps1`** — one-off script that regenerated `cards.json` from the original
  VB source; kept for reference.

## Acknowledgments

Devine Clairvoyance was originally written in Visual Basic and rebuilt in C# / .NET 10
with **Claude** (Anthropic's AI). The direction, the card interpretations, and the
testing are human; much of the implementation was AI-assisted. 🤖🤝

## License

[MIT](LICENSE)
