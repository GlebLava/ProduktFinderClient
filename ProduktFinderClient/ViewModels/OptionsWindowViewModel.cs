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

        public OptionsWindowViewModel(ObservableCollection<string> attributesStrings)
        {
            attributes = new ObservableCollection<CheckableStringObject>();
            filtersDpd = new ObservableCollection<CheckableStringObject>();

            for (int i = 0; i < attributesStrings.Count; i++)
            {
                attributes.Add(new CheckableStringObject(OnPropertyChanged, nameof(Attributes)) { AttributeName = attributesStrings[i], IsChecked = true });
            }

            List<string> filterNames = Filters.GetFilterMethodStringTranslations();
            foreach (string filterName in filterNames)
            {
                filtersDpd.Add(new CheckableStringObject(OnPropertyChanged, nameof(FiltersDpd)) { AttributeName = filterName, IsChecked = false });
            }

        }

    }

}
