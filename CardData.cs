using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevineClairvoyance;

/// <summary>
/// One entry from cards.json. Suits have only a long meaning; cards have both.
/// </summary>
public sealed class CardEntry
{
    [JsonPropertyName("short")]
    public string? Short { get; set; }

    [JsonPropertyName("long")]
    public string? Long { get; set; }
}

/// <summary>
/// Loads and serves all Tarot card / suit text from cards.json — the single
/// source of truth that replaces the old Module1.vb hard-coded strings.
/// </summary>
public static class CardData
{
    private static readonly Dictionary<string, CardEntry> _entries = Load();

    private static Dictionary<string, CardEntry> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "cards.json");
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<Dictionary<string, CardEntry>>(json);
        return data ?? new Dictionary<string, CardEntry>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Long meaning for a card or suit (the TextBox content).</summary>
    public static string GetMeaning(string name) =>
        _entries.TryGetValue(name, out var e) && e.Long is not null ? e.Long : string.Empty;

    /// <summary>Short phrase for a card (Label2 / hint text).</summary>
    public static string GetShortTerm(string name) =>
        _entries.TryGetValue(name, out var e) && e.Short is not null ? e.Short : "Brief Meaning";

    /// <summary>The 78 card names, in deck order, grouped by suit.</summary>
    public static readonly string[] MajorArcana =
    {
        "The Fool", "The Magician", "The High Priestess", "The Empress", "The Emperor",
        "The Hierophant", "The Lovers", "The Chariot", "Strength", "The Hermit",
        "Wheel of Fortune", "Justice", "The Hanged Man", "Death", "Temperance",
        "The Devil", "The Tower", "The Star", "The Moon", "The Sun", "Judgement", "The World",
    };

    public static readonly string[] Cups = Suit("Cups");
    public static readonly string[] Pentacles = Suit("Pentacles");
    public static readonly string[] Swords = Suit("Swords");
    public static readonly string[] Wands = Suit("Wands");

    private static string[] Suit(string suit) => new[]
    {
        "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
        "Page", "Knight", "Queen", "King",
    }.Select(rank => $"{rank} of {suit}").ToArray();

    /// <summary>Every card name in the deck (used by the spread to draw at random).</summary>
    public static IEnumerable<string> AllCards =>
        MajorArcana.Concat(Cups).Concat(Pentacles).Concat(Swords).Concat(Wands);
}
