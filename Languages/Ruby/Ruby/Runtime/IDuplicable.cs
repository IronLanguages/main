/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

namespace IronRuby.Runtime {

    // Naming conventions:
    // method            | tainted | Ruby instance variables | frozen |  singleton members  | internal state of the object  
    // Copy              |    NO   |            NO           |   NO   |          NO         |            YES               
    // Clone             |   YES   |            NO           |   NO   |          NO         |            YES               
    // Duplicate(false)  |   YES   |           YES           |   NO   |          NO         |          PARTIAL              
    // Duplicate(true)   |   YES   |           YES           |   NO   |         YES         |          PARTIAL             
    //
    // PARTIAL - part of the state is initialized by "initialize_copy"
    // 
    // Hash: initialize_copy 

    /// <summary>
    /// Implemented by classes that are capable of creating their subclass clones.
    /// </summary>
    public interface IDuplicable {
        object Duplicate(RubyContext/*!*/ context, bool copySingletonMembers);
    }
}
