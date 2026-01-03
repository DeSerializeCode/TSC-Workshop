using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace WinFormsMain.Invoicing
{
    public class PdfInvoiceService
    {
        private readonly ILogger<PdfInvoiceService> logger;

        public PdfInvoiceService(ILogger<PdfInvoiceService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<string> GenerateInvoiceAsync(Customer customer, Vehicle vehicle, IEnumerable<InvoiceLine> lines)
        {
            if (customer is null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            if (vehicle is null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }

            if (lines is null || !lines.Any())
            {
                throw new ArgumentException("At least one line item is required", nameof(lines));
            }

            var fileName = Path.Combine(Path.GetTempPath(), $"Invoice-{vehicle.Registration}-{DateTime.Now:yyyyMMddHHmmss}.pdf");
            logger.LogInformation("Generating invoice PDF at {Path}", fileName);

            var document = new PdfDocument();
            var page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            var gfx = XGraphics.FromPdfPage(page);

            var headerFont = new XFont("Segoe UI", 18, XFontStyle.Bold);
            var subHeaderFont = new XFont("Segoe UI", 12, XFontStyle.Regular);
            var bodyFont = new XFont("Segoe UI", 10, XFontStyle.Regular);
            var boldFont = new XFont("Segoe UI", 10, XFontStyle.Bold);

            var y = 40d;
            gfx.DrawString("Workshop Invoice", headerFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 30), XStringFormats.TopLeft);
            y += 35;

            gfx.DrawString($"Customer: {customer.Name}", subHeaderFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 20), XStringFormats.TopLeft);
            y += 20;
            gfx.DrawString($"Email: {customer.Email}", bodyFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 18), XStringFormats.TopLeft);
            y += 18;
            if (!string.IsNullOrWhiteSpace(customer.Phone))
            {
                gfx.DrawString($"Phone: {customer.Phone}", bodyFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 18), XStringFormats.TopLeft);
                y += 18;
            }

            y += 6;
            gfx.DrawString($"Vehicle: {vehicle.Make} {vehicle.Model} ({vehicle.Registration})", bodyFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 18), XStringFormats.TopLeft);
            y += 18;
            gfx.DrawString($"VIN: {vehicle.Vin}", bodyFont, XBrushes.Black, new XRect(40, y, page.Width - 80, 18), XStringFormats.TopLeft);
            y += 24;

            // Table header
            gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, 40, y, page.Width - 80, 22);
            gfx.DrawString("Description", boldFont, XBrushes.Black, new XRect(50, y + 4, page.Width - 170, 14), XStringFormats.TopLeft);
            gfx.DrawString("Amount", boldFont, XBrushes.Black, new XRect(page.Width - 140, y + 4, 100, 14), XStringFormats.TopLeft);
            y += 22;

            foreach (var line in lines)
            {
                gfx.DrawRectangle(XPens.Black, new XSolidBrush(XColor.FromKnownColor(XKnownColor.White)), 40, y, page.Width - 80, 22);
                gfx.DrawString(line.Description, bodyFont, XBrushes.Black, new XRect(50, y + 4, page.Width - 170, 14), XStringFormats.TopLeft);
                gfx.DrawString(line.Amount.ToString("C2"), bodyFont, XBrushes.Black, new XRect(page.Width - 140, y + 4, 100, 14), XStringFormats.TopLeft);
                y += 22;
            }

            var total = lines.Sum(l => l.Amount);
            y += 10;
            gfx.DrawLine(XPens.Black, page.Width - 180, y, page.Width - 40, y);
            y += 6;
            gfx.DrawString("Total", boldFont, XBrushes.Black, new XRect(page.Width - 180, y, 60, 14), XStringFormats.TopLeft);
            gfx.DrawString(total.ToString("C2"), boldFont, XBrushes.Black, new XRect(page.Width - 120, y, 100, 14), XStringFormats.TopLeft);

            document.Save(fileName);
            return Task.FromResult(fileName);
        }
    }
}
