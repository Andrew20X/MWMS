using System;
using ClosedXML.Excel;

class Program {
    static void Main() {
        try {
            using var workbook = new XLWorkbook(@"D:\MWMS\FB.xlsx");
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed();
            int i = 0;
            foreach(var row in rows) {
                if (i > 5) break;
                Console.WriteLine($"Row {i}:");
                for(int j=1; j<=10; j++) {
                    Console.Write($"[{row.Cell(j).Value}] ");
                }
                Console.WriteLine();
                i++;
            }
        } catch(Exception ex) { Console.WriteLine(ex.Message); }
    }
}
