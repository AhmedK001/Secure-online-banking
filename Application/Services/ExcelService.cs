using Application.Interfaces;
using ClosedXML.Excel;
using Core.Entities;
using Core.Enums;

namespace Application.Services;

public class ExcelService : IExcelService
{
    private readonly IOperationService _operationService;

    public ExcelService(IOperationService operationService)
    {
        _operationService = operationService;
    }

    string FormatCurrency(EnumCurrency currency, decimal balance)
    {
        string currencySymbol = currency switch
        {
            EnumCurrency.SAR => "ريال",
            EnumCurrency.EGP => "E£",
            EnumCurrency.AED => "د.إ",
            EnumCurrency.USD => "$",
            EnumCurrency.EUR => "€",
            EnumCurrency.TRY => "₺",
            _ => currency.ToString(),
        };

        return $"{currencySymbol} {balance:F2}";
    }

    public async Task<StreamContent> GetAllOperations(bool sendEmail,List<Operation> operations, User user,
        BankAccount bankAccount)
    {
        var book = new XLWorkbook();
        var sheet = book.Worksheets.Add("sheet1");

        sheet = await BuildOperationsSheet(sheet, operations, bankAccount);

        // Style headers
        var headerRange = sheet.Range("A9:E9");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Fill.BackgroundColor = XLColor.DarkBlue;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        headerRange.Style.Border.OutsideBorderColor = XLColor.Black;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorderColor = XLColor.LightGray;
        sheet.Columns().AdjustToContents();

        return await ConvertToStreamContent(book);
    }

    public async Task<StreamContent> ConvertToStreamContent(XLWorkbook workbook)
    {
        var memStream = new MemoryStream();
        workbook.SaveAs(memStream);
        memStream.Seek(0, SeekOrigin.Begin);


        var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var streamContent = new StreamContent(memoryStream);
        streamContent.Headers.ContentType
            = new System.Net.Http.Headers.MediaTypeHeaderValue(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        streamContent.Headers.ContentDisposition
            = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = "MyBankStatements.xlsx"
            };

        return streamContent;
    }

    public async Task<IXLWorksheet> BuildOperationsSheet(IXLWorksheet sheet, List<Operation> operations,
        BankAccount bankAccount)
    {
        // Add main logo text
        string logoLocation = "A1";
        sheet.Cell(logoLocation).Value = "Secure Online Banking";

        sheet.Range($"{logoLocation}:E1").Merge();

        sheet.Cell(logoLocation).Style.Font.Bold = true;
        sheet.Cell(logoLocation).Style.Font.FontSize = 24;
        sheet.Cell(logoLocation).Style.Font.FontColor = XLColor.White;
        sheet.Cell(logoLocation).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Cell(logoLocation).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        sheet.Cell(logoLocation).Style.Fill.BackgroundColor = XLColor.DarkBlue;
        sheet.Cell(logoLocation).Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
        sheet.Cell(logoLocation).Style.Border.OutsideBorderColor = XLColor.Black;

        sheet.Row(1).Height = 30;

        var headers = new List<(string Title, string Value)>
        {
            ("Name Surname/Title", $"{bankAccount.User.FirstName} {bankAccount.User.LastName}"),
            ("Customer National ID", bankAccount.NationalId.ToString()),
            ("Account IBAN" , bankAccount.AccountNumber),
            ("Currency/Balance", FormatCurrency(bankAccount.Currency, bankAccount.Balance)),
            ("Creation Date", bankAccount.CreationDate.ToString("yyyy MMMM dd"))
        };

        // Start filling data into the sheet
        int startRow = 3;
        foreach (var (title, value) in headers)
        {
            sheet.Cell($"A{startRow}").Value = title;
            sheet.Cell($"B{startRow}").Value = value;

            sheet.Cell($"A{startRow}").Style.Font.Bold = true;
            sheet.Cell($"A{startRow}").Style.Font.FontColor = XLColor.White;
            sheet.Cell($"A{startRow}").Style.Fill.BackgroundColor = XLColor.DarkBlue;
            sheet.Cell($"A{startRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            sheet.Cell($"A{startRow}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            sheet.Cell($"B{startRow}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            sheet.Cell($"B{startRow}").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Borders
            var range = sheet.Range($"A{startRow}:B{startRow}");
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = XLColor.Black;

            startRow++;
        }

        sheet.Columns("A:B").AdjustToContents();

        int rowNumber = 9;
        sheet.Cell($"A{rowNumber}").Value = "Operation ID";
        sheet.Cell($"B{rowNumber}").Value = "Type";
        sheet.Cell($"C{rowNumber}").Value = "Amount";
        sheet.Cell($"D{rowNumber}").Value = "Date";
        sheet.Cell($"E{rowNumber}").Value = "Description";

        for (int j = 1; j < operations.Count; j++)
        {
            int i = j + 9;
            sheet.Cell($"A{i}").Value = operations[j].OperationId;
            sheet.Cell($"B{i}").Value = Enum.GetName(typeof(EnumOperationType), operations[j].OperationType);
            sheet.Cell($"C{i}").Value = FormatCurrency(operations[j].Currency, operations[j].Amount);
            sheet.Cell($"D{i}").Value = operations[j].DateTime.ToString("u");
            sheet.Cell($"E{i}").Value = operations[j].Description;
        }

        return sheet;
    }
}