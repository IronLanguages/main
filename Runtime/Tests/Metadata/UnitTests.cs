/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Scripting.Metadata;

namespace Metadata {
    internal static class UnitTests {
        public static void Run() {
            TestEmpty();

            TestIndexOf("xx", '\0', -1);
            TestIndexOf("", '\0', -1);
            TestIndexOf("", 'x', -1);
            TestIndexOf(".", '.', 0);
            TestIndexOf(".", 'x', -1);
            TestIndexOf("hello.world", '.', 5);
            TestIndexOf("helloworld", '.', -1);
            TestIndexOf("helloworld.", '.', 10);

            TestPrefixSuffix("Func`4", '`', "Func", "4");
            TestPrefixSuffix("Func`", '`', "Func", "");
            TestPrefixSuffix("`", '`', "", "");
            TestPrefixSuffix("Func", '`', null, null);

            TestEquals();
            TestAllPrefixes("System.Collections.Generic");
            TestDict();
        }

        private static unsafe void TestEmpty() {
            byte[] empty = new byte[] { 0 };
            fixed (byte* fempty = &empty[0]) {
                MetadataName e = new MetadataName(fempty, null);
                Assert(e.Equals(MetadataName.Empty));
                Assert(e.Equals(MetadataNamePart.Empty));
                
                Assert(MetadataName.Empty.IsEmpty);
                Assert(MetadataName.Empty.GetHashCode() == e.GetHashCode());
                Assert(MetadataName.Empty.Equals(e));
                Assert(MetadataName.Empty.Equals(MetadataNamePart.Empty));
                Assert(MetadataName.Empty.GetLength() == 0);
                Assert(MetadataName.Empty.ToString() == "");
                Assert(MetadataName.Empty.GetExtent().Equals(MetadataNamePart.Empty));

                Assert(MetadataNamePart.Empty.Length == 0);
                Assert(MetadataNamePart.Empty.Equals(e));
                Assert(MetadataNamePart.Empty.Equals(MetadataName.Empty));
                Assert(MetadataNamePart.Empty.GetPart(0).Equals((object)MetadataNamePart.Empty));
                Assert(MetadataNamePart.Empty.GetPart(0).Equals(MetadataNamePart.Empty));
                Assert(MetadataNamePart.Empty.GetPart(0, 0).Equals(MetadataNamePart.Empty));
                Assert(MetadataNamePart.Empty.ToString() == "");
                Assert(MetadataNamePart.Empty.IndexOf(1) == -1);
                Assert(MetadataNamePart.Empty.IndexOf(1, 0, 0) == -1);
                Assert(MetadataNamePart.Empty.LastIndexOf(1, 0, 0) == -1);
                Assert(MetadataNamePart.Empty.IndexOf(0) == -1);
                Assert(MetadataNamePart.Empty.IndexOf(0, 0, 0) == -1);
                Assert(MetadataNamePart.Empty.LastIndexOf(0, 0, 0) == -1);
            }
        }

        private static unsafe void TestIndexOf(string str, char c, int expected) {
            byte[] bytes = Encoding.UTF8.GetBytes(str + '\0');
            fixed (byte* fbytes = &bytes[0]) {
                MetadataName name = new MetadataName(fbytes, null);
                Assert(name.IndexOf(checked((byte)c)) == expected);
            }
        }

        private static unsafe void TestPrefixSuffix(string str, char separator, string expectedPrefix, string expectedSuffix) {
            byte[] bytes = Encoding.UTF8.GetBytes(str + '\0');
            fixed (byte* fbytes = &bytes[0]) {
                MetadataName name = new MetadataName(fbytes, null);
                MetadataNamePart prefix;
                MetadataNamePart suffix;
                MetadataNamePart extent = name.GetExtent();

                int index = extent.IndexOf((byte)separator);
                Assert((index < 0) == (expectedPrefix == null));

                if (index >= 0) {
                    prefix = extent.GetPart(0, index);
                    Assert(prefix.ToString() == expectedPrefix);
                    suffix = extent.GetPart(index + 1);
                    Assert(suffix.ToString() == expectedSuffix);
                }
            }
        }

        private static unsafe void TestEquals() {
            byte[] b1 = Encoding.UTF8.GetBytes("hello\0");
            byte[] b2 = Encoding.UTF8.GetBytes("__hello__\0");
            byte[] b3 = Encoding.UTF8.GetBytes("__hell\0");
            byte[] b4 = Encoding.UTF8.GetBytes("\0");
            
            fixed (byte* fb1 = &b1[0]) {
                MetadataName name = new MetadataName(fb1, null);
                Assert(name.Equals(b2, 2, 5));
                Assert(!name.Equals(b2, 2, 6));
                Assert(!name.Equals(b2, 2, 4));
                Assert(!name.Equals(b2, 1, 4));
            }

            fixed (byte* fb4 = &b4[0]) {
                MetadataName name = new MetadataName(fb4, null);
                Assert(name.Equals(b2, 2, 0));
                Assert(!name.Equals(b2, 0, 1));
            }
        }

        private static unsafe void TestAllPrefixes(string ns) {
            byte[] bytes = Encoding.UTF8.GetBytes(ns + '\0');
            fixed (byte* fbytes = &bytes[0]) {
                MetadataNamePart name = new MetadataName(fbytes, null).GetExtent();
                int dot = name.Length;

                while (true) {
                    int nextDot = name.LastIndexOf((byte)'.', dot - 1, dot);
                    Assert(nextDot == ns.LastIndexOf('.', dot - 1, dot));
                    dot = nextDot;
                    if (dot < 0) {
                        break;
                    }
                    Assert(name.GetPart(0, dot).ToString() == ns.Substring(0, dot));
                    Assert(name.GetPart(dot + 1).ToString() == ns.Substring(dot + 1));
                }
            }
        }

        private static unsafe void TestDict() {
            Dictionary<MetadataNamePart, int> dict = new Dictionary<MetadataNamePart, int>();
            byte[] bytes = Encoding.UTF8.GetBytes("A.B.XXXXXXXXX" + '\0');
            fixed (byte* fbytes = &bytes[0]) {
                MetadataName name = new MetadataName(fbytes, null);
                MetadataNamePart[] parts = new[] {
                    MetadataNamePart.Empty,
                    name.GetExtent(),
                    name.GetExtent().GetPart(0, 1),
                    name.GetExtent().GetPart(2, 1),
                    name.GetExtent().GetPart(4, 6),
                };

                for (int i = 0; i < parts.Length; i++) {
                    dict.Add(parts[i], i);
                }

                Assert(dict.Count == parts.Length);

                for (int i = 0; i < parts.Length; i++) {
                    int value;
                    Assert(dict.TryGetValue(parts[i], out value));
                    Assert(value == i);
                }
            }
        }

        internal static void Assert(bool cond) {
            if (!cond) {
                throw new ApplicationException();
            }
        }
    }
}
