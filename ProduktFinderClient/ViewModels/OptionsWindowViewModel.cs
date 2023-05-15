using ProduktFinderClient.Commands;
using ProduktFinderClient.Components;
using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Input;

namespace ProduktFinderClient.ViewModels;

public class OptionsWindowViewModel : ViewModelBase
{
    public event EventHandler? ApplyEvent;

    #region dpds

    public ICommand ApplyCommand { get; }

    private string resultsInSearchPerAPI = "10";
    public string ResultsInSearchPerAPI
    {
        get { return resultsInSearchPerAPI; }
        set { resultsInSearchPerAPI = value; OnPropertyChanged(nameof(ResultsInSearchPerAPI)); SaveOptionsConfiguration(); }
    }

    /*
        Since attributes and sorts basically always do the same thing, they are automatically derived from 
        PartToView and PartSorts. F.e. the name of the sort directly corresesponds to one specific sort method in PartSorts.
        If the CheckableStringObjects that holds the string of the method isChecked, it will get called from within the MainWindowViewModel
        to sort the current List of Parts.
        
        The difference for filters is that they require additional inputs. To stay flexible in the view, instead of getting chained to a specific 
        DataType class, we need to define, we will explicitly define each Filter and Hook them up in code manually.     
        F.e. how would we do the TextBox validation of our Filter Inputs manually?
        Still for some consistency and just general structure every Filter will still be defined in PartFilters
     */

    private readonly ObservableCollection<CheckableStringObject> attributes;
    public ObservableCollection<CheckableStringObject> Attributes
    {
        get { return attributes; }
    }

    private readonly ObservableCollection<CheckableStringObject> sortsDpd;
    public ObservableCollection<CheckableStringObject> SortsDpd
    {
        get { return sortsDpd; }
    }

    public CheckableStringObject FilterAvailabilityMoreThen { get; set; }
    public CheckableStringObject FilterAvailabilityLessThen { get; set; }
    public CheckableStringObject FilterPriceLessThenAt { get; set; }
    private string priceLessThenAtAmount = "";
    public string PriceLessThenAtAmount { get { return priceLessThenAtAmount; } set { priceLessThenAtAmount = value; OnPropertyChanged(nameof(PriceLessThenAtAmount)); SaveOptionsConfiguration(); } }

    #endregion


    public OptionsWindowViewModel()
    {
        OptionsConfigData optionsConfigData = LoadSaveSystem.LoadOptionsConfig();

        attributes = PartsGrid.COLUMN_TITLES.ToObservableCollection(OnPropertyChangedAndSaveCallback, nameof(Attributes), true);
        attributes.CheckFrom(optionsConfigData.AttributesChecked);
        sortsDpd = PartSorts.GetSortMethodStringTranslations().ToObservableCollection(OnPropertyChangedAndSaveCallback, nameof(SortsDpd));
        sortsDpd.CheckFrom(optionsConfigData.SortsChecked);


        // all CheckableStringObjects responsible for the Filters
        FilterAvailabilityMoreThen = new CheckableStringObject(OnPropertyChangedAndSaveCallback, nameof(FilterAvailabilityMoreThen)) { AttributeName = "0" };
        FilterAvailabilityLessThen = new CheckableStringObject(OnPropertyChangedAndSaveCallback, nameof(FilterAvailabilityLessThen)) { AttributeName = "0" };
        FilterPriceLessThenAt = new CheckableStringObject(OnPropertyChangedAndSaveCallback, nameof(FilterPriceLessThenAt)) { AttributeName = "0.0" };
        PriceLessThenAtAmount = "0";

        ApplyCommand = new FastCommand((o) => ApplyEvent?.Invoke(o, EventArgs.Empty));
    }

    public void Filter(ref List<Part> parts)
    {
        if (FilterAvailabilityMoreThen.IsChecked)
        {
            PartFilters.FilterAvailableMoreThen(parts, int.Parse(FilterAvailabilityMoreThen.AttributeName));
        }

        if (FilterAvailabilityLessThen.IsChecked)
        {
            PartFilters.FilterAvailableLessThen(parts, int.Parse(FilterAvailabilityMoreThen.AttributeName));
        }

        if (FilterPriceLessThenAt.IsChecked)
        {
            PartFilters.FilterLessThenPriceAt(parts, float.Parse(FilterPriceLessThenAt.AttributeName), int.Parse(PriceLessThenAtAmount));
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

    private void OnPropertyChangedAndSaveCallback(string propertyName)
    {
        OnPropertyChanged(propertyName);
        SaveOptionsConfiguration();
    }

    private void SaveOptionsConfiguration()
    {
        OptionsConfigData optionsConfigData = new();

        if (Attributes is not null) optionsConfigData.AttributesChecked = Attributes.ToList();
        if (SortsDpd is not null) optionsConfigData.SortsChecked = SortsDpd.ToList();

        try
        {
            optionsConfigData.ResultsInSearchPerAPI = int.Parse(ResultsInSearchPerAPI);
        }
        catch (FormatException) { }

        LoadSaveSystem.SaveOptionsConfig(optionsConfigData);
    }

}

