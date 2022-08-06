using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Application.Models
{
    public enum SearchOption
    {
        Image = 0,
        Video = 1,
        Gif = 2
    }

    public static class SearchOptionsContext
    {
        private static List<SearchOption> _allOptions = new();
        static SearchOptionsContext()
        {
            _allOptions = ((SearchOption[])Enum.GetValues(typeof(SearchOption))).ToList();
        }

        public static List<SearchOption> AllOptions { get => _allOptions; } 
    }
}