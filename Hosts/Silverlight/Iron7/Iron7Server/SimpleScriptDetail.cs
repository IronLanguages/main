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

namespace Iron7.Iron7Server
{
    public class SimpleScriptDetail
    {
        public string AuthorName { get; set; }
        public string Code { get; set; }
        public string TagsAsText { get; set; }
        public string ScriptId { get; set; }
        public string Title { get; set; }
        public DateTime WhenLastEdited { get; set; }
    }
}
