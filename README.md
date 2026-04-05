# Philiprehberger.StringTemplate

[![CI](https://github.com/philiprehberger/dotnet-string-template/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-string-template/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.StringTemplate.svg)](https://www.nuget.org/packages/Philiprehberger.StringTemplate)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-string-template)](https://github.com/philiprehberger/dotnet-string-template/commits/main)

Named placeholder string interpolation from objects and dictionaries with formatting and defaults.

## Installation

```bash
dotnet add package Philiprehberger.StringTemplate
```

## Usage

```csharp
using Philiprehberger.StringTemplate;

var result = Template.Render("Hello, {name}!", new { name = "World" });
// "Hello, World!"
```

### Render from Object

```csharp
using Philiprehberger.StringTemplate;

var user = new { FirstName = "Alice", LastName = "Smith" };
var result = Template.Render("Welcome, {FirstName} {LastName}!", user);
// "Welcome, Alice Smith!"
```

### Render from Dictionary

```csharp
using Philiprehberger.StringTemplate;

var values = new Dictionary<string, object?>
{
    ["product"] = "Widget",
    ["quantity"] = 5
};

var result = Template.Render("Order: {quantity}x {product}", values);
// "Order: 5x Widget"
```

### Format Specifiers

```csharp
using Philiprehberger.StringTemplate;

var data = new { price = 19.99m, date = new DateTime(2026, 3, 21) };
var result = Template.Render("Price: {price:C2}, Date: {date:yyyy-MM-dd}", data);
// "Price: $19.99, Date: 2026-03-21"
```

### Detect Placeholders

```csharp
using Philiprehberger.StringTemplate;

Template.HasPlaceholders("Hello, {name}!");       // true
Template.HasPlaceholders("No placeholders here"); // false
Template.HasPlaceholders("Escaped {{braces}}");   // false
```

### Extract Placeholder Names

```csharp
using Philiprehberger.StringTemplate;

var names = Template.ExtractPlaceholders("Hello, {name}! You have {count} items.");
// ["name", "count"]

var formatted = Template.ExtractPlaceholders("{price:C2} on {date:yyyy-MM-dd}");
// ["price", "date"]
```

## API

### `Template`

| Method | Description |
|--------|-------------|
| `Render(template, values)` | Replace placeholders using an object's public properties |
| `Render(template, values)` | Replace placeholders using a dictionary |
| `Render(template, values, options)` | Replace placeholders with custom missing-key behavior |
| `HasPlaceholders(template)` | Returns `true` if the template contains any `{name}` placeholders |
| `ExtractPlaceholders(template)` | Returns a `string[]` of distinct placeholder names in order of appearance |

### `TemplateOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MissingKeyBehavior` | `string` | `"Throw"` | How to handle missing keys: `"Throw"`, `"Empty"`, or `"LeaveTemplate"` |
| `DefaultValue` | `string` | `""` | Fallback value when `MissingKeyBehavior` is `"Empty"` |

## Development

```bash
dotnet build src/Philiprehberger.StringTemplate.csproj --configuration Release
```

## Support

If you find this project useful:

⭐ [Star the repo](https://github.com/philiprehberger/dotnet-string-template)

🐛 [Report issues](https://github.com/philiprehberger/dotnet-string-template/issues?q=is%3Aissue+is%3Aopen+label%3Abug)

💡 [Suggest features](https://github.com/philiprehberger/dotnet-string-template/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)

❤️ [Sponsor development](https://github.com/sponsors/philiprehberger)

🌐 [All Open Source Projects](https://philiprehberger.com/open-source-packages)

💻 [GitHub Profile](https://github.com/philiprehberger)

🔗 [LinkedIn Profile](https://www.linkedin.com/in/philiprehberger)

## License

[MIT](LICENSE)
