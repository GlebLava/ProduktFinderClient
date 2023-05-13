using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProduktFinderClient.CSV
{
    class CSVParser
    {
        public static CSVHashObject ParseCSVHashFile(string path)
        {
            string[] headers = null;
            List<string[]> fields = new List<string[]>();

            using (TextFieldParser parser = new TextFieldParser(path, Encoding.Default))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");


                headers = parser.ReadFields();

                //First line might contain seperator
                //if it does read next line as headers
                if (headers[0].ToLower().Contains("sep"))
                    headers = parser.ReadFields();

                while (!parser.EndOfData)
                {
                    fields.Add(parser.ReadFields());
                }
            }

            return new CSVHashObject(headers, fields);
        }

        public static CSVObject ParseCSVFile(string path)
        {
            string[] headers = null;
            List<string[]> fields = new List<string[]>();

            using (TextFieldParser parser = new TextFieldParser(path, Encoding.Default))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(";");

                headers = parser.ReadFields();

                //First line might contain seperator
                //if it does read next line as headers
                if (headers[0].ToLower().Contains("sep"))
                    headers = parser.ReadFields();

                while (!parser.EndOfData)
                { 
                    fields.Add(parser.ReadFields());
                }
            }

            return new CSVObject(headers, fields);
        }

    }
}
