using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace ProduktFinderClient.ViewModels
{
    public class OptionsWindowViewModel : ViewModelBase
    {
        private string resultsInSearchPerAPI = "10";
        public string ResultsInSearchPerAPI
        {
            get { return resultsInSearchPerAPI; }
            set { resultsInSearchPerAPI = value; OnPropertyChanged(nameof(ResultsInSearchPerAPI)); }
        }

        private readonly ObservableCollection<CheckableStringObject> attributes;
        public ObservableCollection<CheckableStringObject> Attributes
        {
            get { return attributes; }
        }

        private readonly ObservableCollection<CheckableStringObject> filtersDpd;
        public ObservableCollection<CheckableStringObject> FiltersDpd
        {
            get { return filtersDpd; }
        }

        public OptionsWindowViewModel()
        {
            attributes = PartToView.GetColumnNames().ToObservableCollection(OnPropertyChanged, nameof(Attributes), true);
            filtersDpd = PartFilters.GetFilterMethodStringTranslations().ToObservableCollection(OnPropertyChanged, nameof(FiltersDpd));
        }

    }

}
