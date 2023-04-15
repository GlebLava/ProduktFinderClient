using System;
using System.Collections.Generic;
using System.Text;

namespace ProduktFinderClient.CSV
{

    public class CSVObject
    {
        public int ColumnsLength { get { return headers.Length; } }
        public int FieldsLength { get { return fields.Count; } }

        public string[] headers;
        public List<string[]> fields;

        public CSVObject(string[] headers, List<string[]> fields)
        {
            this.headers = headers;
            this.fields = fields;



        }

        public string GetHeader(int column)
        {
            return headers[column];
        }

        public string GetField(int row, int column)
        {
            return fields[row][column];
        }
    }


    class CSVHashObject
    {

        public int ColumnsLength { get { return headers.Length; } }
        public int FieldsLength { get { return fields.Count; } }

        public string[] headers;
        public List<string[]> fields;
        readonly Dictionary<string, int> headerToColumn;

        public CSVHashObject(string[] headers, List<string[]> fields)
        {
            this.headers = headers;
            this.fields = fields;

            headerToColumn = new Dictionary<string, int>();

            for (int i = 0; i < headers.Length; i++)
            {
                headerToColumn.Add(headers[i], i);
            }

        }

        public string GetFieldForHeader(string header, int row)
        {

            if (!headerToColumn.TryGetValue(header, out int column))
            {
                throw new KeyNotFoundException(header + " is not a header in this csv object!");
            }

            return fields[row][column];
        }

        public string GetHeader(int column)
        {
            return headers[column];
        }

        public string GetField(int row, int column)
        {
            return fields[row][column];
        }
    }
}
