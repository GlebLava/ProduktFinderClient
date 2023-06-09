﻿using ProduktFinderClient.Commands;
using ProduktFinderClient.Components;
using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models;
using ProduktFinderClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ProduktFinderClient.ViewModels;

public class OptionsWindowViewModel : ViewModelBase
{
    public event EventHandler? ApplyEvent;

    #region dpds

    public ICommand ApplyCommand { get; }
    public ICommand OnOptionsWindowCloseCommand { get; }

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


    private string actualLicenseKey = "";
    private string licenseKeyDisplayed = "";

    public string LicenseKey
    {
        get { return licenseKeyDisplayed; }
        set { licenseKeyDisplayed = HandleLicenseKeyInput(value); OnPropertyChanged(nameof(LicenseKey)); SaveOptionsConfiguration(); }
    }

    public ICommand LicenseKeyVisibilityToggleCommand { get; }
    private string licenseKeyVisibilityToggleContent = "";
    public string LicenseKeyVisibilityToggleContent
    {
        get { return licenseKeyVisibilityToggleContent; }
        set { licenseKeyVisibilityToggleContent = value; OnPropertyChanged(nameof(LicenseKeyVisibilityToggleContent)); }
    }
    private bool isLicenseKeyHidden = true;
    private string hiddenString = "▬▬▬▬▬▬▬▬▬▬";

    public bool licenseKeyWindowPopupEnabled = true;
    public bool LicenseKeyWindowPopupEnabled
    {
        get { return licenseKeyWindowPopupEnabled; }
        set { licenseKeyWindowPopupEnabled = value; OnPropertyChanged(nameof(LicenseKeyWindowPopupEnabled)); SaveOptionsConfiguration(); }
    }

    public bool LicenseKeyWindowPopupDisabled
    {
        get { return !licenseKeyWindowPopupEnabled; }
        set { LicenseKeyWindowPopupEnabled = !value; OnPropertyChanged(nameof(LicenseKeyWindowPopupDisabled)); SaveOptionsConfiguration(); }
    }
    #endregion

    bool constructed = false;

    public OptionsWindowViewModel()
    {
        #region initValuesFromSave
        // Init all values from saved Options
        OptionsConfigData optionsConfigData = LoadSaveSystem.LoadOptionsConfig();

        attributes = PartsGrid.COLUMN_TITLES.ToObservableCollection(OnPropertyChangedAndSaveCallback, nameof(Attributes), true);
        attributes.CheckFrom(optionsConfigData.AttributesChecked);
        sortsDpd = PartSorts.GetSortMethodStringTranslations().ToObservableCollection(OnPropertyChangedAndSaveCallback, nameof(SortsDpd));
        sortsDpd.CheckFrom(optionsConfigData.SortsChecked);


        // all CheckableStringObjects responsible for the Filters
        FilterAvailabilityMoreThen = new CheckableStringObject(OnPropertyChangedAndSaveCallback, nameof(FilterAvailabilityMoreThen), optionsConfigData.FilterAvailabilityMoreThen);
        FilterAvailabilityLessThen = new CheckableStringObject(OnPropertyChangedAndSaveCallback, nameof(FilterAvailabilityLessThen), optionsConfigData.FilterAvailabilityLessThen);
        FilterPriceLessThenAt = new CheckableStringObject(OnPropertyChangedAndSaveCallback, nameof(FilterPriceLessThenAt), optionsConfigData.FilterPriceLessThenAt);
        PriceLessThenAtAmount = optionsConfigData.PriceLessThenAtAmount;

        actualLicenseKey = optionsConfigData.LicenseKey;
        LicenseKey = actualLicenseKey; // gets handled anyway 
        LicenseKeyWindowPopupEnabled = optionsConfigData.LicenseKeyWindowPopupEnabled;
        #endregion

        ApplyCommand = new FastCommand((o) => ApplyEvent?.Invoke(o, EventArgs.Empty));
        ApplyEvent += SaveOptionConfigurationOnApply;

        OnOptionsWindowCloseCommand = new FastCommand(OnOptionsWindowClose);

        LicenseKeyVisibilityToggleCommand = new FastCommand(ToggleLicenseKeyVisibility);
        LicenseKeyVisibilityToggleContent = "Lizensschlüssel anzeigen";
        constructed = true;
    }

    public void Filter(ref List<Part> parts)
    {
        if (FilterAvailabilityMoreThen.IsChecked)
        {
            if (int.TryParse(FilterAvailabilityMoreThen.AttributeName, out int moreThen))
            {
                PartFilters.FilterAvailableMoreThen(parts, moreThen);
            }
        }

        if (FilterAvailabilityLessThen.IsChecked)
        {
            if (int.TryParse(FilterAvailabilityLessThen.AttributeName, out int lessThen))
            {
                PartFilters.FilterAvailableMoreThen(parts, lessThen);
            }
        }

        if (FilterPriceLessThenAt.IsChecked)
        {
            if (float.TryParse(FilterPriceLessThenAt.AttributeName, out float price) && int.TryParse(PriceLessThenAtAmount, out int lessAmount))
            {
                PartFilters.FilterLessThenPriceAt(parts, price, lessAmount);
            }
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

    public string GetLicenseKey()
    {
        return actualLicenseKey;
    }

    private void OnPropertyChangedAndSaveCallback(string propertyName)
    {
        OnPropertyChanged(propertyName);
        SaveOptionsConfiguration();
    }

    private void SaveOptionConfigurationOnApply(object? o, EventArgs e)
    {
        SaveOptionsConfiguration();
        if (!isLicenseKeyHidden)
        {
            ToggleLicenseKeyVisibility(null);
        }
    }

    private void SaveOptionsConfiguration()
    {
        if (!constructed) return;

        OptionsConfigData optionsConfigData = new();
        optionsConfigData.AttributesChecked = Attributes.ToList();
        optionsConfigData.SortsChecked = SortsDpd.ToList();

        optionsConfigData.FilterAvailabilityLessThen = FilterAvailabilityLessThen;
        optionsConfigData.FilterAvailabilityMoreThen = FilterAvailabilityMoreThen;
        optionsConfigData.FilterPriceLessThenAt = FilterPriceLessThenAt;
        optionsConfigData.PriceLessThenAtAmount = PriceLessThenAtAmount;
        optionsConfigData.LicenseKey = actualLicenseKey;
        optionsConfigData.LicenseKeyWindowPopupEnabled = LicenseKeyWindowPopupEnabled;

        try
        {
            optionsConfigData.ResultsInSearchPerAPI = int.Parse(ResultsInSearchPerAPI);
        }
        catch (FormatException) { }

        LoadSaveSystem.SaveOptionsConfig(optionsConfigData);
    }


    private string HandleLicenseKeyInput(string input)
    {
        if (isLicenseKeyHidden)
        {
            return hiddenString;
        }

        if (input.Length > 16)
            input = input.Substring(0, 16);

        actualLicenseKey = input;
        return input;
    }

    private void ToggleLicenseKeyVisibility(object? input)
    {
        isLicenseKeyHidden = !isLicenseKeyHidden;
        LicenseKeyVisibilityToggleContent = isLicenseKeyHidden ? "Lizensschlüssel anzeigen" : "Lizensschlüssel verbergen";
        LicenseKey = isLicenseKeyHidden ? hiddenString : actualLicenseKey;
    }

    private void OnOptionsWindowClose(object? input)
    {
        if (!isLicenseKeyHidden) ToggleLicenseKeyVisibility(null);
    }

    public void OnWrongLicenseKeyWhileSearching()
    {
        if (LicenseKeyWindowPopupEnabled)
        {
            LicenseKeyPopup window = new();
            window.DataContext = this;
            window.Show();
        }
    }


}

