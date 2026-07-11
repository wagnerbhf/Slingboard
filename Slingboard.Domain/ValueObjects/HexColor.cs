using Slingboard.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Slingboard.Domain.ValueObjects;

public sealed partial class HexColor : IEquatable<HexColor>
{
    public string Value { get; }

    private HexColor(string value) => Value = value;

    public static HexColor Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !HexRegex().IsMatch(value))
            throw new BusinessRuleViolationException("Cor deve estar no formato HEX (#RRGGBB).");

        return new HexColor(value.ToUpperInvariant());
    }

    [GeneratedRegex("^#([A-Fa-f0-9]{6})$")]
    private static partial Regex HexRegex();

    public bool Equals(HexColor? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as HexColor);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}