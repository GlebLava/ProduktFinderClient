using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        public static List<CheckableStringObject> ToList(this ObservableCollection<CheckableStringObject> input)
        {
            List<CheckableStringObject> ret = new();
            foreach (var checkableStringObject in input)
            {
                ret.Add(checkableStringObject);
            }
            return ret;
        }

        /// <summary>
        /// This looks over all CheckableStrings in input and checks the one corresponding in obs if it exists
        /// </summary>
        /// <param name="obs"></param>
        /// <param name="input"></param>
        public static void CheckFrom(this ObservableCollection<CheckableStringObject> obs, List<CheckableStringObject> input)
        {
            foreach (var checkableStringObject in obs)
            {
                CheckableStringObject? found = input.Find((cs) => cs.AttributeName == checkableStringObject.AttributeName);
                if (found is null)
                    continue;

                checkableStringObject.IsChecked = found.IsChecked;
            }
        }



    }


    public class CheckableStringObject
    {
        [JsonIgnore]
        private readonly Action<string> OnPropertyChangedCallBack;
        [JsonIgnore]
        private string callBackParam = "";


        public string AttributeName { get; set; } = "";

        /// <summary>
        /// DO NOT USE ONLY PUBLIC FOR JSON SERIALIZATION PURPOSES, I AM TO LAZY TO WRITE A CUSTOM CONVERTER
        /// </summary>
        public bool isChecked { get; set; } = false;

        [JsonIgnore]
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

        [JsonConstructor] //This is only here for json to work
        public CheckableStringObject() { }

        public CheckableStringObject(Action<string> OnPropertyChangedCallBack)
        {
            this.OnPropertyChangedCallBack = OnPropertyChangedCallBack;
        }

        public CheckableStringObject(Action<string> OnPropertyChangedCallBack, string callBackParam)
        {
            this.OnPropertyChangedCallBack = OnPropertyChangedCallBack;
            this.callBackParam = callBackParam;
        }

        public CheckableStringObject(Action<string> OnPropertyChangedCallBack, string callBackParam, CheckableStringObject other)
        {
            this.OnPropertyChangedCallBack = OnPropertyChangedCallBack;
            this.callBackParam = callBackParam;
            this.IsChecked = other.IsChecked;
            this.AttributeName = other.AttributeName;
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
