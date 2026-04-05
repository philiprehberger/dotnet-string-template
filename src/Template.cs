using System.Globalization;
using System.Text;

namespace Philiprehberger.StringTemplate;

/// <summary>
/// Renders template strings by replacing named placeholders with values
/// from objects, dictionaries, or key-value pairs.
/// </summary>
/// <remarks>
/// Placeholders use <c>{name}</c> syntax. Nested properties are supported via dot notation
/// (<c>{user.name}</c>). Format specifiers follow the standard .NET pattern (<c>{price:C2}</c>).
/// Escape literal braces with <c>{{</c> and <c>}}</c>.
/// </remarks>
public static class Template
{
    private static readonly TemplateOptions DefaultOptions = new();

    /// <summary>
    /// Determines whether the template string contains any <c>{name}</c> placeholders.
    /// Escaped braces (<c>{{</c> and <c>}}</c>) are not considered placeholders.
    /// </summary>
    /// <param name="template">The template string to inspect.</param>
    /// <returns><c>true</c> if the template contains at least one placeholder; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="template"/> is <c>null</c>.
    /// </exception>
    public static bool HasPlaceholders(string template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var segments = TemplateParser.Parse(template);
        return segments.Any(s => s.IsPlaceholder);
    }

    /// <summary>
    /// Extracts the names of all placeholders found in the template string.
    /// Format specifiers are stripped (e.g. <c>{price:C2}</c> yields <c>"price"</c>).
    /// Escaped braces (<c>{{</c> and <c>}}</c>) are ignored.
    /// </summary>
    /// <param name="template">The template string to inspect.</param>
    /// <returns>An array of distinct placeholder names in the order they first appear.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="template"/> is <c>null</c>.
    /// </exception>
    public static string[] ExtractPlaceholders(string template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var segments = TemplateParser.Parse(template);
        var names = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var segment in segments)
        {
            if (!segment.IsPlaceholder)
            {
                continue;
            }

            var colonIndex = segment.Value.IndexOf(':');
            var name = colonIndex == -1
                ? segment.Value.Trim()
                : segment.Value[..colonIndex].Trim();

            if (seen.Add(name))
            {
                names.Add(name);
            }
        }

        return names.ToArray();
    }

    /// <summary>
    /// Renders a template by replacing placeholders with values from the specified object's properties.
    /// </summary>
    /// <param name="template">The template string containing named placeholders.</param>
    /// <param name="values">An object whose public properties provide the placeholder values.</param>
    /// <returns>The rendered string with all placeholders replaced.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="template"/> or <paramref name="values"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when a placeholder references a property that does not exist on the object
    /// and <see cref="TemplateOptions.MissingKeyBehavior"/> is <c>"Throw"</c>.
    /// </exception>
    public static string Render(string template, object values)
    {
        return Render(template, values, DefaultOptions);
    }

    /// <summary>
    /// Renders a template by replacing placeholders with values from the specified dictionary.
    /// </summary>
    /// <param name="template">The template string containing named placeholders.</param>
    /// <param name="values">A dictionary mapping placeholder names to their values.</param>
    /// <returns>The rendered string with all placeholders replaced.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="template"/> or <paramref name="values"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when a placeholder references a key that does not exist in the dictionary
    /// and <see cref="TemplateOptions.MissingKeyBehavior"/> is <c>"Throw"</c>.
    /// </exception>
    public static string Render(string template, IDictionary<string, object?> values)
    {
        return Render(template, values, DefaultOptions);
    }

    /// <summary>
    /// Renders a template by replacing placeholders with values from the specified object's properties,
    /// using the provided options to control rendering behavior.
    /// </summary>
    /// <param name="template">The template string containing named placeholders.</param>
    /// <param name="values">An object whose public properties provide the placeholder values.</param>
    /// <param name="options">Options controlling missing key behavior and default values.</param>
    /// <returns>The rendered string with all placeholders replaced.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="template"/>, <paramref name="values"/>, or <paramref name="options"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when a placeholder references a property that does not exist on the object
    /// and <see cref="TemplateOptions.MissingKeyBehavior"/> is <c>"Throw"</c>.
    /// </exception>
    public static string Render(string template, object values, TemplateOptions options)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(options);

        var segments = TemplateParser.Parse(template);
        var sb = new StringBuilder(template.Length);
        var isDictionary = values is IDictionary<string, object?>;

        foreach (var segment in segments)
        {
            if (!segment.IsPlaceholder)
            {
                sb.Append(segment.Value);
                continue;
            }

            var (key, format) = ParsePlaceholder(segment.Value);
            bool resolved;
            object? value;

            if (isDictionary)
            {
                resolved = PlaceholderResolver.TryResolve(key, (IDictionary<string, object?>)values, out value);
            }
            else
            {
                resolved = PlaceholderResolver.TryResolve(key, values, out value);
            }

            if (!resolved)
            {
                switch (options.MissingKeyBehavior)
                {
                    case "Throw":
                        throw new KeyNotFoundException($"Placeholder key '{key}' was not found.");
                    case "LeaveTemplate":
                        sb.Append('{').Append(segment.Value).Append('}');
                        continue;
                    case "Empty":
                        sb.Append(options.DefaultValue);
                        continue;
                    default:
                        throw new ArgumentException(
                            $"Unknown MissingKeyBehavior: '{options.MissingKeyBehavior}'. " +
                            "Valid values are \"Throw\", \"Empty\", or \"LeaveTemplate\".",
                            nameof(options));
                }
            }

            sb.Append(FormatValue(value, format));
        }

        return sb.ToString();
    }

    private static string Render(string template, IDictionary<string, object?> values, TemplateOptions options)
    {
        return Render(template, (object)values, options);
    }

    private static (string Key, string? Format) ParsePlaceholder(string placeholder)
    {
        var colonIndex = placeholder.IndexOf(':');

        if (colonIndex == -1)
        {
            return (placeholder.Trim(), null);
        }

        var key = placeholder[..colonIndex].Trim();
        var format = placeholder[(colonIndex + 1)..];
        return (key, format);
    }

    private static string FormatValue(object? value, string? format)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (format is not null && value is IFormattable formattable)
        {
            return formattable.ToString(format, CultureInfo.CurrentCulture);
        }

        return value.ToString() ?? string.Empty;
    }
}
