using PdfSharp.Pdf;
using PdfSharp.Pdf.Security;
using System;

namespace PdfReadOnly
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string pdfPath = args[0];

            if (!System.IO.File.Exists(pdfPath))
            {
                Console.Error.WriteLine("file not found!");
                return;
            }

            byte[] pdfBytes = System.IO.File.ReadAllBytes(pdfPath);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(pdfBytes))
            {
                using (PdfDocument document = PdfSharp.Pdf.IO.PdfReader.Open(ms, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Modify))
                {
                    PdfSecuritySettings securitySettings = document.SecuritySettings;
                    securitySettings.PermitAccessibilityExtractContent = false;
                    securitySettings.PermitAnnotations = false;
                    securitySettings.PermitAssembleDocument = false;
                    securitySettings.PermitExtractContent = false;
                    securitySettings.PermitFormsFill = false;
                    securitySettings.PermitModifyDocument = false;
                    securitySettings.PermitFullQualityPrint = securitySettings.PermitPrint = true;
                    securitySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted128Bit;

                    if (args.Length > 1)
                    {
                        securitySettings.OwnerPassword = args[1];
                    }
                    if (args.Length > 2)
                    {
                        securitySettings.UserPassword = args[2];
                    }

                    string filename = $"sec-{System.IO.Path.GetFileName(pdfPath)}";
                    string filepath = System.IO.Path.GetDirectoryName(pdfPath);
                    document.Save(System.IO.Path.Combine(filepath, filename));
                }
            }
        }
    }
}
