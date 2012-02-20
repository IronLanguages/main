using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

[assembly: ReferenceAssembly]
[assembly: CLSCompliant(true)]

namespace System.Runtime.CompilerServices {
    [CLSCompliant(true)]
    public interface IRuntimeVariables {
        int Count { get; }

        object this[int index] { get; set; }
    }

	public sealed class Closure
	{
		public readonly object[] Constants;
		public readonly object[] Locals;

		public Closure(object[] constants, object[] locals)
		{
			this.Constants = constants;
			this.Locals = locals;
		}
	}
}

namespace System.Collections
{
    [CLSCompliant(true)]
    public interface IDictionary : ICollection, IEnumerable
	{
        object this[object key]
		{
			get;
			set;
		}
		
        ICollection Keys
		{
			get;
		}
		
        ICollection Values
		{
			get;
		}
		
        bool IsReadOnly
		{
			get;
		}
		
        bool IsFixedSize
		{
			get;
		}
		
        bool Contains(object key);
		
        void Add(object key, object value);
		
        void Clear();
		
        new IDictionaryEnumerator GetEnumerator();
		
        void Remove(object key);
	}

    [CLSCompliant(true)]
	public interface IDictionaryEnumerator : IEnumerator
	{
		object Key
		{
			get;
		}

		object Value
		{
			get;
		}

		DictionaryEntry Entry
		{
			get;
		}
	}

    [CLSCompliant(true)]
    public struct DictionaryEntry
	{
		public object Key
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		
        public object Value
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		
        public DictionaryEntry(object key, object value)
		{
		}
	}
}

namespace System.Runtime.CompilerServices
{
	public sealed class ReadOnlyCollectionBuilder<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
	{
		public int Capacity
		{
			get
			{
                return 0;
			}
			set
			{
			}
		}

        public int Count
		{
			get
			{
                return 0;
			}
		}

		public T this[int index]
		{
			get
			{
                return default(T);
			}
			set
			{
			}
		}

		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		bool IList.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}
		object IList.this[int index]
		{
			get
			{
                return null;
			}
			set
			{
				
			}
		}
		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}
		object ICollection.SyncRoot
		{
			get
			{
				return null;
			}
		}
		public ReadOnlyCollectionBuilder()
		{
		}
		public ReadOnlyCollectionBuilder(int capacity)
		{
		}
		public ReadOnlyCollectionBuilder(IEnumerable<T> collection)
		{
		}
		public int IndexOf(T item)
		{
            return 0;
		}
		public void Insert(int index, T item)
		{
		}
		public void RemoveAt(int index)
		{
		}
		public void Add(T item)
		{
		}
		public void Clear()
		{
		}
		public bool Contains(T item)
		{
			return false;
		}
		public void CopyTo(T[] array, int arrayIndex)
		{
		}
		public bool Remove(T item)
		{
			return false;
		}
		public IEnumerator<T> GetEnumerator()
		{
            return null;
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
            return null;
		}
		int IList.Add(object value)
		{
            return 0;
		}
		bool IList.Contains(object value)
		{
            return false;
		}
		int IList.IndexOf(object value)
		{
			return 0;
		}
		void IList.Insert(int index, object value)
		{
		}
		void IList.Remove(object value)
		{
		}
		void ICollection.CopyTo(Array array, int index)
		{
		}
		public void Reverse()
		{
		}
		public void Reverse(int index, int count)
		{
		}
		public T[] ToArray()
		{
            return null;
		}
		public ReadOnlyCollection<T> ToReadOnlyCollection()
		{
            return null;
		}
	}

	public abstract class DebugInfoGenerator
	{
		public abstract void MarkSequencePoint(LambdaExpression method, int ilOffset, DebugInfoExpression sequencePoint);
		
		protected DebugInfoGenerator()
		{
		}
	}
}
