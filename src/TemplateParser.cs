namespace Philiprehberger.StringTemplate;

/// <summary>
/// Parses a template string into a sequence of segments (literal text and placeholder references).
/// </summary>
internal static class TemplateParser
{
    /// <summary>
    /// Parses the template into an ordered list of segments.
    /// Escaped braces (<c>{{</c> and <c>}}</c>) are converted to literal <c>{</c> and <c>}</c>.
    /// </summary>
    /// <param name="template">The template string to parse.</param>
    /// <returns>A list of <see cref="TemplateSegment"/> instances.</returns>
    /// <exception cref="FormatException">Thrown when the template contains unmatched braces.</exception>
    internal static List<TemplateSegment> Parse(string template)
    {
        var segments = new List<TemplateSegment>();
        var i = 0;
        var literalStart = 0;

        while (i < template.Length)
        {
            if (template[i] == '{')
            {
                if (i + 1 < template.Length && template[i + 1] == '{')
                {
                    // Escaped opening brace: capture preceding literal + the escaped brace
                    segments.Add(new TemplateSegment(template[literalStart..i] + "{", IsPlaceholder: false));
                    i += 2;
                    literalStart = i;
                    continue;
                }

                // Capture any preceding literal text
                if (i > literalStart)
                {
                    segments.Add(new TemplateSegment(template[literalStart..i], IsPlaceholder: false));
                }

                // Find closing brace
                var closingIndex = template.IndexOf('}', i + 1);
                if (closingIndex == -1)
                {
                    throw new FormatException($"Unmatched opening brace at position {i}.");
                }

                var placeholderContent = template[(i + 1)..closingIndex];
                segments.Add(new TemplateSegment(placeholderContent, IsPlaceholder: true));
                i = closingIndex + 1;
                literalStart = i;
            }
            else if (template[i] == '}')
            {
                if (i + 1 < template.Length && template[i + 1] == '}')
                {
                    // Escaped closing brace
                    segments.Add(new TemplateSegment(template[literalStart..i] + "}", IsPlaceholder: false));
                    i += 2;
                    literalStart = i;
                    continue;
                }

                throw new FormatException($"Unmatched closing brace at position {i}.");
            }
            else
            {
                i++;
            }
        }

        // Capture any remaining literal text
        if (literalStart < template.Length)
        {
            segments.Add(new TemplateSegment(template[literalStart..], IsPlaceholder: false));
        }

        return segments;
    }
}

/// <summary>
/// Represents a segment of a parsed template string.
/// </summary>
/// <param name="Value">
/// The segment content. For literals, this is the raw text. For placeholders, this is the
/// placeholder expression (e.g. <c>"name"</c> or <c>"price:C2"</c>).
/// </param>
/// <param name="IsPlaceholder">Whether this segment is a placeholder reference.</param>
internal record TemplateSegment(string Value, bool IsPlaceholder);
