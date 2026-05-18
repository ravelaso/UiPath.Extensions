using Ravelaso.UiPath.Extensions.FileDataTable;
Console.WriteLine("Hello, World!");


const string path = @"\\prg-dc.dhl.com\nl_exp\NLMST\ACC_Parcel\RPA\UiPath\Testomgeving\OPS\OPS_Match_Claims_Drivers\arch\Claims per week 2026.xlsx";


var table = ExcelReader.Read(path, new()
{
    SheetName = "Week 19",
    SkipEmptyRows = true,
    UseHeaders = true
});

Console.WriteLine(table.Rows.Count);
