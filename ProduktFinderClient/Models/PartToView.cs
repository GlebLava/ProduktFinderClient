using ProduktFinderClient.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProduktFinderClient.Models;

public static class PartToView
{
    public static readonly ColumnTypeDefinition[] columnDefinitions = {
            new ColumnTypeDefinition("Produkt Bild", ColumnType.Image),
            new ColumnTypeDefinition("Lieferant", ColumnType.Hyperlink),
            new ColumnTypeDefinition("Hersteller", ColumnType.Text),
            new ColumnTypeDefinition("Hersteller-TeileNr.", ColumnType.Text),
            new ColumnTypeDefinition("Beschreibung", ColumnType.Text),
            new ColumnTypeDefinition("Verfügbarkeit", ColumnType.Text),
            new ColumnTypeDefinition("Mengenpreise", ColumnType.Text) };
    public static readonly int AMOUNT_OF_ATTRIBUTES = columnDefinitions.Length;


    public static List<string> GetColumnNames()
    {
        List<string> columnNames = new();
        foreach (var columnDefinition in columnDefinitions)
        {
            columnNames.Add(columnDefinition.text);
        }

        return columnNames;
    }

    public static void AddPart(this SpecifiedGridObservableCollection<AttributesInfo> specifiedGrid, Part part)
    {
        AddPartToSpecifiedGrid(specifiedGrid, part);

    }

    public static void AddPartToSpecifiedGrid(SpecifiedGridObservableCollection<AttributesInfo> specifiedGrid, Part part)
    {
        string[] newRow = new string[AMOUNT_OF_ATTRIBUTES];

        newRow[0] = part.ImageUrl ?? "";
        newRow[1] = part.Supplier ?? "";
        newRow[2] = part.Manufacturer ?? "";
        newRow[3] = part.ManufacturerPartNumber ?? "";
        newRow[4] = part.Description ?? "";

        string amountInStock = "keineAngabe";
        if (part.AmountInStock is not null && part.AmountInStock != -1)
            amountInStock = part.AmountInStock.ToString()!;

        newRow[5] = amountInStock;
        newRow[6] = ConstructPrices(part.Prices);

        AttributesInfo attributesInfo = new()
        {
            hLink = part.Hyperlink ?? "",
        };

        specifiedGrid.AddRow(newRow, attributesInfo);
    }

    private static string ConstructPrices(List<Price> prices)
    {
        if (prices == null || prices.Count == 0)
            return "keine Angabe";

        string s = "";

        foreach (Price price in prices)
        {
            if (price.FromAmount == -1 || price.PricePerPiece == -1.0f)
                continue;

            s += "Ab " + price.FromAmount + " Stück " + price.PricePerPiece + " " + price.Currency + "\n";
        }

        return s;
    }



}
