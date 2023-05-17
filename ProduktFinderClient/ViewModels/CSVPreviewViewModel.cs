using Microsoft.Win32;
using ProduktFinderClient.Commands;
using ProduktFinderClient.Components;
using ProduktFinderClient.CSV;
using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models;
using ProduktFinderClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProduktFinderClient.ViewModels
{

    public class CSVPreviewViewModel : ViewModelBase
    {

        #region dpds
        private int bedarfIndex = -1;
        private ObservableCollection<StringWithCommand> bedarf;
        public ObservableCollection<StringWithCommand> Bedarf
        {
            get { return bedarf; }
            set { bedarf = value; OnPropertyChanged(nameof(Bedarf)); }
        }

        private int h_ArtikelnummerIndex = -1;
        private ObservableCollection<StringWithCommand> h_Artikelnummer;
        public ObservableCollection<StringWithCommand> H_Artikelnummer
        {
            get { return h_Artikelnummer; }
            set { h_Artikelnummer = value; OnPropertyChanged(nameof(H_Artikelnummer)); }
        }

        private int hcs_ArtikelnummerIndex = -1;
        private ObservableCollection<StringWithCommand> hcs_Artikelnummer;
        public ObservableCollection<StringWithCommand> HCS_Artikelnummer
        {
            get { return hcs_Artikelnummer; }
            set { hcs_Artikelnummer = value; OnPropertyChanged(nameof(HCS_Artikelnummer)); }
        }


        private string bedarfTitel;
        public string BedarfTitel
        {
            get { return bedarfTitel; }
            set { bedarfTitel = value; OnPropertyChanged(nameof(BedarfTitel)); }
        }

        private string h_ArtikelnummerTitel;
        public string H_ArtikelnummerTitel
        {
            get { return h_ArtikelnummerTitel; }
            set { h_ArtikelnummerTitel = value; OnPropertyChanged(nameof(H_ArtikelnummerTitel)); }
        }

        private string hcs_ArtikelnummerTitel;
        public string HCS_ArtikelnummerTitel
        {
            get { return hcs_ArtikelnummerTitel; }
            set { hcs_ArtikelnummerTitel = value; OnPropertyChanged(nameof(HCS_ArtikelnummerTitel)); }
        }

        private string savePath;
        public string SavePath
        {
            get { return savePath; }
            set { savePath = value; OnPropertyChanged(nameof(SavePath)); }
        }

        private ICommand chooseSavePathCommand;
        public ICommand ChooseSavePathCommand
        {
            get { return chooseSavePathCommand; }
            set { chooseSavePathCommand = value; OnPropertyChanged(nameof(ChooseSavePathCommand)); }
        }

        private CreateCSVBetragsauskunftCommand bedarfsauskunftsCommand;
        public CreateCSVBetragsauskunftCommand BedarfsauskunftsCommand
        {
            get { return bedarfsauskunftsCommand; }
            set { bedarfsauskunftsCommand = value; OnPropertyChanged(nameof(BedarfsauskunftsCommand)); }
        }

        private SpecifiedGridObservableCollection<string> previewGrid;
        public SpecifiedGridObservableCollection<string> PreviewGrid
        {
            get { return previewGrid; }
            set { previewGrid = value; OnPropertyChanged(nameof(PreviewGrid)); }
        }

        private string buttonContent;
        public string ButtonContent
        {
            get { return buttonContent; }
            set { buttonContent = value; OnPropertyChanged(nameof(ButtonContent)); }
        }

        private bool excelOutput;
        public bool ExcelOutput
        {
            get { return excelOutput; }
            set { excelOutput = value; OnPropertyChanged(nameof(ExcelOutput)); }
        }

        #endregion

        CSVObject csv;
        public CSVPreviewViewModel(string inputFile, CSVObject csvin, MainWindowViewModel mainWindowViewModel, Func<StatusHandle> UserUpdateStatusHandleCreate)
        {
            csv = csvin;
            Bedarf = new ObservableCollection<StringWithCommand>();
            H_Artikelnummer = new ObservableCollection<StringWithCommand>();
            HCS_Artikelnummer = new ObservableCollection<StringWithCommand>();
            ExcelOutput = false;

            for (int i = 0; i < csvin.headers.Length; i++)
            {
                //Some optimization for for loops makes i in lambdas not work
                int index = i;
                Bedarf.Add(new StringWithCommand((o) => { BedarfTitel = csvin.GetHeader(index); bedarfIndex = index; OnChangePreviewGrid(); },
                    csvin.GetHeader(index)));
                H_Artikelnummer.Add(new StringWithCommand((o) => { H_ArtikelnummerTitel = csvin.GetHeader(index); h_ArtikelnummerIndex = index; OnChangePreviewGrid(); },
                    csvin.GetHeader(index)));
                HCS_Artikelnummer.Add(new StringWithCommand((o) => { HCS_ArtikelnummerTitel = csvin.GetHeader(index); hcs_ArtikelnummerIndex = index; OnChangePreviewGrid(); },
                    csvin.GetHeader(index)));
            }

            SavePath = LoadSaveSystem.configData.LastUsedSaveFile;

            BedarfTitel = SetToIfNotFound(ref bedarfIndex, "Bedarf auswählen", LoadSaveSystem.bedarfMostUsedKeywordsModule);
            H_ArtikelnummerTitel = SetToIfNotFound(ref h_ArtikelnummerIndex, "H_Artikelnummer auswählen", LoadSaveSystem.hArtikelNrMostUsedKeywordsModule);
            HCS_ArtikelnummerTitel = SetToIfNotFound(ref hcs_ArtikelnummerIndex, "HCS_Artikelnummer auswählen", LoadSaveSystem.hcsArtikelNrMostUsedKeywordsModule);


            ChooseSavePathCommand = new FastCommand((o) =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(inputFile) + "_Bedarfsauskunft.csv";

                if (saveFileDialog.ShowDialog() == true)
                {
                    SavePath = saveFileDialog.FileName;
                    LoadSaveSystem.SaveLastUsedSaveFile(saveFileDialog.FileName);
                }
            });

            BedarfsauskunftsCommand = new CreateCSVBetragsauskunftCommand("Starten", "Abbrechen", s => ButtonContent = s,
                mainWindowViewModel, csvin, UserUpdateStatusHandleCreate, TransformString, CommandParams);

            OnChangePreviewGrid();

        }
        /// <summary>
        /// For easy of use, whitespaces, '_', '-', are stripped and not case sensitive
        /// </summary>
        private string SetToIfNotFound(ref int columndIndex, string setToIfNotFound, MostUsedKeywordsModule mostUsedKeywordsModule)
        {
            string[] regexdHeaders = new string[csv.ColumnsLength];
            for (int i = 0; i < csv.ColumnsLength; i++)
                regexdHeaders[i] = TransformString(csv.GetHeader(i));

            columndIndex = -1;

            string s = mostUsedKeywordsModule.GetMostUsedKeyword(ref columndIndex, regexdHeaders);
            if (s == null)
                return setToIfNotFound;
            else
                return csv.GetHeader(columndIndex);
        }

        private string TransformString(string s)
        {
            return Regex.Replace(s, "[\\s-_]+", "", RegexOptions.Compiled).ToUpper();
        }

        private CreateCSVBetragsauskunftCommand.CommandParams CommandParams()
        {
            return new CreateCSVBetragsauskunftCommand.CommandParams(SavePath, BedarfTitel, bedarfIndex, H_ArtikelnummerTitel, h_ArtikelnummerIndex, HCS_ArtikelnummerTitel, hcs_ArtikelnummerIndex, ExcelOutput);
        }

        private void OnChangePreviewGrid()
        {
            ColumnTypeDefinition[] columns = new ColumnTypeDefinition[3];

            columns[0] = new ColumnTypeDefinition { text = bedarfIndex != -1 ? "Bedarf" : "Bedarf muss ausgewählt sein", type = ColumnType.Text };
            columns[1] = new ColumnTypeDefinition { text = h_ArtikelnummerIndex != -1 ? "H_Artikelnummer" : "h_Artikelnummer muss ausgewählt sein", type = ColumnType.Text };
            columns[2] = new ColumnTypeDefinition { text = hcs_ArtikelnummerIndex != -1 ? "HCS-Artikelnummer" : "     ", type = ColumnType.Text };

            PreviewGrid = new SpecifiedGridObservableCollection<string>(columns);

            for (int i = 0; i < csv.FieldsLength; i++)
            {
                string[] row = new string[3];
                row[0] = bedarfIndex != -1 ? csv.GetField(i, bedarfIndex) : "";
                row[1] = h_ArtikelnummerIndex != -1 ? csv.GetField(i, h_ArtikelnummerIndex) : "h_Artikelnummer muss ausgewählt sein";
                row[2] = hcs_ArtikelnummerIndex != -1 ? csv.GetField(i, hcs_ArtikelnummerIndex) : "     ";

                PreviewGrid.AddRow(row, "");
            }
        }


    }
}
