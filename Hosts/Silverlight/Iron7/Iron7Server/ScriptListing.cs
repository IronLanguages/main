using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Iron7.Iron7Server
{
    public class ScriptListing
    {
        public class Item
        {
            public string AuthorName { get; set; }
            public string ScriptId { get; set; }
            public string Title { get; set; }
            public DateTime WhenLastEdited { get; set; }
        }

        public List<Item> Scripts { get; set; }
        public bool MoreAvailable { get; set; }
        public string NextPageUrl { get; set; }
    }
}
