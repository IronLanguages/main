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
using System.Reflection;
using Microsoft.Scripting;
using RowanTest.Common;
using MutableTuple = Microsoft.Scripting.MutableTuple;
using System.Runtime.CompilerServices;

namespace TestInternalDLR {

    // Strongbox should not ever be sealed
    class MyStrongBox<T> : System.Runtime.CompilerServices.StrongBox<T>
    {
        public MyStrongBox(T value) : base(value) {
        }
    }

    class TestTuple : BaseTest {
        public void VerifyTuple(int size) {
            //Construct a tuple of the right type
            MethodInfo mi = typeof(MutableTuple).GetMethod("MakeTupleType",BindingFlags.Public | BindingFlags.Static);
            Assert(mi!=null,"Could not find Tuple.MakeTupleType");

            Type[] args = new Type[size];
            object[] values = new object[size];
            for (int i = 0; i < size; i++) {
                args[i] = typeof(int);
                values[i] = 0;
            }

            Type tupleType = (Type)mi.Invoke(null, new object[] { args });
            MutableTuple t = MutableTuple.MakeTuple(tupleType, values);

            /////////////////////
            //Properties

            //Write
            for (int i=0; i< size; i++){
                object o = t;
                foreach (PropertyInfo pi in MutableTuple.GetAccessPath(tupleType,i)){
                    if (typeof(MutableTuple).IsAssignableFrom(pi.PropertyType))
                        o = pi.GetValue(o, null);
                    else
                        pi.SetValue(o, i * 5, null);
                }
            }

            //Read
            for (int i=0; i< size; i++){
                object o = t;
                foreach (PropertyInfo pi in MutableTuple.GetAccessPath(tupleType,i))
                    o = pi.GetValue(o,null);
                AreEqual(typeof(int),o.GetType());
                AreEqual((int)o,i*5);            
            }

            //Negative cases for properties
            AssertError<ArgumentException>(delegate() {
                foreach (PropertyInfo pi in MutableTuple.GetAccessPath(tupleType, -1))
                    Console.WriteLine(pi.Name); //This won't run, but we need it so that this call isn't inlined
            });

            /////////////////////
            //GetTupleValues
            values = MutableTuple.GetTupleValues(t);
            AreEqual(values.Length, size);
            for(int i=0; i<size; i++) {
                AreEqual(typeof(int), values[i].GetType());
                AreEqual((int)(values[i]), i * 5);
            }

            /////////////////////
            //Access methods

            if (size <= MutableTuple.MaxSize) {
                //SetValue
                for (int i=0; i < size; i++)
                    t.SetValue(i, i * 3);

                //GetValue
                for (int i=0; i < size; i++)
                    AreEqual(t.GetValue(i), i * 3);

                //Ensure there are no extras
                if(tupleType.GetGenericArguments().Length<=size){
                    //We're requesting an index beyond the end of this tuple.
                    AssertError<ArgumentException>(delegate() { t.SetValue(size, 3); });
                    AssertError<ArgumentException>(delegate() { t.GetValue(size); });
                } else {
                    /*We're requesting an index in the scope of this tuple but beyond the scope of our
                     requested capacity (in which case the field's type will be Microsoft.Scripting.None
                     and we won't be able to convert "3" to that).  Imagine asking for a tuple of 3 ints,
                     we'd actually get a Tuple<int,int,int,Microsoft.Scripting.None> since there is no
                     Tuple that takes only 3 generic arguments.*/
                    AssertError<InvalidCastException>(delegate() { t.SetValue(size, 3); });

                    //Verify the type of the field
                    AreEqual(typeof(Microsoft.Scripting.Runtime.DynamicNull), tupleType.GetGenericArguments()[size]);

                    //Verify the value of the field is null
                    AreEqual(null, t.GetValue(size));
                }
            }
        }

        public void TestBasic() {
            foreach (int i in new int[] { 1, 2, 4, 8, 16, 32, 64, 127, 128, 129, 256, 512, 1024, 24, 96 }) {
                VerifyTuple(i);
            }
        }

        // Quick validation of this.
        public void TestStrongBox()
        {
            MyStrongBox<int> sb = new MyStrongBox<int>(5);
            AreEqual(sb.Value, 5);

        }
    }

    // Basic tests for ReadOnlyCollectionBuilder<T>
    class TestROCBuilder : BaseTest
    {

        public void TestReadOnlyCollectionBuilder()
        {

            int cnt = 0;

            // Empty 
            ReadOnlyCollectionBuilder<int> a = new ReadOnlyCollectionBuilder<int>();
            AreEqual(0, a.Count);
            AreEqual(0, a.Capacity );
            AreEqual(a.ToReadOnlyCollection().Count, 0);
            AreEqual(a.ToReadOnlyCollection().Count, 0);

            // Simple case
            a.Add(5);
            AreEqual(1, a.Count);
            AreEqual(4, a.Capacity);
            AreEqual(a.ToReadOnlyCollection()[0], 5);
            AreEqual(a.ToReadOnlyCollection().Count, 0);  // Will reset

            a = new ReadOnlyCollectionBuilder<int>(0);
            AreEqual(0, a.Count);
            AssertError<ArgumentException>(() => a = new ReadOnlyCollectionBuilder<int>(-1));

            a = new ReadOnlyCollectionBuilder<int>(5);
            for (int i = 1; i <= 10; i++)
            {
                a.Add(i);
            }

            AreEqual(10, a.Capacity);
            System.Collections.ObjectModel.ReadOnlyCollection<int> readonlyCollection = a.ToReadOnlyCollection();
            AreEqual(0, a.Capacity);
            AreEqual(readonlyCollection.Count , 10);

            ReadOnlyCollectionBuilder<int> b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            b.Add(11);
            AreEqual(b.Count, 11);
                        
            AssertError<ArgumentException>(() => a = new ReadOnlyCollectionBuilder<int>(null));

            // Capacity tests
            b.Capacity = 11;
            AssertError<ArgumentException>(() => b.Capacity = 10);
            b.Capacity = 50;
            AreEqual(b.Count, 11);
            AreEqual(b.Capacity , 50);

            // IndexOf cases
            AreEqual(b.IndexOf(5), 4);
            AreEqual(b[4], 5);            
            a = new ReadOnlyCollectionBuilder<int>();
            AreEqual(a.IndexOf(5), -1);

            // Insert cases
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            AssertError<ArgumentException>(() => b.Insert(11,11));
            b.Insert(2, 24);
            AreEqual(b.Count, 11);
            AreEqual(b[1], 2);
            AreEqual(b[2], 24);
            AreEqual(b[3], 3);
            b.Insert(11, 1234);
            AssertError<ArgumentException>(() => b.Insert(-1, 55));
            AreEqual(b[11], 1234);
            AreEqual(b.ToReadOnlyCollection().Count, 12);

            // Remove
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            AreEqual(b.Remove(2),true);
            AreEqual(b[0], 1);
            AreEqual(b[1], 3);
            AreEqual(b[2], 4);
            AreEqual(b.Remove(2), false);

            // RemoveAt
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            b.RemoveAt(2);
            AreEqual(b[1], 2);
            AreEqual(b[2], 4);
            AreEqual(b[3], 5);
            AssertError<ArgumentException>(() => b.RemoveAt(-5));
            AssertError<ArgumentException>(() => b.RemoveAt(9));
            
            // Clear
            b.Clear();
            AreEqual(b.Count, 0);
            AreEqual(b.ToReadOnlyCollection().Count , 0);
            b = new ReadOnlyCollectionBuilder<int>();
            b.Clear();
            AreEqual(b.Count, 0);

            // Contains
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            AreEqual(b.Contains(5), true);
            AreEqual(b.Contains(-3), false);

            ReadOnlyCollectionBuilder<object> c = new ReadOnlyCollectionBuilder<object>();
            c.Add("HI");
            AreEqual(c.Contains("HI"), true);
            AreEqual(c.Contains(null), false);
            c.Add(null);
            AreEqual(c.Contains(null), true);

            // CopyTo
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            int[] ary = new int[10];
            b.CopyTo(ary, 0);

            AreEqual(ary[0], 1);
            AreEqual(ary[9], 10);

            // Reverse
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            b.Reverse();
            // 1..10
            cnt = 10;
            for (int i = 0; i < 10; i++)
            {
                AreEqual(b[i], cnt--);
            }

            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            AssertError<ArgumentException>(() => b.Reverse(-1,5));
            AssertError<ArgumentException>(() => b.Reverse(5,-1));
            b.Reverse(3, 3);
            // 1,2,3,4,5,6,7,8,9.10
            // 1,2,3,6,5,4,7,8,9,10
            AreEqual(b[1], 2);
            AreEqual(b[2], 3);
            AreEqual(b[3], 6);
            AreEqual(b[4], 5);
            AreEqual(b[5], 4);
            AreEqual(b[6], 7);

            // ToArray
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            int[] intAry = b.ToArray();
            AreEqual(intAry[0], 1);
            AreEqual(intAry[9], 10);

            b = new ReadOnlyCollectionBuilder<int>();
            intAry  = b.ToArray();
            AreEqual(intAry.Length, 0);

            // IEnumerable cases
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            cnt = 0;
            foreach (int i in b)
            {
                cnt++;
            }
            AreEqual(cnt, 10);

            b = new ReadOnlyCollectionBuilder<int>();
            cnt = 0;
            foreach (int i in b)
            {
                cnt++;
            }
            AreEqual(cnt, 0);

            // Error case
            AssertError<InvalidOperationException>(() => ChangeWhileEnumeratingAdd());
            AssertError<InvalidOperationException>(() => ChangeWhileEnumeratingRemove());

            // IList members
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            System.Collections.IList lst = b;

            // IsReadOnly
            AreEqual(lst.IsReadOnly, false);

            // Add
            AreEqual(lst.Add(11), 10);
            AreEqual(lst.Count,11);
            AssertError<ArgumentException>(() => lst.Add("MOM"));
            AssertError<ArgumentException>(() => lst.Add(null));

            c = new ReadOnlyCollectionBuilder<object>();

            c.Add("HI");
            c.Add(null);
            lst = c;
            lst.Add(null);
            AreEqual(lst.Count, 3);

            // Contains
            lst = b;
            AreEqual(lst.Contains(5), true);
            AreEqual(lst.Contains(null), false);

            lst = c;
            AreEqual(lst.Contains("HI"), true);
            AreEqual(lst.Contains("hi"), false);
            AreEqual(lst.Contains(null), true);
            
            // IndexOf          
            lst = b;
            AreEqual(lst.IndexOf(null), -1);
            AreEqual(lst.IndexOf(1234), -1);
            AreEqual(lst.IndexOf(5), 4);

            // Insert
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            lst = b;
            AssertError<ArgumentException>(() => lst.Insert(11, 11));
            lst.Insert(2, 24);
            AreEqual(lst.Count, 11);
            AreEqual(lst[1], 2);
            AreEqual(lst[2], 24);
            AreEqual(lst[3], 3);
            lst.Insert(11, 1234);
            AssertError<ArgumentException>(() => lst.Insert(-1, 55));
            AreEqual(lst[11], 1234);

            AssertError<ArgumentException>(() => lst.Insert(3,"MOM"));

            // IsFixedSize
            AreEqual(lst.IsFixedSize, false);

            // Remove
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            lst = b;
            lst.Remove(2);
            AreEqual(lst[0], 1);
            AreEqual(lst[1], 3);
            AreEqual(lst[2], 4);
            lst.Remove(2);

            // Indexing
            lst[3] = 234;
            AreEqual(lst[3], 234);
            AssertError<ArgumentException>(() => lst[3] = null);
            AssertError<ArgumentException>(() => lst[3] = "HI");

            // ICollection<T>

            // IsReadOnly
            System.Collections.Generic.ICollection<int> col = b;
            AreEqual(col.IsReadOnly, false);

            // ICollection
            b = new ReadOnlyCollectionBuilder<int>(readonlyCollection);
            System.Collections.ICollection col2 = b;
            AreEqual(col2.IsSynchronized, false);
            Assert(col2.SyncRoot != null);
            intAry = new int[10];
            col2.CopyTo(intAry, 0);
            AreEqual(intAry[0], 1);
            AreEqual(intAry[9], 10);

            string[] str = new string[50];
            AssertError<ArrayTypeMismatchException>(() => col2.CopyTo(str,0));

        }

        void ChangeWhileEnumeratingAdd()
        {
            ReadOnlyCollectionBuilder<int> b = new ReadOnlyCollectionBuilder<int>();
            b.Add(5);
            b.Add(6);
            foreach (int i in b)
            {
                b.Add(234);
            }
        }

        void ChangeWhileEnumeratingRemove()
        {
            ReadOnlyCollectionBuilder<int> b = new ReadOnlyCollectionBuilder<int>();
            b.Add(5);
            b.Add(6);
            foreach (int i in b)
            {
                b.Remove(5);
            }
        }
    }
}
