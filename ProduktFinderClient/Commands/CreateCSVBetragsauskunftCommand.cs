using ProduktFinderClient.CSV;
using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models;
using ProduktFinderClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProduktFinderClient.Commands
{
    public class CreateCSVBetragsauskunftCommand : AsyncCommandBase
    {
        Action<string> UpdateUserCallback;
        MainWindowViewModel mainWindowViewModel;
        Func<string, string> RegexKeywordTransform;
        Func<CommandParams> GetCommandParamsFunc;

        CSVObject csv;

        public struct CommandParams
        {
            public string savePath;

            public string bedarfTitel;
            public int bedarfIndex;
            public string h_ArtikelnummerTitel;
            public int h_ArtikelnummerIndex;
            public string hcs_ArtikelnummerTitel;
            public int hcs_ArtikelnummerIndex;

            public CommandParams(string savePath, string bedarfTitel, int bedarfIndex, string h_ArtikelnummerTitel, int h_ArtikelnummerIndex, string hcs_ArtikelnummerTitel, int hcs_ArtikelnummerIndex)
            {
                this.savePath = savePath;
                this.bedarfTitel = bedarfTitel;
                this.bedarfIndex = bedarfIndex;
                this.h_ArtikelnummerTitel = h_ArtikelnummerTitel;
                this.h_ArtikelnummerIndex = h_ArtikelnummerIndex;
                this.hcs_ArtikelnummerTitel = hcs_ArtikelnummerTitel;
                this.hcs_ArtikelnummerIndex = hcs_ArtikelnummerIndex;
            }
        }

        public CreateCSVBetragsauskunftCommand(MainWindowViewModel mainWindowViewModel, CSVObject csv, Action<string> UpdateUserCallback, Func<string, string> RegexKeywordTransform, Func<CommandParams> GetCommandParamsFunc)
        {
            this.mainWindowViewModel = mainWindowViewModel;
            this.UpdateUserCallback = UpdateUserCallback;
            this.RegexKeywordTransform = RegexKeywordTransform;
            this.csv = csv;
            this.GetCommandParamsFunc = GetCommandParamsFunc;
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
        protected override async Task ExecuteAsync(object parameter)
        {

            CommandParams cparams = GetCommandParamsFunc();

            LoadSaveSystem.bedarfMostUsedKeywordsModule.RegisterKeyword(RegexKeywordTransform(cparams.bedarfTitel));
            LoadSaveSystem.hArtikelNrMostUsedKeywordsModule.RegisterKeyword(RegexKeywordTransform(cparams.h_ArtikelnummerTitel));
            LoadSaveSystem.hcsArtikelNrMostUsedKeywordsModule.RegisterKeyword(RegexKeywordTransform(cparams.hcs_ArtikelnummerTitel));

            LoadSaveSystem.SaveMostUsedKeywordsModules();

            // Configure which Modules we want to search with
            Filter filter = new Filter();
            foreach (var checkableString in mainWindowViewModel.Lieferanten)
            {
                if (checkableString.IsChecked)
                {
                    Filter.ModulesTranslation.TryGetKey(checkableString.AttributeName, out ModuleType moduleType);
                    filter.ModulesToSearchWith.Add(moduleType);
                }
            }

            ColumnedTable mainTable = new ColumnedTable(new string[] { "HCS-Artikel-Nr", "Bedarf", "HCS H-Artikelnr" });
            List<ColumnedTable> apiTables = new List<ColumnedTable>();


            foreach (ModuleType moduleType in filter.ModulesToSearchWith)
            {
                Filter.ModulesTranslation.TryGetValue(moduleType, out string supplierName);

                apiTables.Add(new ColumnedTable(new string[]
                {
                    "Herstellernr bei " + supplierName,
                    "HCS H-Artikelnr == " + supplierName + "-Herstellernr",
                    "Lagerbestand bei " + supplierName,
                    "Preis bei " + supplierName
                }));
            }

            // Fill main table
            for (int i = 0; i < csv.FieldsLength; i++)
            {
                string hcsNr = csv.GetField(i, cparams.hcs_ArtikelnummerIndex);
                int orderAmount = ExtractIntFromOrderAmount(csv.GetField(i, cparams.bedarfIndex));
                string keyword = csv.GetField(i, cparams.h_ArtikelnummerIndex);
                mainTable.AddNewRow();
                mainTable.SetField(i, 0, hcsNr);
                mainTable.SetField(i, 1, orderAmount.ToString());
                mainTable.SetField(i, 2, keyword);
            }

            // Fill all moduleTables
            List<Task> tasks = new();
            int tableIndex = 0;
            foreach (ModuleType moduleType in filter.ModulesToSearchWith)
            {
                ColumnedTable table = apiTables[tableIndex];
                tableIndex++;
                tasks.Add(FillTable(moduleType, table, cparams));
            }
            await Task.WhenAll(tasks);


            apiTables.Insert(0, mainTable);
            ColumnedTable combinedTable = ColumnedTable.Combine(apiTables.ToArray());



            using (StreamWriter sw = File.CreateText(cparams.savePath))
            {
                sw.Write(combinedTable.ConvertToCSVString(new StringBuilder()));
            }

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@cparams.savePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }



        private int ExtractIntFromOrderAmount(string orderAmount)
        {
            int orderAmountAsInt = StringDataExtractor.ExtractInt(orderAmount);
            return orderAmountAsInt;
        }


        private async Task FillTable(ModuleType moduleType, ColumnedTable table, CommandParams cparams)
        {
            List<Task> tasks = new();
            for (int i = 0; i < csv.FieldsLength; i++)
            {
                table.AddNewRow();
                int orderAmount = ExtractIntFromOrderAmount(csv.GetField(i, cparams.bedarfIndex));
                string keyword = csv.GetField(i, cparams.h_ArtikelnummerIndex);

                tasks.Add(ProcessRequest(moduleType, table, keyword, i, orderAmount));
                if (tasks.Count >= 5)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }

            // Important! Dont forget to await all remaining tasks
            await Task.WhenAll(tasks);
        }

        private async Task ProcessRequest(ModuleType moduleType, ColumnedTable table, string keyword, int row, int orderAmount)
        {
            List<Part>? answers = await RequestHandler.SearchWith(keyword, moduleType, 3, UpdateUserCallback);
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
}
