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

        private readonly ObservableCollection<CheckableStringObject> sortsDpd;
        public ObservableCollection<CheckableStringObject> SortsDpd
        {
            get { return sortsDpd; }
        }

        public OptionsWindowViewModel()
        {
            attributes = PartToView.GetColumnNames().ToObservableCollection(OnPropertyChanged, nameof(Attributes), true);
            filtersDpd = PartFilters.GetFilterMethodStringTranslations().ToObservableCollection(OnPropertyChanged, nameof(FiltersDpd));
            sortsDpd = PartSorts.GetSortMethodStringTranslations().ToObservableCollection(OnPropertyChanged, nameof(SortsDpd));
        }

        public void Filter(ref List<Part> parts)
        {
            foreach (var checkObj in FiltersDpd)
            {
                if (!checkObj.IsChecked)
                    continue;

                PartFilters.Filter(ref parts, checkObj.AttributeName);
            }
        }

        public void Sort(ref List<Part> parts)
        {
            foreach (var checkObj in SortsDpd)
            {
                if (!checkObj.IsChecked)
                    continue;

                PartSorts.Sort(ref parts, checkObj.AttributeName);
            }
        }

    }

}
