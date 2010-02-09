/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2

// IValueEquality is unnecessary in .NET 4.0 and can be replaced by IStructuralEquatable
// given some default IEqualityComparer
namespace Microsoft.Scripting {
    /// <summary>
    /// Provides hashing and equality based upon the value of the object instead of the reference.
    /// </summary>
    public interface IValueEquality {
        /// <summary>
        /// Gets the hash code for the value of the instance.
        /// </summary>
        /// <returns>A hash code</returns>
        /// <exception cref="Microsoft.Scripting.ArgumentTypeException">The type is mutable and cannot be hashed by value</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        int GetValueHashCode();

        /// <summary>
        /// Determines if two values are equal
        /// </summary>
        /// <param name="other">The object to compare the current object against.</param>
        /// <returns>Returns true if the objects are equal, false if they are not.</returns>        
        bool ValueEquals(object other);
    }
}

// IStructuralEquatable/IStructuralComparable are included in .NET 4.0.
namespace System.Collections {
    /// <summary>
    /// Defines methods to support the comparison of objects for structural equality.
    /// </summary>
    public interface IStructuralEquatable {
        /// <summary>
        /// Determines whether an object is equal to the current instance.
        /// </summary>
        /// <param name="other">The object to compare with the current instance.</param>
        /// <param name="comparer">An object that determines whether the current instance and other are equal.</param>
        /// <returns>true if the two objects are equal; otherwise, false.</returns>
        bool Equals(object other, IEqualityComparer comparer);
        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <param name="comparer">An object that computes the hash code of the current object.</param>
        /// <returns>The hash code for the current instance.</returns>
        int GetHashCode(IEqualityComparer comparer);
    }

    /// <summary>
    /// Supports the structural comparison of collection objects.
    /// </summary>
    public interface IStructuralComparable {
        /// <summary>
        /// Determines whether the current collection object precedes, occurs in the
        /// same position as, or follows another object in the sort order.
        /// </summary>
        /// <param name="other">The object to compare with the current instance.</param>
        /// <param name="comparer">An object that compares the current object and other.</param>
        /// <returns>
        /// An integer that indicates the relationship of the current collection object
        /// to other, as shown in the following table.
        /// Return value    Description
        /// -1              The current instance precedes other.
        /// 0               The current instance and other are equal.
        /// 1               The current instance follows other.
        /// </returns>
        int CompareTo(object other, IComparer comparer);
    }
}

#endif
