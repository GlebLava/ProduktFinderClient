using OfficeOpenXml;
using ProduktFinderClient.Components;
using ProduktFinderClient.CSV;
using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models;
using ProduktFinderClient.Models.ErrorLogging;
using ProduktFinderClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ProduktFinderClient.Commands;

public class CreateCSVBetragsauskunftCommand : AsyncCancelCommandBase
{
    Func<StatusHandle> UserUpdateStatusHandleCreate;
    MainWindowViewModel mainWindowViewModel;
    OptionsWindowViewModel optionsWindowViewModel;
    Func<string, string> RegexKeywordTransform;
    Func<CommandParams> GetCommandParamsFunc;

    public class CommandParams
    {
        public class Data
        {
            public string h_Artikelnummer = "";
            public string hcs_Artikelnummer = "";
            public int bedarf;
        }

        public List<Data>? datas;

        public string savePath = "";

        public string bedarfTitel = "";
        public string h_ArtikelnummerTitel = "";
        public string hcs_ArtikelnummerTitel = "";
        public bool useExcelOutput = false;
    }

    public CreateCSVBetragsauskunftCommand(string normalText, string cancelText, Action<string> SetButtonContent,
        MainWindowViewModel mainWindowViewModel, OptionsWindowViewModel optionsWindowViewModel, Func<StatusHandle> UserUpdateStatusHandleCreate, Func<string, string> RegexKeywordTransform, Func<CommandParams> GetCommandParamsFunc)
        : base(normalText, cancelText, SetButtonContent)
    {
        this.mainWindowViewModel = mainWindowViewModel;
        this.optionsWindowViewModel = optionsWindowViewModel;
        this.UserUpdateStatusHandleCreate = UserUpdateStatusHandleCreate;
        this.RegexKeywordTransform = RegexKeywordTransform;
        this.GetCommandParamsFunc = GetCommandParamsFunc;
    }


    protected override async Task ExecuteAsync(object? parameter, CancellationToken cancellationToken)
    {
        try
        {
            await CreateCSVBedarfsAuskunft(cancellationToken);
        }
        catch (Exception e)
        {
            ErrorLogger.LogError(e, "");
        }
    }

    /// <summary>
    /// This method works the following way. The Command gets the input CSV Table (the table that the user opens in CSV Bedarfsauskunft) as the input
    /// When the user chooses which columns are wanted, the indexes of these columns are saved in cparams.
    /// This way we know where to look for in the csv table for the inputs
    /// 
    /// Afterwards we create one ColumnedTable for the headers. This one always exists. It represents all the left columns (the inputs)
    /// And for each api we create a ColumnedTable seperatly. Each one gets filled with all the answers individually and async. That way each table can get filled on its own.
    /// Afterwards all tables are combined. ColumnedTable.Combine, combines the tables in Order from left to right, such that the columns in the resulting table are  side by side
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    private async Task CreateCSVBedarfsAuskunft(CancellationToken cancellationToken)
    {
        CommandParams cparams = GetCommandParamsFunc();
        if (cparams.datas is null) throw new Exception("cparams.data cant be null!");


        if (File.Exists(cparams.savePath))
        {
            if (MessageBox.Show($"Die Datei {cparams.savePath} existiert schon." +
                $" Soll diese überschrieben werden?", "Achtung", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
        }


        LoadSaveSystem.bedarfMostUsedKeywordsModule.RegisterKeyword(RegexKeywordTransform(cparams.bedarfTitel));
        LoadSaveSystem.hArtikelNrMostUsedKeywordsModule.RegisterKeyword(RegexKeywordTransform(cparams.h_ArtikelnummerTitel));
        LoadSaveSystem.hcsArtikelNrMostUsedKeywordsModule.RegisterKeyword(RegexKeywordTransform(cparams.hcs_ArtikelnummerTitel));

        LoadSaveSystem.SaveMostUsedKeywordsModules();

        // Configure which Modules we want to search with
        ModuleTranslations filter = new ModuleTranslations();
        foreach (var checkableString in mainWindowViewModel.Lieferanten)
        {
            if (checkableString.IsChecked)
            {
                ModuleTranslations.ModulesTranslation.TryGetKey(checkableString.AttributeName, out ModuleType moduleType);
                filter.ModulesToSearchWith.Add(moduleType);
            }
        }

        ColumnedTable mainTable = new ColumnedTable(new string[] { "HCS-Artikel-Nr", "Bedarf", "HCS H-Artikelnr" });
        List<ColumnedTable> apiTables = new List<ColumnedTable>();


        foreach (ModuleType moduleType in filter.ModulesToSearchWith)
        {
            ModuleTranslations.ModulesTranslation.TryGetValue(moduleType, out string supplierName);

            apiTables.Add(new ColumnedTable(new string[]
            {
                "Herstellernr bei " + supplierName,
                "HCS H-Artikelnr == " + supplierName + "-Herstellernr",
                "Lagerbestand bei " + supplierName,
                "Preis bei " + supplierName
            }));
        }


        for (int i = 0; i < cparams.datas.Count; i++)
        {
            var data = cparams.datas[i];

            mainTable.AddNewRow();
            mainTable.SetField(i, 0, data.hcs_Artikelnummer ?? "");
            mainTable.SetField(i, 1, data.bedarf.ToString());
            mainTable.SetField(i, 2, data.h_Artikelnummer);
        }

        // Fill all moduleTables
        List<Task> tasks = new();
        int tableIndex = 0;
        foreach (ModuleType moduleType in filter.ModulesToSearchWith)
        {
            ColumnedTable table = apiTables[tableIndex];
            tableIndex++;
            tasks.Add(FillTable(moduleType, table, cparams, cancellationToken));
        }
        await Task.WhenAll(tasks);


        if (cancellationToken.IsCancellationRequested)
            return;

        apiTables.Insert(0, mainTable);
        ColumnedTable combinedTable = ColumnedTable.Combine(apiTables.ToArray());

        string filePath = cparams.savePath;
        if (cparams.useExcelOutput)
        {
            filePath = Path.ChangeExtension(filePath, "xlsx");
            WriteAndOpenExcelFileWithRenameIfFileIsInUse(filePath, combinedTable);
        }
        else
        {
            filePath = Path.ChangeExtension(filePath, "csv");
            WriteAndOpenCSVFileWithRenameIfFileIsInUse(filePath, combinedTable.ConvertToCSVString(new StringBuilder()));
        }
    }


    /// <summary>
    /// If the file at filePath already exists but is not in use, it will be overwritten
    /// If the file at filePath already exists but is in use, a new fileName with an incrementing index will be used
    /// fileNames with the same incremented index name that are not in use will NOT be overwritten.
    /// </summary>
    /// <param name="filePath">Full path of the file with name and extension to write into</param>
    /// <param name="content">What to write into the file</param>
    private void WriteAndOpenCSVFileWithRenameIfFileIsInUse(string filePath, string content)
    {
        int appendCount = 0;
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        string dir = Path.GetDirectoryName(filePath)!;


        while (appendCount < 50)
        {
            try
            {
                string newFilePath = Path.Combine(dir, fileName + extension);

                if (appendCount == 0 || !File.Exists(newFilePath))
                {
                    using (StreamWriter sw = File.CreateText(newFilePath))
                    {
                        sw.Write(content);
                    }

                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(newFilePath)
                        {
                            UseShellExecute = true
                        }
                    };
                    p.Start();
                    return;
                }
            }
            catch (IOException)
            {
            }
            finally
            {
                fileName = Path.GetFileNameWithoutExtension(filePath) + appendCount++.ToString();
            }
        }
    }

    private void WriteAndOpenExcelFileWithRenameIfFileIsInUse(string filePath, ColumnedTable table)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Create a new Excel package
        using (var package = new ExcelPackage())
        {
            // Add a new worksheet to the Excel package
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");


            int columns = table.ColumnLength;
            for (int i = 0; i < columns; i++)
            {
                worksheet.Cells[1, i + 1].Value = table.GetHeader(i);
            }

            for (int row = 0; row < table.RowLength; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = table.GetField(row, col);
                }
            }

            // Save the Excel package to a file
            package.SaveAs(filePath);
        }

        // ÖFFNET EXCEL FILE, AM PFAD filePath
        ProcessStartInfo startInfo = new(filePath);
        startInfo.UseShellExecute = true;
        Process process = new();
        process.StartInfo = startInfo;
        process.Start();
    }

    private async Task FillTable(ModuleType moduleType, ColumnedTable table, CommandParams cparams, CancellationToken cancellationToken)
    {

        List<Task> tasks = new();
        for (int i = 0; i < cparams.datas!.Count && !cancellationToken.IsCancellationRequested; i++)
        {
            table.AddNewRow();
            int orderAmount = cparams.datas[i].bedarf;
            string keyword = cparams.datas[i].h_Artikelnummer;

            tasks.Add(ProcessRequest(moduleType, table, keyword, i, orderAmount, cancellationToken));
            if (tasks.Count >= 3)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }

        // Important! Dont forget to await all remaining tasks
        await Task.WhenAll(tasks);
    }

    private async Task ProcessRequest(ModuleType moduleType, ColumnedTable table, string keyword, int row, int orderAmount, CancellationToken cancellationToken)
    {
        StatusHandle statusHandle = UserUpdateStatusHandleCreate();

        List<Part>? answers = await RequestHandler.SearchWith(optionsWindowViewModel.GetLicenseKey(), keyword, moduleType, 3, statusHandle, cancellationToken);
        if (answers is null || answers.Count == 0)
            return;

        if (!SetTablesForOrderAmount(table, answers, orderAmount, orderAmount, row, keyword))
            SetTablesForOrderAmount(table, answers, 1, orderAmount, row, keyword);
    }



    ///<summary>returns true if an answer in answers has |parts| >= orderAmount </summary>
    private bool SetTablesForOrderAmount(ColumnedTable table, List<Part> answers, int lookUpAmount, int priceAmount, int row, string keyword)
    {

        foreach (var answer in answers)
        {
            if (answer.AmountInStock < lookUpAmount)
                continue;

            string manufacturerPartNumber = answer.ManufacturerPartNumber ?? "";
            int amountInStock = answer.AmountInStock ?? -1;

            table.SetField(row, 0, manufacturerPartNumber);
            table.SetField(row, 1, manufacturerPartNumber.Equals(keyword) ? "TRUE" : "FALSE");
            table.SetField(row, 2, amountInStock >= priceAmount ? amountInStock.ToString() : amountInStock.ToString() + " (!Achtung! Bestand < Bedarf)");
            table.SetField(row, 3, FindPriceInPrices(priceAmount, answer.Prices));
            return true;
        }

        return false;
    }

    private string FindPriceInPrices(int orderAmount, List<Price> prices)
    {
        if (prices == null)
            return "NULL";

        try
        {
            for (int i = 0; i < prices.Count - 1; i++)
            {
                if (orderAmount >= prices[i].FromAmount && orderAmount < prices[i + 1].FromAmount)
                    return prices[i].PricePerPiece + " " + prices[i].Currency;
            }

            return prices[prices.Count - 1].PricePerPiece + " " + prices[prices.Count - 1].Currency;
        }
        catch (Exception)
        {
            return "NULL";
        }
    }


}
