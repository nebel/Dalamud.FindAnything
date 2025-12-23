using Dalamud.Game;
using Dalamud.Game.Text.Sanitizer;
using Lumina.Text.ReadOnly;
using System.Runtime.CompilerServices;

namespace Dalamud.FindAnything;

public class Normalizer(ClientLanguage lang)
{
    private readonly Sanitizer sanitizer = new(lang);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Searchable(ReadOnlySeString input, bool normalizeKana = false) {
        return Searchable(input.ToText(), normalizeKana);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Searchable(string input, bool normalizeKana = false) {
        return sanitizer.Sanitize(StripPunctuation(input).Downcase(normalizeKana));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string SearchableAuto(string input) {
        return sanitizer.Sanitize(StripPunctuation(input).Downcase(lang == ClientLanguage.Japanese));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string SearchableAscii(ReadOnlySeString input) {
        return input.ToText().ToLowerInvariant();
    }

    public string SearchableAscii(string input) {
        return input.ToLowerInvariant();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string StripPunctuation(string str) {
        return str.Replace("'", string.Empty);
    }
}