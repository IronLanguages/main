using IronRuby;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IronApp
{
    
    public enum TrustLevel 
    { 
      BaseTrust      = 0,
      PartialTrust   = 1,
      FullTrust      = 2 
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90")]
    public interface IInspectable
    {
        void GetIids(out int iidCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]out Guid[] iids);
        void GetRuntimeClassName([MarshalAs(UnmanagedType.HString)]out string winTypeName);
        void GetTrustLevel(out TrustLevel trustLevel);
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ScriptEngine engine;
        private ScriptScope scope;

        public MainPage()
        {
            this.InitializeComponent();

            var setup = new ScriptRuntimeSetup();
            setup.HostType = typeof(DlrHost);
            setup.AddRubySetup();

            var runtime = Ruby.CreateRuntime(setup);
            this.engine = Ruby.GetEngine(runtime);
            this.scope = engine.CreateScope();
            this.scope.SetVariable("Main", this);

            runtime.LoadAssembly(typeof(object).GetTypeInfo().Assembly);
            runtime.LoadAssembly(typeof(TextSetOptions).GetTypeInfo().Assembly);
            runtime.LoadAssembly(typeof(TextAlignment).GetTypeInfo().Assembly);

            string outputText = @"
box = main.find_name('OutputBox')
p box.text_alignment
box.text_alignment = Windows::UI::Xaml::TextAlignment.Right
p box.text_alignment
";
            InputBox.IsSpellCheckEnabled = false;
            OutputBox.IsSpellCheckEnabled = false;
            InputBox.Document.SetText(TextSetOptions.None, outputText);

            // http://msdn.microsoft.com/en-us/library/windows/apps/br211377.aspx

            IInspectable insp = (IInspectable)InputBox.Document;
            string winTypeName;
            insp.GetRuntimeClassName(out winTypeName);
            Guid[] iids;
            int iidCount;
            insp.GetIids(out iidCount, out iids);

           // winTypeName = "Windows.Foundation.Collections.IIterator`1<Windows.Foundation.Collections.IMapView`2<Windows.Foundation.Collections.IVector`1<String>, String>>";

            var parts = ParseWindowsTypeName(winTypeName);
            Type type = MakeWindowsType(parts);
            var guid = type.GetTypeInfo().GUID;
        }

        private static char[] nameTerminators = new[] { '`', '>', ','};

        internal static class WinRT
        {
            [DllImport("WinTypes")]
            public static extern void RoParseTypeName(
                [MarshalAs(UnmanagedType.HString)]string typename,
                out int partsCount,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]out IntPtr[] typeNameParts);
        }

        private static string[] ParseWindowsTypeName(string name)
        {
            int partCount;
            IntPtr[] partPtrs = null;
            string[] parts;
            try
            {
                WinRT.RoParseTypeName(name, out partCount, out partPtrs);

                parts = new string[partPtrs.Length];
                for (int i = 0; i < partPtrs.Length; i++)
                {
                    parts[i] = WindowsRuntimeMarshal.PtrToStringHString(partPtrs[i]);
                }
            }
            finally
            {
                if (partPtrs != null)
                {
                    for (int i = 0; i < partPtrs.Length; i++)
                    {
                        if (partPtrs[i] != null)
                        {
                            WindowsRuntimeMarshal.FreeHString(partPtrs[i]);
                        }
                    }
                }
            }

            return parts;
            //List<string> parts = new List<string>();
            //int position = 0;
            //ParseWindowsTypeName(name, ref position, ref parts);
            //return parts.ToArray();
        }

        private static void ParseWindowsTypeName(string name, ref int position, ref List<string> parts)
        {
            int i = name.IndexOfAny(nameTerminators, position);
            if (i == -1)
            {
                parts.Add(name.Substring(position));
                position = name.Length;
                return;
            }

            if (name[i] != '`')
            {
                parts.Add(name.Substring(position, i - position));
                position = i;
                return;
            }

            i++;
            while (name[i] >= '0' && name[i] <= '9')
            {
                i++;
            }

            parts.Add(name.Substring(position, i - position));

            if (i == name.Length || name[i] != '<')
            {
                position = i;
                return;
            }

            // parameters:
            position = i + 1;
            while (true)
            {
                ParseWindowsTypeName(name, ref position, ref parts);
                if (position == name.Length || name[position] != ',')
                {
                    break;
                }

                while (name[++position] == ' ')
                {
                }
            }

            Debug.Assert(name[position] == '>');
            position++;
        }

        private static Type MakeWindowsType(string[] parts)
        {
            int i = 0;
            return MakeWindowsType(parts, ref i);
        }

        private static Type MakeWindowsType(string[] parts, ref int i)
        {
            Debug.Assert(parts.Length > 0);
            Type type = GetWindowsType(parts[i++]);
            var info = type.GetTypeInfo();
            if (!info.IsGenericTypeDefinition)
            {
                return type;
            }

            var args = new Type[info.GenericTypeParameters.Length];
            for (int j = 0; j < args.Length; j++)
            {
                args[j] = MakeWindowsType(parts, ref i);
            }

            return type.MakeGenericType(args);
        }

        private static Type GetWindowsType(string name)
        {
            return GetPrimitiveWindowsType(name) ?? 
                   ProjectWindowsType(name) ?? 
                   Type.GetType(name + ", Windows, ContentType=WindowsRuntime");
        }

        private static Type GetPrimitiveWindowsType(string name)
        {
            switch (name)
            {
                case "Boolean": return typeof(System.Boolean);
                case "Byte": return typeof(System.Byte);
                case "Char": return typeof(System.Char);
                case "Char16": return typeof(System.Char);
                case "DateTime": return typeof(System.DateTimeOffset);
                case "Double": return typeof(System.Double);
                case "Guid": return typeof(System.Guid);
                case "Int16": return typeof(System.Int16);
                case "Int32": return typeof(System.Int32);
                case "Int64":return  typeof(System.Int64);
                case "Object": return typeof(System.Object);
                case "Single": return typeof(System.Single);
                case "String": return typeof(System.String);
                case "TimeSpan": return typeof(System.TimeSpan);
                case "UInt8": return typeof(System.Byte);
                case "UInt16": return typeof(System.UInt16);
                case "UInt32": return typeof(System.UInt32);
                case "UInt64": return typeof(System.UInt64);
                case "Uri": return typeof(System.Uri);
                case "Void": return typeof(void);
                default:
                    return null;
            }
        }

        private static Type ProjectWindowsType(string name)
        {
            switch (name)
            {
                case "Windows.Foundation.Collections.IIterable`1":
                    return typeof(IEnumerable<>);

                case "Windows.Foundation.Collections.IVector`1":
                    return typeof(IList<>);

                case "Windows.Foundation.Collections.IVectorView`1":
                    return typeof(IReadOnlyList<>);

                case "Windows.Foundation.Collections.IKeyValuePair`2":
                    return typeof(KeyValuePair<,>);

                case "Windows.Foundation.Collections.IMap`2":
                    return typeof(IDictionary<,>);

                case "Windows.Foundation.Collections.IMapView`2":
                    return typeof(IReadOnlyDictionary<,>);

                case "Windows.Foundation.Metadata.AttributeUsageAttribute":
                    return typeof(AttributeUsageAttribute);

                case "Windows.Foundation.Metadata.AttributeTargets":
                    return typeof(AttributeTargets);

                case "Windows.Foundation.EventHandler`1":
                    return typeof(EventHandler<>);

                case "Windows.Foundation.EventRegistrationToken":
                    return typeof(EventRegistrationToken);

                case "Windows.Foundation.HResult":
                    return typeof(Exception);

                case "Windows.Foundation.IReference`1":
                    return typeof(Nullable<>);

                case "Windows.Foundation.TimeSpan":
                    return typeof(TimeSpan);

                case "Windows.UI.Xaml.Data.INotifyPropertyChanged":
                    return typeof(INotifyPropertyChanged);

                case "Windows.UI.Xaml.Data.PropertyChangedEventHandler":
                    return typeof(PropertyChangedEventHandler);

                case "Windows.UI.Xaml.Data.PropertyChangedEventArgs":
                    return typeof(PropertyChangedEventArgs);

                case "Windows.UI.Xaml.Interop.TypeName":
                    return typeof(Type);

                // TODO: is anything missing?
            }

            return null;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string text;
            string outputText;
            InputBox.Document.GetText(TextGetOptions.UseCrlf, out text);
            object result;
            using (var stream = new MemoryStream())
            {
                engine.Runtime.IO.SetOutput(stream, Encoding.UTF8);
                try
                {
                    result = engine.Execute<object>(text, scope);
                }
                catch (Exception ex)
                {
                    result = ex;
                }

                var a = stream.ToArray();
                outputText = Encoding.UTF8.GetString(a, 0, a.Length);
            }

            if (result != null)
            {
                outputText += result.ToString();
            }

            OutputBox.Document.SetText(TextSetOptions.None, outputText);
        }
    }
}
