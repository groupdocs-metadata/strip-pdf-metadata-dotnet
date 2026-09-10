using System;
using System.IO;
using GroupDocs.Metadata;
using RemovePdfMetadata;

// Prefer the project Resources folder so outputs stay next to the sample input.
var projectResources = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources"));
var resources = Directory.Exists(projectResources)
    ? projectResources
    : Path.Combine(AppContext.BaseDirectory, "Resources");

var input = Path.Combine(resources, "contract-with-metadata.pdf");
var outSanitized = Path.Combine(resources, "contract-sanitized.pdf");
var outAuthorsRemoved = Path.Combine(resources, "contract-authors-removed.pdf");

ApplyLicenseFromEnvironment();

if (!File.Exists(input))
{
    throw new FileNotFoundException($"Missing sample PDF: {input}");
}

Directory.CreateDirectory(resources);

Console.WriteLine("--- before ---");
PdfMetadataCleaner.InspectPdfDocumentMetadata(input);

Console.WriteLine("--- sanitize all metadata ---");
PdfMetadataCleaner.SanitizePdfMetadata(input, outSanitized);
Console.WriteLine("--- after sanitize (identity fields) ---");
PdfMetadataCleaner.InspectPdfDocumentMetadata(outSanitized);
PdfMetadataCleaner.VerifyPdfAuthorMetadataRemoved(outSanitized);

Console.WriteLine("--- remove author metadata only ---");
PdfMetadataCleaner.RemovePdfAuthorMetadata(input, outAuthorsRemoved);
Console.WriteLine("--- after author removal (identity fields) ---");
PdfMetadataCleaner.InspectPdfDocumentMetadata(outAuthorsRemoved);
PdfMetadataCleaner.VerifyPdfAuthorMetadataRemoved(outAuthorsRemoved);

static void ApplyLicenseFromEnvironment()
{
    var licenseDir = Environment.GetEnvironmentVariable("LIC_METADATA_VALID");
    if (string.IsNullOrWhiteSpace(licenseDir))
    {
        return;
    }

    var licensePath = Path.Combine(licenseDir, "GroupDocs.Metadata.Product.Family.lic");
    if (!File.Exists(licensePath))
    {
        return;
    }

    var license = new License();
    license.SetLicense(licensePath);
}
