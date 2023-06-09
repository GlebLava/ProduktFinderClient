﻿using ProduktFinderClient.DataTypes;
using System.Collections.Generic;

namespace ProduktFinderClient.Models;

public class ConfigData
{
    public string LastUsedSaveFile { get; set; } = "";
    public HashSet<ModuleType> SuppliersChecked { get; set; } = new HashSet<ModuleType>();
    public string LastSearchedForKeyWord { get; set; } = "bc847c";
    public OptionsConfigData OptionsConfigData { get; set; } = new();

    public int GlobalFontSize { get; set; } = 20;
    public int PartGridFontSize { get; set; } = 20;
}

public class OptionsConfigData
{
    public int ResultsInSearchPerAPI { get; set; } = 10;
    public List<CheckableStringObject> AttributesChecked { get; set; } = new List<CheckableStringObject>();
    public List<CheckableStringObject> SortsChecked { get; set; } = new List<CheckableStringObject>();

    public CheckableStringObject FilterAvailabilityMoreThen { get; set; } = new() { AttributeName = "0" }; // Important dont forget
    public CheckableStringObject FilterAvailabilityLessThen { get; set; } = new() { AttributeName = "0" }; // Important dont forget
    public CheckableStringObject FilterPriceLessThenAt { get; set; } = new CheckableStringObject() { isChecked = false, AttributeName = "0.0" };
    public string PriceLessThenAtAmount { get; set; } = "0";
    public string LicenseKey { get; set; } = "0000000000000000";
    public bool LicenseKeyWindowPopupEnabled { get; set; } = true;
}

