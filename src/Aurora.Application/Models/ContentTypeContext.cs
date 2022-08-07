using System;
using System.Collections.Generic;
using System.Linq;

namespace Aurora.Application.Models
{
    public static class ContentTypeContext
    {
        private static List<ContentType> _allContentTypes = new();
        static ContentTypeContext()
        {
            _allContentTypes = ((ContentType[])Enum.GetValues(typeof(ContentType))).ToList();
        }

        public static List<ContentType> ContentTypes => _allContentTypes;
    }
}