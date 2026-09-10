# PDF Metadata Sanitization

[![Docs](https://img.shields.io/badge/Docs-2865E0?style=for-the-badge&logo=Hugo&logoColor=white)](https://docs.groupdocs.com/metadata/net/)
[![Blog](https://img.shields.io/badge/Blog-2865E0?style=for-the-badge&logo=WordPress&logoColor=white)](https://blog.groupdocs.com/categories/groupdocs.metadata-product-family/)
[![Free Support](https://img.shields.io/badge/Free%20Support-2865E0?style=for-the-badge&logo=Discourse&logoColor=white)](https://forum.groupdocs.com/c/metadata/)
[![Temporary License](https://img.shields.io/badge/Temporary%20License-2865E0?style=for-the-badge&logo=rocket&logoColor=white)](https://purchase.groupdocs.com/temp-license/100216)

## Overview

`strip-pdf-metadata-dotnet` is a runnable .NET 8 console demo that clears PDF Info dictionary and XMP metadata with GroupDocs.Metadata 26.8.0. It shows a full `Sanitize()` wipe next to a selective `RemoveProperties` pass that drops Author-style identity while keeping Title and Subject. The samples target backend developers who need on-prem PDF hygiene before a file leaves a trust boundary.

I built this after shipping a contract PDF that still carried `Author = Alice Example` and XMP creator tags that a browser cleaner never touched.

## Technology Stack

- Platform: .NET 8.0
- Product: GroupDocs.Metadata 26.8.0
- Language: C#
- Framework: .NET SDK-style console project

## Problem Statement

PDF files carry Author, Creator, Producer, Keywords, and XMP packets that survive ordinary "export" or print-to-PDF steps. Browser cleaners and one-off Acrobat clicks often miss XMP, so identity still travels with the bytes. Server pipelines need a repeatable API, not a GUI.

Developers also need two intensities of cleanup: erase every detected package when a file exits the company, or strip only person-tagged fields when Title and Subject must remain for filing systems.

GroupDocs.Metadata for .NET exposes `Sanitize()`, tagged `RemoveProperties`, and `FindProperties` so you can inspect, clean, and verify in code.

## Solution Overview

GroupDocs.Metadata addresses these challenges through a single `Metadata` object over the PDF, property tags such as `Tags.Person.Creator`, and save-back to a new file. Key technical features include:

- Inspect before clean: `FindProperties` lists Author, Creator, Producer, Title, Subject, and Keywords.
- Full wipe: `Sanitize()` clears recognized metadata packages, including Info and XMP.
- Selective author removal: `RemoveProperties` with person/creator predicates keeps descriptive fields.
- Verify after save: re-open and confirm Author / `Person.Creator` are gone.
- On-prem .NET 8: NuGet package `GroupDocs.Metadata` 26.8.0, no browser dependency.

## When should I use this demo?

Use this demo when a PDF must leave an internal system without Author or person-tagged creator fields, or when you need a full metadata wipe before sharing outside a trust boundary. Prefer `SanitizePdfMetadata` for outbound packs and `RemovePdfAuthorMetadata` when Title, Subject, and Keywords still matter for search. Always call `VerifyPdfAuthorMetadataRemoved` after `Save`, because the PDF engine may rewrite Tool.Software Creator/Producer fingerprints that are not Author identity.

## Prerequisites

Before running these examples, ensure you have:

- Runtime - .NET 8 SDK
- Package - GroupDocs.Metadata 26.8.0 (restored from the `.csproj`)
- License (optional for full Save) - set `LIC_METADATA_VALID` to a directory that contains `GroupDocs.Metadata.Product.Family.lic`
- Sample input - `Resources/contract-with-metadata.pdf` ships with the repo

## Getting Started

### Installation

**Using Package Manager:**

```bash
dotnet add package GroupDocs.Metadata --version 26.8.0
```

**Manual Installation:**

Clone this repository, open `RemovePdfMetadata.csproj`, and restore packages with `dotnet restore`.

### Configuration

1. Set `LIC_METADATA_VALID` to the folder that holds your Metadata family license file (optional; evaluation mode limits Save).
2. Confirm `Resources/contract-with-metadata.pdf` is present.
3. Run `dotnet run --project RemovePdfMetadata.csproj -c Release`.

## Repository Structure

```
strip-pdf-metadata-dotnet/
│
├── .gitignore
├── PdfMetadataCleaner.cs
├── Program.cs
├── RemovePdfMetadata.csproj
└── Resources/
    └── contract-with-metadata.pdf
```

### File Descriptions

- `.gitignore` - Ignores `bin/`, `obj/`, licenses, and local cleaned PDF outputs.
- `PdfMetadataCleaner.cs` - Documented methods with summary and remarks for each operation.
- `Program.cs` - Runner that applies the license from the environment and calls the documented methods.
- `RemovePdfMetadata.csproj` - .NET 8 project referencing GroupDocs.Metadata 26.8.0.
- `Resources/contract-with-metadata.pdf` - Sample PDF seeded with Author, Creator, Producer, Title, Subject, and Keywords.

## Code Implementation

### Implementation: Inspect PDF document metadata

Lists identity-bearing and descriptive PDF properties before any cleanup so you have a before baseline.

```csharp
using var metadata = new Metadata(inputPath);
var properties = metadata.FindProperties(p =>
    p.Tags.Contains(Tags.Person.Creator) ||
    p.Tags.Contains(Tags.Tool.Software) ||
    p.Tags.Contains(Tags.Content.Title) ||
    p.Tags.Contains(Tags.Content.Subject) ||
    string.Equals(p.Name, "Author", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(p.Name, "Creator", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(p.Name, "Producer", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(p.Name, "Keywords", StringComparison.OrdinalIgnoreCase));

foreach (var property in properties)
{
    Console.WriteLine($"{property.Name} = {property.Value}");
}
```

#### Technical Details

`FindProperties` walks tagged packages on the loaded PDF. The predicate mixes tag checks and name equals so Author and Keywords still surface when tags differ by producer.

Key components:
- `Metadata`: entry point for the file
- `FindProperties`: filtered property enumeration
- `Tags.Person.Creator` / `Tags.Tool.Software`: identity and tool tags

Parameters:
- `inputPath`: path to the source PDF

Output: Lines such as `author = Alice Example` and `producer = Demo PDF Library`.

---

### Implementation: Sanitize all PDF metadata

Clears recognized metadata packages in one call, then saves the cleaned PDF.

```csharp
using var metadata = new Metadata(inputPath);
int removed = metadata.Sanitize();
Console.WriteLine(removed);
metadata.Save(outputPath);
```

#### Technical Details

`Sanitize()` removes detected packages (Info dictionary fields and XMP among them) and returns how many properties were cleared. Use this when no authorship trail should remain.

Key components:
- `Sanitize`: full package wipe
- `Save`: write the cleaned PDF

Parameters:
- `inputPath` / `outputPath`: source and destination paths

Output: Integer count of removed properties (for example `7`).

---

### Implementation: Remove author-style PDF properties

Strips Author / Creator / Producer style identity while leaving Title, Subject, and Keywords.

```csharp
using var metadata = new Metadata(inputPath);
int removed = metadata.RemoveProperties(p =>
    p.Tags.Contains(Tags.Person.Creator) ||
    p.Tags.Contains(Tags.Person.Editor) ||
    string.Equals(p.Name, "Author", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(p.Name, "Creator", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(p.Name, "Producer", StringComparison.OrdinalIgnoreCase));
Console.WriteLine(removed);
metadata.Save(outputPath);
```

#### Technical Details

`RemoveProperties` applies a predicate instead of wiping every package. Person tags catch Author-style fields; name equals catch Creator/Producer strings that tools still expose.

Key components:
- `RemoveProperties`: selective deletion
- `Tags.Person.Creator` / `Tags.Person.Editor`: person identity tags

Parameters:
- `inputPath` / `outputPath`: source and destination paths

Output: Removal count (for example `3`) and a PDF that still lists Title/Subject/Keywords on inspect.

---

### Implementation: Verify author metadata removed

Re-opens the cleaned file and reports whether Author / person-creator leftovers remain.

```csharp
using var metadata = new Metadata(inputPath);
var leftovers = metadata.FindProperties(p =>
    p.Tags.Contains(Tags.Person.Creator) ||
    p.Tags.Contains(Tags.Person.Editor) ||
    string.Equals(p.Name, "Author", StringComparison.OrdinalIgnoreCase));

Console.WriteLine(!leftovers.Any());
```

#### Technical Details

Check Author and `Person.Creator` / `Person.Editor` only. Residual Creator/Producer values tagged as Tool.Software after `Save` are engine fingerprints, not a failed Author wipe.

Key components:
- `FindProperties`: leftover scan
- Boolean print: `True` when no Author-style leftovers remain

Parameters:
- `inputPath`: cleaned PDF path

Output: `True` or `False`.

## Best Practices

When implementing PDF metadata sanitization, consider these best practices:

- Inspect first: Print the before list so you know which fields the file actually carries.
- Choose wipe intensity: Use `Sanitize()` for outbound shares; use `RemoveProperties` when descriptive fields must stay.
- Verify Author, not Tool.Software: After `Save`, ignore engine-rewritten Creator/Producer fingerprints when checking person identity.
- Dispose Metadata: Keep `using` so file handles release before the next open.
- License for Save: Evaluation builds can block unrestricted `Save`; set `LIC_METADATA_VALID` for full runs.
- Keep inputs in Resources: Ship a known seeded PDF so CI and local runs share the same baseline.

## Additional Resources

For more in-depth information about PDF metadata cleanup on .NET, explore these technical resources:

* **[Remove PDF metadata use case](https://docs.groupdocs.com/metadata/net/use-cases/remove-pdf-metadata/)** - Predicted sibling docs page for this topic (draft until published): [Open page →](https://docs.groupdocs.com/metadata/net/use-cases/remove-pdf-metadata/)

* **[Remove all detected metadata packages](https://docs.groupdocs.com/metadata/net/removing-metadata/)** - Official GroupDocs.Metadata for .NET guidance on wiping packages: [Read the article →](https://docs.groupdocs.com/metadata/net/removing-metadata/)

* **[Remove specific metadata properties](https://docs.groupdocs.com/metadata/net/remove-metadata-properties/)** - Predicate-based property removal patterns: [Read the article →](https://docs.groupdocs.com/metadata/net/remove-metadata-properties/)

* **[Sibling blog: remove PDF metadata (.NET)](https://blog.groupdocs.com/metadata/remove-pdf-metadata-net/)** - Predicted article URL for this package (draft until published): [Read the article →](https://blog.groupdocs.com/metadata/remove-pdf-metadata-net/)

* **[Outlook metadata cleaner context](https://blog.groupdocs.com/metadata/introducing-outlook-metadata-cleaner-outlook-add-clean-metadata-email-attachments/)** - Earlier GroupDocs.Metadata cleaning angle for attachments: [Read the article →](https://blog.groupdocs.com/metadata/introducing-outlook-metadata-cleaner-outlook-add-clean-metadata-email-attachments/)

## Keywords

`remove PDF metadata C#`, `metadata removal API`, `strip PDF author XMP`, `sanitize PDF .NET`, `GroupDocs.Metadata`, `document sanitization`, `PDF Info dictionary`, `RemoveProperties`, `Sanitize`, `Tags.Person.Creator`, `XMP cleanup`, `.NET 8`, `FindProperties`, `PDF Author removal`, `on-prem PDF hygiene`, `Creator Producer fingerprint`, `contract PDF metadata`, `GroupDocs.Metadata 26.8`

## Support

- [GroupDocs.Metadata for .NET docs](https://docs.groupdocs.com/metadata/net/)
- [API reference](https://reference.groupdocs.com/metadata/net/)
- [Free support forum](https://forum.groupdocs.com/c/metadata/)
- [Temporary license](https://purchase.groupdocs.com/temp-license/100216)
