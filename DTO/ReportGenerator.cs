using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class ReportGenerator
{
    public static byte[] GenerateFullPdf(
        Employee employee,
        List<Attendance> attendances,
        List<Advance> advances,
        decimal grossSalary,
        decimal totalAdvance,
        decimal netSalary)
    {
        using (var ms = new MemoryStream())
        {
            // Create document
            var doc = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // Fonts
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.Green);
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            // Title
            var title = new Paragraph($"Monthly Employee Report - {employee.Name}", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);
            doc.Add(new Paragraph($"Employee ID: {employee.EmployeeId}", normalFont));
            doc.Add(new Paragraph($"Month/Year: {DateTime.Now:MM/yyyy}", normalFont));
            doc.Add(new Paragraph($"Generated on: {DateTime.Now:dd MMM yyyy}", normalFont));
            doc.Add(new Paragraph(" "));

            // Attendance Summary Table
            PdfPTable attendanceTable = new PdfPTable(2);
            attendanceTable.WidthPercentage = 50;
            attendanceTable.SetWidths(new float[] { 2, 1 });

            attendanceTable.AddCell(new PdfPCell(new Phrase("Attendance Summary", headerFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.Green });
            attendanceTable.AddCell("Present Days");
            attendanceTable.AddCell(attendances.Count(a => a.Status == "Present").ToString());
            attendanceTable.AddCell("Half Days");
            attendanceTable.AddCell(attendances.Count(a => a.Status == "Half-day").ToString());
            attendanceTable.AddCell("Absent / Leave");
            attendanceTable.AddCell(attendances.Count(a => a.Status == "Absent" || a.Status == "Leave").ToString());
            doc.Add(attendanceTable);
            doc.Add(new Paragraph(" "));

            // Salary Summary Table
            PdfPTable salaryTable = new PdfPTable(2);
            salaryTable.WidthPercentage = 50;
            salaryTable.SetWidths(new float[] { 2, 1 });

            salaryTable.AddCell(new PdfPCell(new Phrase("Salary Summary", headerFont)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            salaryTable.AddCell("Gross Salary");
            salaryTable.AddCell($"₹{grossSalary:N2}");
            salaryTable.AddCell("Advance Deduction");
            salaryTable.AddCell($"₹{totalAdvance:N2}");
            salaryTable.AddCell("Net Salary");
            salaryTable.AddCell($"₹{netSalary:N2}");
            doc.Add(salaryTable);
            doc.Add(new Paragraph(" "));

            // Advance Details Table
            PdfPTable advanceTable = new PdfPTable(3);
            advanceTable.WidthPercentage = 100;
            advanceTable.SetWidths(new float[] { 2, 1, 2 });

            advanceTable.AddCell(new PdfPCell(new Phrase("Advance Details", headerFont)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LightGray });
            advanceTable.AddCell("Date");
            advanceTable.AddCell("Amount");
            advanceTable.AddCell("Reason");

            foreach (var adv in advances)
            {
                advanceTable.AddCell(adv.DateGiven.ToString("dd MMM yyyy"));
                advanceTable.AddCell($"₹{adv.Amount:N2}");
                advanceTable.AddCell(string.IsNullOrEmpty(adv.Reason) ? "N/A" : adv.Reason);
            }

            doc.Add(advanceTable);

            // Close document
            doc.Close();
            return ms.ToArray();
        }
    }
}
