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
using System.Text;

namespace Microsoft.Scripting.Actions {
    /// <summary>
    /// Determines the result of a conversion action.  The result can either result in an exception, a value that
    /// has been successfully converted or default(T), or a true/false result indicating if the value can be converted.
    /// </summary>
    public enum ConversionResultKind {
        /// <summary>
        /// Attempts to perform available implicit conversions and throws if there are no available conversions.
        /// </summary>
        ImplicitCast,
        /// <summary>
        /// Attempst to perform available implicit and explicit conversions and throws if there are no available conversions.
        /// </summary>
        ExplicitCast,
        /// <summary>
        /// Attempts to perform available implicit conversions and returns default(ReturnType) if no conversions can be performed.
        /// 
        /// If the return type of the rule is a value type then the return value will be zero-initialized.  If the return type
        /// of the rule is object or another class then the return type will be null (even if the conversion is to a value type).
        /// This enables ImplicitTry to be used to do TryConvertTo even if the type is value type (and the difference between
        /// null and a real value can be distinguished).
        /// </summary>
        ImplicitTry,
        /// <summary>
        /// Attempts to perform available implicit and explicit conversions and returns default(ReturnType) if no conversions 
        /// can be performed.
        /// 
        /// If the return type of the rule is a value type then the return value will be zero-initialized.  If the return type
        /// of the rule is object or another class then the return type will be null (even if the conversion is to a value type).
        /// This enables ExplicitTry to be used to do TryConvertTo even if the type is value type (and the difference between
        /// null and a real value can be distinguished).
        /// </summary>
        ExplicitTry
    }
}
