using ProduktFinderClient.DataTypes;
using System.Collections.ObjectModel;

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

        private readonly ObservableCollection<CheckableStringObject> filters;
        public ObservableCollection<CheckableStringObject> Filters
        {
            get { return filters; }
        }

        public OptionsWindowViewModel(ObservableCollection<string> attributesStrings, ObservableCollection<string> filterStrings)
        {
            attributes = new ObservableCollection<CheckableStringObject>();
            filters = new ObservableCollection<CheckableStringObject>();

            for (int i = 0; i < attributesStrings.Count; i++)
            {
                attributes.Add(new CheckableStringObject(OnPropertyChanged, nameof(Attributes)) { AttributeName = attributesStrings[i], IsChecked = true });
            }

            for (int i = 0; i < filterStrings.Count; i++)
            {
                filters.Add(new CheckableStringObject(OnPropertyChanged, nameof(Filters)) { AttributeName = filterStrings[i], IsChecked = false });
            }

        }

    }

}
