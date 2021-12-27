﻿using System.Collections.Generic;
using Aurora.Application.Enums;

namespace Aurora.Application.Models
{
    public class SearchRequestDto
    {
        public string SearchTerm { get; set; }
        public List<SearchOption> SearchOptions { get; set; }
        public int ResponseItemsMaxCount { get; set; }
        public List<SupportedWebsite> Websites { get; set; }
    }
}