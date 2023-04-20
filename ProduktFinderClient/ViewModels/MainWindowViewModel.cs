using ProduktFinderClient.Commands;
using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProduktFinderClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        public ICommand SearchCommand { get; }
        public ICommand OpenOptionsCommand { get; }
        public ICommand OpenCSVPreviewCommand { get; }

        public string userUpdate;
        public string UserUpdate
        {
            get { return userUpdate; }
            set { userUpdate = value; OnPropertyChanged(nameof(UserUpdate)); }
        }


        private SpecifiedGridObservableCollection<AttributesInfo> specifiedGrid;
        public SpecifiedGridObservableCollection<AttributesInfo> SpecifiedGrid
        {
            get { return specifiedGrid; }
            set { specifiedGrid = value; OnPropertyChanged(nameof(SpecifiedGrid)); }
        }

        private ObservableCollection<CheckableStringObject> lieferanten;
        public ObservableCollection<CheckableStringObject> Lieferanten
        {
            get { return lieferanten; }
            set { lieferanten = value; OnPropertyChanged(nameof(Lieferanten)); }
        }

        //Needs to be set to a new object to trigger
        private bool[] headersActive;
        public bool[] HeadersActive
        {
            get { return headersActive; }
            set { headersActive = value; OnPropertyChanged(nameof(HeadersActive)); }
        }

        private List<Part> rows;

        public MainWindowViewModel(OptionsWindowViewModel optionsWindowViewModel)
        {
            // Init the observable colletion Lieferante from the Enum ModuleTypes and Filter from RequestHandler
            ObservableCollection<string> lieferanten = new ObservableCollection<string>();
            foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
            {
                Filter.ModulesTranslation.TryGetValue(moduleType, out string moduleString);
                lieferanten.Add(moduleString);
            }  

            Lieferanten = new ObservableCollection<CheckableStringObject>
                (CheckableStringObject.StringCollectionToCheckableStringObject(lieferanten, OnPropertyChanged));

            InitLieferanten();

            SpecifiedGrid = new SpecifiedGridObservableCollection<AttributesInfo>(App.columnDefinitions);

            SetHeadersActive(optionsWindowViewModel.Attributes);

            optionsWindowViewModel.PropertyChanged += OnGridSettingsChanged;

            OpenOptionsCommand = new OpenOptionsCommand(optionsWindowViewModel);
            SearchCommand = new SearchCommand(ClearGrid, VisualizeGrid, optionsWindowViewModel, this, SetUserUpdate);
            OpenCSVPreviewCommand = new OpenCSVPreviewCommand(this, SetUserUpdate);
            UserUpdate = "";
        }

        private void OnGridSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            OptionsWindowViewModel options = sender as OptionsWindowViewModel;

            if (e.PropertyName == "Filters")
            {
                VisualizeGrid(sender, rows);
            }
            else if (e.PropertyName == "Attributes")
            {
                SetHeadersActive(options.Attributes);
            }
        }


        private void ClearGrid()
        {
            SpecifiedGrid.Clear();
        }

        //   0          1           2               3               4           5            6
        //ProduktBild Lieferant Hersteller Hersteller-TeileNr Beschreibung Lagerbestand Mengenpreise
        private void VisualizeGrid(object sender, List<Part>? rows)
        {
            if (rows is null)
                return;

            this.rows = rows;

            for (int i = 0; i < rows.Count; i++)
            {
                string[] newRow = new string[App.AMOUNT_OF_ATTRIBUTES];

                newRow[0] = rows[i].ImageUrl;
                newRow[1] = rows[i].Supplier;
                newRow[2] = rows[i].Manufacturer;
                newRow[3] = rows[i].ManufacturerPartNumber;
                newRow[4] = rows[i].Description;
                newRow[5] = (rows[i].AmountInStock is null || rows[i].AmountInStock == -1) ? "keine Angabe" : rows[i].AmountInStock.ToString();
                newRow[6] = ConstructPrices(rows[i].Prices);

                AttributesInfo attributesInfo = new AttributesInfo()
                {
                    hLink = rows[i].Hyperlink
                };

                SpecifiedGrid.AddRow(newRow, attributesInfo);
            }

        }

        private string ConstructPrices(List<Price> prices)
        {
            if (prices == null || prices.Count == 0)
                return "keine Angabe";

            string s = "";

            foreach (Price price in prices)
            {
                if (price.FromAmount == -1 || price.PricePerPiece == -1.0f)
                    continue;

                s += "Ab " + price.FromAmount + " Stück " + price.PricePerPiece + " " + price.Currency + "\n";
            }

            return s;
        }

        private void SetHeadersActive(ObservableCollection<CheckableStringObject> newHeadersActive)
        {
            bool[] arr = new bool[newHeadersActive.Count];
            for (int i = 0; i < newHeadersActive.Count; i++)
            {
                arr[i] = newHeadersActive[i].IsChecked;
            }
            HeadersActive = arr;
        }

        private void InitLieferanten()
        {
            foreach (CheckableStringObject lieferant in Lieferanten)
                lieferant.IsChecked = true;
        }


        private void SetUserUpdate(string? update)
        {
            _ = SetUserUpdateAsync(update);
        }

        private async Task SetUserUpdateAsync(string? update)
        {
            if (update is null) return;

            DateTime currentTime = DateTime.Now;
            string formattedTime = currentTime.ToString("HH:mm");

            await Task.Delay(20);
            UserUpdate = UserUpdate.Insert(0, $"<{formattedTime}> {update}\n");

            if (UserUpdate.Length > 1024)
                UserUpdate = UserUpdate.Remove(1024);

        }



    }
}
