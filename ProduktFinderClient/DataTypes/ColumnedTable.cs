using System;
using System.Collections.Generic;
using System.Text;

namespace ProduktFinderClient.DataTypes
{
    class ColumnedTable
    {
        public int ColumnLength { get { return columns.Length; } }

        string[] columns;
        List<string[]> rows;

        public ColumnedTable(string[] columns)
        {
            this.columns = columns;
            rows = new List<string[]>();
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

    }
}