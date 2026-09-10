using System;
using System.Linq;
using GroupDocs.Metadata;
using GroupDocs.Metadata.Tagging;

namespace RemovePdfMetadata;

/// <summary>
/// Showcase methods for removing PDF Info dictionary and XMP metadata.
/// </summary>
/// <remarks>
/// Each public method carries summary and remarks so content-builder can
/// extract runnable samples. License setup lives in Program.cs.
/// </remarks>
public static class PdfMetadataCleaner
{
    /// <summary>
    /// Inspect Author, Creator, Producer and related PDF metadata.
    /// </summary>
    /// <remarks>
    /// Loads a PDF and prints identity-bearing document properties so you
    /// can confirm what the Info dictionary and related packages expose
    /// before cleaning. Browser cleaners often miss XMP; this listing is
    /// the baseline for a before/after check.
    /// </remarks>
    public static void InspectPdfDocumentMetadata(string inputPath)
    {
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
    }

    /// <summary>
    /// Remove all detected metadata packages from a PDF with Sanitize.
    /// </summary>
    /// <remarks>
    /// Calls Sanitize to clear recognized metadata packages — including
    /// Info dictionary fields and XMP — then saves the cleaned file. Use
    /// this when a PDF leaves a trust boundary and no authorship trail
    /// should remain. Returns the number of removed properties.
    /// </remarks>
    public static void SanitizePdfMetadata(string inputPath, string outputPath)
    {
        using var metadata = new Metadata(inputPath);
        int removed = metadata.Sanitize();
        Console.WriteLine(removed);
        metadata.Save(outputPath);
    }

    /// <summary>
    /// Remove author-style PDF properties while keeping descriptive fields.
    /// </summary>
    /// <remarks>
    /// Uses RemoveProperties with person/creator tags so Author-style
    /// identity is stripped without wiping Title and Subject. Prefer this
    /// when the document stays in circulation but must not name people.
    /// </remarks>
    public static void RemovePdfAuthorMetadata(string inputPath, string outputPath)
    {
        using var metadata = new Metadata(inputPath);
        int removed = metadata.RemoveProperties(p =>
            p.Tags.Contains(Tags.Person.Creator) ||
            p.Tags.Contains(Tags.Person.Editor) ||
            string.Equals(p.Name, "Author", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.Name, "Creator", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.Name, "Producer", StringComparison.OrdinalIgnoreCase));
        Console.WriteLine(removed);
        metadata.Save(outputPath);
    }

    /// <summary>
    /// Verify that person/author identity metadata is gone after cleaning.
    /// </summary>
    /// <remarks>
    /// Re-opens the cleaned PDF and looks for Author / Person.Creator /
    /// Person.Editor properties. Prints True when none remain. Do not treat
    /// residual Creator/Producer Tool.Software values as a failed wipe —
    /// Save may rewrite those fields with the PDF engine fingerprint.
    /// </remarks>
    public static void VerifyPdfAuthorMetadataRemoved(string inputPath)
    {
        using var metadata = new Metadata(inputPath);
        var leftovers = metadata.FindProperties(p =>
            p.Tags.Contains(Tags.Person.Creator) ||
            p.Tags.Contains(Tags.Person.Editor) ||
            string.Equals(p.Name, "Author", StringComparison.OrdinalIgnoreCase));

        Console.WriteLine(!leftovers.Any());
    }
}
