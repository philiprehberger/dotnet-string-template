namespace Philiprehberger.StringTemplate;

/// <summary>
/// Controls how the template engine handles missing keys and other rendering behavior.
/// </summary>
/// <param name="MissingKeyBehavior">
/// Determines what happens when a placeholder references a key that does not exist.
/// Valid values: <c>"Throw"</c> (throw an exception), <c>"Empty"</c> (replace with empty string),
/// <c>"LeaveTemplate"</c> (leave the placeholder as-is). Defaults to <c>"Throw"</c>.
/// </param>
/// <param name="DefaultValue">
/// The fallback value used when <paramref name="MissingKeyBehavior"/> is <c>"Empty"</c>
/// and a more specific default is desired. Defaults to an empty string.
/// </param>
public record TemplateOptions(
    string MissingKeyBehavior = "Throw",
    string DefaultValue = "");
