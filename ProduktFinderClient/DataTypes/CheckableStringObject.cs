using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProduktFinderClient.DataTypes
{
    public static class CheckableStringObjectExtensions
    {
        public static ObservableCollection<CheckableStringObject> ToObservableCollection(this List<string> input, Action<string> OnPropertyChangedCallback, string OnPropertyChangedCallbackInput, bool isChecked = false)
        {
            ObservableCollection<CheckableStringObject> ret = new();

            foreach (var s in input)
            {
                ret.Add(new CheckableStringObject(OnPropertyChangedCallback, OnPropertyChangedCallbackInput) { AttributeName = s, IsChecked = isChecked });
            }

            return ret;
        }
    }


    public class CheckableStringObject
    {

        private readonly Action<string> OnPropertyChangedCallBack;
        private string callBackParam = "";

        public string AttributeName { get; set; } = "";

        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                if (callBackParam == null)
                    OnPropertyChangedCallBack(AttributeName);
                else
                    OnPropertyChangedCallBack(callBackParam);
            }
        }

        public CheckableStringObject(Action<string> OnPropertyChangedCallBack)
        {
            this.OnPropertyChangedCallBack = OnPropertyChangedCallBack;
        }

        public CheckableStringObject(Action<string> OnPropertyChangedCallBack, string callBackParam)
        {
            this.OnPropertyChangedCallBack = OnPropertyChangedCallBack;
            this.callBackParam = callBackParam;
        }


        public static Collection<CheckableStringObject> StringCollectionToCheckableStringObject(Collection<string> collection, Action<string> OnPropertyChangedCallBack, bool isCheckedInit = false)
        {
            Collection<CheckableStringObject> newColl = new();

            if (collection == null)
                return newColl;

            for (int i = 0; i < collection.Count; i++)
            {
                newColl.Add(new CheckableStringObject(OnPropertyChangedCallBack) { AttributeName = collection[i], isChecked = isCheckedInit });
            }

            return newColl;
        }

    }
}
