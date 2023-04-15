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


            int includedAPIsCount = filter.ModulesToSearchWith.Count;
            int amountDefaultAttributes = 3;
            int amountSupplierAttributes = 5;
            int j = amountDefaultAttributes;
            string[] headers = new string[j + (amountSupplierAttributes * includedAPIsCount)];
            headers[0] = "HCS-Artikel-Nr";
            headers[1] = "Bedarf";
            headers[2] = "HCS H-Artikelnr";
            foreach (ModuleType moduleType in filter.ModulesToSearchWith)
            {
                Filter.ModulesTranslation.TryGetValue(moduleType, out string supplierName);

                headers[j] = "Herstellernr bei " + supplierName;
                headers[j + 1] = "HCS H-Artikelnr == " + supplierName + "-Herstellernr";
                headers[j + 2] = "Lagerbestand bei " + supplierName;
                headers[j + 4] = "Preis bei " + supplierName;
                j += amountSupplierAttributes;
            }
            ColumnedTable table = new ColumnedTable(headers);

            for (int i = 0; i < csv.FieldsLength; i++)
            {
                string hcsNr = csv.GetField(i, cparams.hcs_ArtikelnummerIndex);
                int orderAmount = ExtractIntFromOrderAmount(csv.GetField(i, cparams.bedarfIndex));
                string keyword = csv.GetField(i, cparams.h_ArtikelnummerIndex);
                table.AddNewRow();
                table.SetField(i, 0, hcsNr);
                table.SetField(i, 1, orderAmount.ToString());
                table.SetField(i, 2, keyword);
                int columnIndex = 0;

                //                                        WICHTIG NICHT VERGESSEN
                foreach (ModuleType moduleType in filter.ModulesToSearchWith)
                {
                    columnIndex += amountSupplierAttributes;
                    List<Part>? answers = await RequestHandler.SearchWith(moduleType, keyword, 3, UpdateUserCallback);
                    if (answers is null || answers.Count == 0)
                        continue;
                    
                    if (!SetTablesForOrderAmount(table, answers, orderAmount, orderAmount, i, columnIndex, amountDefaultAttributes, keyword))
                        SetTablesForOrderAmount(table, answers, 1, orderAmount, i, columnIndex, amountDefaultAttributes, keyword);
                }
            }

            using (StreamWriter sw = File.CreateText(cparams.savePath))
            {
                sw.Write(table.ConvertToCSVString(new StringBuilder()));
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

        ///<summary>returns true if an answer in answers has |parts| >= orderAmount </summary>
        private bool SetTablesForOrderAmount(ColumnedTable table, List<Part> answers, int lookUpAmount, int priceAmount, int row, int columnIndex, int amountDefaultAttributes, string keyword)
        {
            
            foreach (var answer in answers)
            {
                if (answer.AmountInStock < lookUpAmount)
                    continue;

                string manufacturerPartNumber = answer.ManufacturerPartNumber ?? "";
                int amountInStock = answer.AmountInStock ?? -1;

                table.SetField(row, amountDefaultAttributes + columnIndex, manufacturerPartNumber);
                table.SetField(row, amountDefaultAttributes + columnIndex + 1, manufacturerPartNumber.Equals(keyword) ? "TRUE" : "FALSE");
                table.SetField(row, amountDefaultAttributes + columnIndex + 2, amountInStock >= priceAmount ? amountInStock.ToString() : amountInStock.ToString() + " (!Achtung! Bestand < Bedarf)");
                table.SetField(row, amountDefaultAttributes + columnIndex + 4, FindPriceInPrices(priceAmount, answer.Prices));
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
