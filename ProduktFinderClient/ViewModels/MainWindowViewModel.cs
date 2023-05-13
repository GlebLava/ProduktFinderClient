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
        #region dpds

        public ICommand SearchCommand { get; }

        public string? searchButtonContent;
        public string? SearchButtonContent
        {
            get { return searchButtonContent; }
            set { searchButtonContent = value; OnPropertyChanged(nameof(SearchButtonContent)); }
        }

        public ICommand OpenOptionsCommand { get; }
        public ICommand OpenCSVPreviewCommand { get; }

        public string? userUpdate;
        public string? UserUpdate
        {
            get { return userUpdate; }
            set { userUpdate = value; OnPropertyChanged(nameof(UserUpdate)); }
        }


        private ObservableCollection<CheckableStringObject>? lieferanten;
        public ObservableCollection<CheckableStringObject>? Lieferanten
        {
            get { return lieferanten; }
            set { lieferanten = value; OnPropertyChanged(nameof(Lieferanten)); }
        }

        #endregion

        private readonly List<Part> _partsReceived;
        private List<Part> partsModified;


        private readonly OptionsWindowViewModel _optionsWindowViewModel;
        private readonly StatusBlock _statusBlock;
        private readonly PartsGrid _partsGrid;

        public MainWindowViewModel(OptionsWindowViewModel optionsWindowViewModel, PartsGrid partsGrid, StatusBlock statusBlock)
        {
            _partsReceived = new();
            partsModified = new();
            _optionsWindowViewModel = optionsWindowViewModel;
            _statusBlock = statusBlock;
            _partsGrid = partsGrid;


            InitLieferanten();

            optionsWindowViewModel.ApplyEvent += OnGridSettingsChanged;

            OpenOptionsCommand = new OpenOptionsCommand(optionsWindowViewModel);
            SearchCommand = new SearchCommand("Suchen", "Abbrechen", s => SearchButtonContent = s, ClearGrid, OnSearchFinishedCallback, optionsWindowViewModel, this, GetNewStatusHandle);
            OpenCSVPreviewCommand = new OpenCSVPreviewCommand(this, GetNewStatusHandle);
            UserUpdate = "";
        }

        private void OnGridSettingsChanged(object? sender, EventArgs e)
        {
            SetHeadersActive();

            partsModified = new(_partsReceived);
            _optionsWindowViewModel.Filter(ref partsModified);
            _optionsWindowViewModel.Sort(ref partsModified);

            ClearGrid();
            _partsGrid.AddPartRange(partsModified);
        }


        private void ClearGrid()
        {
            _partsGrid.Clear();
        }


        private void OnSearchFinishedCallback(object? sender, List<Part>? rows)
        {
            if (rows is null)
                return;

            _partsReceived.AddRange(rows);
            _partsGrid.AddPartRange(rows);
        }

        private void SetHeadersActive()
        {
            ObservableCollection<CheckableStringObject> newHeadersActive = _optionsWindowViewModel.Attributes;

            bool[] arr = new bool[newHeadersActive.Count];
            for (int i = 0; i < newHeadersActive.Count; i++)
            {
                arr[i] = newHeadersActive[i].IsChecked;
            }
            _partsGrid.ChangeColumnsVisibility(arr);
        }

        private void InitLieferanten()
        {
            // Init the observable colletion Lieferante from the Enum ModuleTypes and Filter from RequestHandler
            ObservableCollection<string> lieferantenTranslation = new ObservableCollection<string>();
            foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
            {
                ModuleTranslations.ModulesTranslation.TryGetValue(moduleType, out string moduleString);
                lieferantenTranslation.Add(moduleString);
            }

            Lieferanten = new ObservableCollection<CheckableStringObject>
                (CheckableStringObject.StringCollectionToCheckableStringObject(lieferantenTranslation, OnPropertyChanged));

            foreach (CheckableStringObject lieferant in Lieferanten)
                lieferant.IsChecked = true;
        }

        private StatusHandle GetNewStatusHandle()
        {
            lock (_statusBlock)
            {
                return _statusBlock.AddNewStatus();
            }
        }

    }
}
