using System;
using System.Collections.ObjectModel;

namespace ProduktFinderClient.DataTypes
{
    public class CheckableStringObject
    {

        private readonly Action<string> OnPropertyChangedCallBack;
        private string callBackParam;

        public string AttributeName { get; set; }

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

        public static Collection<CheckableStringObject> StringCollectionToCheckableStringObject(Collection<string> collection, Action<string> OnPropertyChangedCallBack)
        {
            if (collection == null)
                return null;

            Collection<CheckableStringObject> newColl = new Collection<CheckableStringObject>();

            for (int i = 0; i < collection.Count; i++)
            {
                newColl.Add(new CheckableStringObject(OnPropertyChangedCallBack) { AttributeName = collection[i], isChecked = false });
            }

            return newColl;
        }

    }
}
