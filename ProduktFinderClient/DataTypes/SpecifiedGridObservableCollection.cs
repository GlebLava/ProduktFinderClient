using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ProduktFinderClient.DataTypes
{
    public class SpecifiedGridObservableCollection<T> : INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly ColumnTypeDefinition[] headers;
        private readonly List<string[]> grid;
        private readonly List<T> additionalInfo;

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }
            remove
            {
                PropertyChanged -= value;
            }

        }

        public SpecifiedGridObservableCollection(ColumnTypeDefinition[] headers)
        {
            this.headers = headers;
            grid = new List<string[]>();
            additionalInfo = new List<T>();
        }

        public void Clear()
        {
            grid.Clear();
            additionalInfo.Clear();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        public void AddRow(string[] row, T info)
        {
            if (row.Length != headers.Length)
                throw new IndexOutOfRangeException("Trying to add a new Row with wrong column size");

            grid.Add(row);
            additionalInfo.Add(info);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, Tuple.Create<string[], T>(row, info), grid.Count - 1);
        }

        public string[] GetRow(int rowIndex)
        {
            return grid[rowIndex];
        }

        public T GetRowInfo(int rowIndex)
        {
            return additionalInfo[rowIndex];
        }

        public ColumnType GetColumnsType(int column)
        {
            return headers[column].type;
        }

        public int Count
        {
            get { return grid.Count; }
        }

        public string[] GetHeaders()
        {
            string[] sHeaders = new string[headers.Length];
            for (int i = 0; i < headers.Length; i++)
            {
                sHeaders[i] = headers[i].text;
            }

            return sHeaders;
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        #region Private Methods
        /// <summary>
        /// Helper to raise a PropertyChanged event  />).
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        #endregion Private Methods

        private const string CountString = "Count";

        // This must agree with Binding.IndexerName.  It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private const string IndexerName = "Item[]";
    }

    public struct ColumnTypeDefinition
    {
        public string text;
        public ColumnType type;

        public ColumnTypeDefinition(string text, ColumnType type)
        {
            this.text = text;
            this.type = type;
        }
    }

    public enum ColumnType
    {
        DontDisplay, Image, Hyperlink, Text
    }
}
