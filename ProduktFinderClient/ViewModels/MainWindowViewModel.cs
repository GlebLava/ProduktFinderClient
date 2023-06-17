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

        public string searchInput = string.Empty;
        public string SearchInput
        {
            get { return searchInput; }
            set { searchInput = value; OnPropertyChanged(nameof(SearchInput)); }
        }

        public string searchButtonContent = string.Empty;
        public string SearchButtonContent
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
            set { lieferanten = value; OnPropertyChanged(nameof(Lieferanten)); OnSettingToSaveChanged(); }
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
            OnGridSettingsChanged(null, EventArgs.Empty);

            OpenOptionsCommand = new OpenOptionsCommand(optionsWindowViewModel);
            SearchCommand = new SearchCommand("Suchen", "Abbrechen", s => SearchButtonContent = s, ClearGrid, OnSearchFinishedCallback, optionsWindowViewModel, this, GetNewStatusHandle);
            OpenCSVPreviewCommand = new OpenCSVPreviewCommand(this, optionsWindowViewModel, GetNewStatusHandle);
            UserUpdate = "";
            SearchInput = LoadSaveSystem.LoadLastSearchedKeyWord();
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

        /// <summary>
        /// Initialises the collection Lieferanten by using ModuleTranslations.
        /// Then checks all lieferanten which were saved from the last session
        /// 
        /// This and OnSettingToSaveChanged have some potential to be optimized if needed
        /// </summary>
        private void InitLieferanten()
        {
            // Init the observable colletion Lieferante from the Enum ModuleTypes and Filter from RequestHandler
            // this is for ease of use, so if new ModuleTypes are added, we just need to update the translation in one Place, in ModuleTranslations
            // rest happens automatically.

            ObservableCollection<string> lieferantenTranslation = new ObservableCollection<string>();
            foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
            {
                ModuleTranslations.ModulesTranslation.TryGetValue(moduleType, out string moduleString);
                lieferantenTranslation.Add(moduleString);
            }


            // HERE WE DELIBERATLY CALL THE PRIVATE PART OF lieferanten
            // The problem is if we call Lieferanten, OnSettingToSaveChanged would be called,
            // before we load our settings from the previous session.
            // So Lieferanten would be initialised with all Checkboxes checked to false
            // then we would save these settings immediately, and then we would load the just saved settings
            lieferanten = new ObservableCollection<CheckableStringObject>
                (CheckableStringObject.StringCollectionToCheckableStringObject(lieferantenTranslation,
                (s) => { OnPropertyChanged(s); OnSettingToSaveChanged(); }, // Everytime a checkbox is checked or unchecked, we also want the Settings to be saved
                false)); // Since we later check all Suppliers that are in the saved Hashset, we need the default to be false

            // Here we initialise which Supplier is checked based on our Save from last session
            HashSet<ModuleType> savedFromLastSession = LoadSaveSystem.LoadCheckedSuppliers();
            foreach (ModuleType savedSupplierFromLastSession in savedFromLastSession)
            {
                string savedFromLastSessionToString;
                ModuleTranslations.ModulesTranslation.TryGetValue(savedSupplierFromLastSession, out savedFromLastSessionToString);

                if (lieferanten.Find((cs) => cs.AttributeName == savedFromLastSessionToString, out CheckableStringObject? output))
                {
                    output!.IsChecked = true;
                }
            }

            Lieferanten = lieferanten;
        }

        private StatusHandle GetNewStatusHandle()
        {
            lock (_statusBlock)
            {
                return _statusBlock.AddNewStatus();
            }
        }

        /// <summary>
        /// This method is responsible for saving all Settings worthy to save
        /// </summary>
        private void OnSettingToSaveChanged()
        {
            if (Lieferanten is not null)
            {
                HashSet<ModuleType> thisSessionsSuppliersToSave = new();
                foreach (CheckableStringObject checkableString in Lieferanten)
                {
                    if (!checkableString.IsChecked)
                        continue;

                    ModuleTranslations.ModulesTranslation.TryGetKey(checkableString.AttributeName, out ModuleType module);
                    thisSessionsSuppliersToSave.Add(module);
                }

                LoadSaveSystem.SaveCheckedSuppliers(thisSessionsSuppliersToSave);
            }
        }
    }
}
