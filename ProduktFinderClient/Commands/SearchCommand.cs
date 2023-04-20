using ProduktFinderClient.Models;
using ProduktFinderClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProduktFinderClient.Commands
{
    public class SearchCommand : AsyncCommandBase
    {
        Action<object, List<Part>?> SearchFinishedCallBack;
        Action SearchBeganCallBack;
        Action<string> UserUpdateCallback;


        OptionsWindowViewModel optionsWindowViewModel;
        MainWindowViewModel mainWindowViewModel;
        public SearchCommand(Action SearchBeganCallBack, Action<object, List<Part>?> SearchFinishedCallBack, OptionsWindowViewModel optionsWindowViewModel, MainWindowViewModel mainWindowViewModel, Action<string> UserUpdateCallback)
        {
            this.SearchFinishedCallBack = SearchFinishedCallBack;
            this.SearchBeganCallBack = SearchBeganCallBack;
            this.optionsWindowViewModel = optionsWindowViewModel;
            this.mainWindowViewModel = mainWindowViewModel;

            this.UserUpdateCallback = UserUpdateCallback;
        }

        /// <summary>
        /// param is the keyword as a string
        /// </summary>
        /// <param name="parameter"></param>
        protected override async Task ExecuteAsync(object? parameter)
        {
            if (parameter is null) return;

            if (parameter is string keyword)
            {
                SearchBeganCallBack?.Invoke();
                int numberSearchResults = 20;

                if (Int64.Parse(optionsWindowViewModel.ResultsInSearchPerAPI) < 20)
                    numberSearchResults = Int32.Parse(optionsWindowViewModel.ResultsInSearchPerAPI);

                //if 0 is passed most apis will answer with all their results
                if (numberSearchResults == 0)
                    numberSearchResults = 20;


                /// What happens is a translation from the String in the LieferantenDrop down
                /// to an Enum of Module Type using the BidiDict Filter.ModulesTranslation

                List<Task> tasks = new List<Task>();

                foreach (var checkableString in mainWindowViewModel.Lieferanten)
                {
                    if (checkableString.IsChecked)
                    {
                        Filter.ModulesTranslation.TryGetKey(checkableString.AttributeName, out ModuleType moduleType);
                        tasks.Add(RequestHandler.SearchWith(keyword, moduleType, numberSearchResults, UserUpdateCallback, (x) => SearchFinishedCallBack(mainWindowViewModel, x)));
                    }
                }

                await Task.WhenAll(tasks);
            }

        }
    }
}
