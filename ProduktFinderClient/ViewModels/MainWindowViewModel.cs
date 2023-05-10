using ProduktFinderClient.Commands;
using ProduktFinderClient.Components;
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

        public string searchButtonContent;
        public string SearchButtonContent
        {
            get { return searchButtonContent; }
            set { searchButtonContent = value; OnPropertyChanged(nameof(SearchButtonContent)); }
        }

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

        private readonly List<Part> _partsReceived;
        private List<Part> partsModified;


        private readonly OptionsWindowViewModel _optionsWindowViewModel;
        private StatusBlock statusBlock;
        public MainWindowViewModel(OptionsWindowViewModel optionsWindowViewModel, StatusBlock statusBlock)
        {
            _partsReceived = new();
            _optionsWindowViewModel = optionsWindowViewModel;
            this.statusBlock = statusBlock;

            // Init the observable colletion Lieferante from the Enum ModuleTypes and Filter from RequestHandler
            ObservableCollection<string> lieferantenTranslation = new ObservableCollection<string>();
            foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
            {
                ModuleTranslations.ModulesTranslation.TryGetValue(moduleType, out string moduleString);
                lieferantenTranslation.Add(moduleString);
            }

            Lieferanten = new ObservableCollection<CheckableStringObject>
                (CheckableStringObject.StringCollectionToCheckableStringObject(lieferantenTranslation, OnPropertyChanged));


            InitLieferanten();
            SpecifiedGrid = new SpecifiedGridObservableCollection<AttributesInfo>(PartToView.columnDefinitions);

            SetHeadersActive(optionsWindowViewModel.Attributes);

            optionsWindowViewModel.PropertyChanged += OnGridSettingsChanged;

            OpenOptionsCommand = new OpenOptionsCommand(optionsWindowViewModel);
            SearchCommand = new SearchCommand("Suchen", "Abbrechen", s => SearchButtonContent = s, ClearGrid, OnSearchFinishedCallback, optionsWindowViewModel, this, GetNewStatusHandle);
            OpenCSVPreviewCommand = new OpenCSVPreviewCommand(this, GetNewStatusHandle);
            UserUpdate = "";
            this.statusBlock = statusBlock;
        }

        private void OnGridSettingsChanged(object? sender, PropertyChangedEventArgs e)
        {
            OptionsWindowViewModel options = (sender as OptionsWindowViewModel)!;


            if (e.PropertyName == "FiltersDpd")
            {
                ClearGrid();
                VisualizeGrid(sender, _partsReceived);
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


        private void OnSearchFinishedCallback(object? sender, List<Part>? rows)
        {
            if (rows is null)
                return;

            _partsReceived.AddRange(rows);
            VisualizeGrid(this, _partsReceived);
        }


        //   0          1           2               3               4           5            6
        //ProduktBild Lieferant Hersteller Hersteller-TeileNr Beschreibung Lagerbestand Mengenpreise
        private void VisualizeGrid(object? sender, List<Part>? rows)
        {
            if (rows is null)
                return;

            partsModified = new(rows);

            foreach (var checkObj in _optionsWindowViewModel.FiltersDpd)
            {
                if (!checkObj.IsChecked)
                    continue;

                PartFilters.Filter(ref partsModified, checkObj.AttributeName);
            }

            foreach (Part part in partsModified)
            {
                SpecifiedGrid.AddPart(part);
            }
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

        private StatusHandle GetNewStatusHandle()
        {
            lock (statusBlock)
            {
                return statusBlock.AddNewStatus();
            }
        }

    }
}
