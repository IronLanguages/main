// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;


namespace gpcc
{
	public class Set<T>: IEnumerable<T>
	{
		private Dictionary<T, bool> elements = new Dictionary<T,bool>();

		public Set()
		{
		}


		public Set(Set<T> items)
		{
			AddRange(items);
		}


		public void Add(T item)
		{
			elements[item] = true;
		}


		public void AddRange(Set<T> items)
		{
			foreach (T item in items)
				Add(item);
		}


		public IEnumerator<T> GetEnumerator()
		{
			return elements.Keys.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}


		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("[");

			foreach (T element in elements.Keys)
				builder.AppendFormat("{0}, ", element);

			builder.Append("]");

			return builder.ToString();
		}
	}
}