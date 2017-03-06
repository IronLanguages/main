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
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;

#if false // Can't get this to work - problems with Microsoft.CSharp.dll :(
namespace Iron7.Common
{
    public static class DynamicJson
    {
        public static object ConvertJsonStringToObject(string json)
        {
            // Create the json.Net Linq object for our json string
            JObject jsonObject = JObject.Parse(json);
            return ConvertJTokenToObject(jsonObject);
        }

        // taken from http://blog.petegoo.com/blog/archive/2009/10/27/using-json.net-to-eval-json-into-a-dynamic-variable-in.aspx
        public static object ConvertJTokenToObject(JToken token)
        {
            if (token is JValue)
            {
                return ((JValue)token).Value;
            }
            if (token is JObject)
            {
                ExpandoObject expando = new ExpandoObject();
                (from childToken in ((JToken)token) where childToken is JProperty select childToken as JProperty).ToList().ForEach(property =>
                {
                    ((IDictionary<string, object>)expando).Add(property.Name, ConvertJTokenToObject(property.Value));
                });
                return expando;
            }
            if (token is JArray)
            {
                object[] array = new object[((JArray)token).Count];
                int index = 0;
                foreach (JToken arrayItem in ((JArray)token))
                {
                    array[index] = ConvertJTokenToObject(arrayItem);
                    index++;
                }
                return array;
            }
            throw new ArgumentException(string.Format("Unknown token type '{0}'", token.GetType()), "token");
        }
    }
}
#endif