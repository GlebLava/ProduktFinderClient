using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace ProduktFinderClient.DataTypes
{
    [Serializable]
    public class MostUsedKeywordsModule
    {
        public Dictionary<string, int> MainDictionary { get; set; } = new Dictionary<string, int>();

        public string? GetMostUsedKeyword(ref int index, string[] keywords)
        {
            string? s = null;
            int maxKeywordCount = 0;
            
            for (int i = 0; i < keywords.Length; i++) 
            {
                string keyword = keywords[i];

                if (MainDictionary.TryGetValue(keyword, out int count))
                {
                    if (count > maxKeywordCount)
                    {
                        maxKeywordCount = count;
                        s = keyword;
                        index = i;
                    }
                }
            }

            return s;
        }


        //hardcoded sizes for now
        public void RegisterKeyword(string keyword)
        {
            if (MainDictionary.TryGetValue(keyword, out int count))
            {
                //there exists an entry in the maindictionary so increase the count
                if (count != int.MaxValue)
                    MainDictionary[keyword]++;

                return;
            } //no entry in the main dictionary, so first check if its not full
            else if (MainDictionary.Count <= 200)
            {
                //its not full yet so just add the keyword
                MainDictionary.Add(keyword, 1);
            } //maindictionary is full, so remove the keyword with the lowest count for the new one
            else
            {
                string lowestString = "";
                int lowestStringCount = int.MaxValue;
                foreach (string s in MainDictionary.Keys)
                {
                    if (MainDictionary[s] < lowestStringCount)
                    {
                        lowestStringCount = MainDictionary[s];
                        lowestString = s;
                    }
                }


                MainDictionary.Remove(lowestString);
                MainDictionary.Add(keyword, 1);
            }
        }
    }
}
