using ProduktFinderClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProduktFinderClient.DataTypes;


public class ColumnedTable
{
    public int ColumnLength { get { return columns.Length; } }
    public int RowLength { get { return rows.Count; } }

    protected string[] columns;
    protected List<string[]> rows;

    public ColumnedTable(string[] columns)
    {
        this.columns = columns;
        rows = new List<string[]>();
    }


    public string GetHeader(int column)
    {
        return columns[column];
    }
    public string GetField(int row, int column)
    {
        if (column < columns.Length && row < rows.Count)
        {

            string[] array = rows[row];
            return array[column];

        }

        else return " ";

    }
    public void AddNewRow(string[] row)
    {
        if (row == null)
            throw new NullReferenceException("Input row cant be null!");

        if (row.Length != ColumnLength)
            throw new IndexOutOfRangeException($"Input row has to have the same length as ColumnLength! Got {row.Length}  but expected {ColumnLength}");

        rows.Add(row);
    }


    public void AddNewRow()
    {
        rows.Add(new string[ColumnLength]);
    }

    public void SetField(int row, int column, string value)
    {
        rows[row][column] = value;
    }

    public string ConvertToCSVString(StringBuilder sb)
    {
        sb.Append("SEP=;\n");
        foreach (string head in columns)
        {
            sb.Append(head);
            sb.Append(";");
        }

        sb.Append("\n");


        for (int i = 0; i < rows.Count; i++)
        {
            string[] row = rows[i];


            foreach (string value in row)
            {
                sb.Append(value);
                sb.Append(";");
            }

            sb.Append("\n");
        }

        return sb.ToString();
    }

    public static ColumnedTable Combine(ColumnedTable left, ColumnedTable right)
    {
        string[] headers = left.columns.Concat(right.columns);

        ColumnedTable result = new ColumnedTable(headers);

        if (left.rows.Count != right.rows.Count)
            throw new Exception("Both tables need to have the same amount of rows!");


        for (int i = 0; i < left.rows.Count; i++)
        {
            string[] row = left.rows[i].Concat(right.rows[i]);
            result.AddNewRow(row);
        }

        return result;
    }

    public static ColumnedTable Combine(params ColumnedTable[] columnedTables)
    {
        ColumnedTable result = columnedTables[0];

        for (int i = 1; i < columnedTables.Length; i++)
        {
            result = Combine(result, columnedTables[i]);
        }

        return result;
    }

}