using System.Reflection;

namespace Philiprehberger.StringTemplate;

/// <summary>
/// Resolves placeholder values from objects (via reflection) or dictionaries.
/// Supports nested property access via dot notation.
/// </summary>
internal static class PlaceholderResolver
{
    /// <summary>
    /// Resolves a placeholder key against a dictionary of values.
    /// </summary>
    /// <param name="key">The placeholder key, optionally using dot notation for nested access.</param>
    /// <param name="values">The dictionary to resolve values from.</param>
    /// <param name="result">The resolved value, or <c>null</c> if not found.</param>
    /// <returns><c>true</c> if the key was resolved; otherwise <c>false</c>.</returns>
    internal static bool TryResolve(string key, IDictionary<string, object?> values, out object? result)
    {
        // Check for direct key match first (supports dotted keys in dictionaries)
        if (values.TryGetValue(key, out result))
        {
            return true;
        }

        // Try nested resolution via dot notation
        var parts = key.Split('.');
        if (parts.Length <= 1)
        {
            result = null;
            return false;
        }

        if (!values.TryGetValue(parts[0], out var current) || current is null)
        {
            result = null;
            return false;
        }

        return TryResolveNestedProperty(current, parts.AsSpan(1), out result);
    }

    /// <summary>
    /// Resolves a placeholder key against an object using reflection.
    /// </summary>
    /// <param name="key">The placeholder key, optionally using dot notation for nested access.</param>
    /// <param name="source">The object to resolve values from.</param>
    /// <param name="result">The resolved value, or <c>null</c> if not found.</param>
    /// <returns><c>true</c> if the key was resolved; otherwise <c>false</c>.</returns>
    internal static bool TryResolve(string key, object source, out object? result)
    {
        var parts = key.Split('.');
        return TryResolveNestedProperty(source, parts.AsSpan(), out result);
    }

    private static bool TryResolveNestedProperty(object current, ReadOnlySpan<string> parts, out object? result)
    {
        object? value = current;

        foreach (var part in parts)
        {
            if (value is null)
            {
                result = null;
                return false;
            }

            if (value is IDictionary<string, object?> dict)
            {
                if (!dict.TryGetValue(part, out value))
                {
                    result = null;
                    return false;
                }

                continue;
            }

            var type = value.GetType();
            var property = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null)
            {
                result = null;
                return false;
            }

            value = property.GetValue(value);
        }

        result = value;
        return true;
    }
}
