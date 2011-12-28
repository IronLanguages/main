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

using IronRuby.Runtime;
using Microsoft.Scripting.Utils;
using IronRuby.Compiler.Generation;
using System.Diagnostics;
using System;
using System.Runtime.Serialization;

namespace IronRuby.Builtins {
    //
    // Basic members
    //

#if GENERATOR
    Classes = [:MutableString, :Proc, :Range, :RubyRegex, :RubyIO, :Exception, :RubyArray, :Hash, :MatchData]

    def generate
      (Classes - [:Exception]).each do |cls| 
        @class = cls
        super
      end
    end

    def superclass
      @class
    end
#else
    public partial class /*$superclass{*/RubyException/*}*/ {
        [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
        [DebuggerDisplay(RubyObject.DebuggerDisplayValueStr, Type = RubyObject.DebuggerDisplayTypeStr)]
        public sealed partial class Subclass : /*$superclass{*/Exception/*}*/, IRubyObject {
            private RubyInstanceData _instanceData;
            private RubyClass/*!*/ _immediateClass;

            [Emitted]
            public RubyClass/*!*/ ImmediateClass {
                get {
                    return _immediateClass;
                }
                set {
                    // once a singleton immediate class is set it can't be changed:
                    Debug.Assert((_immediateClass == null || !_immediateClass.IsSingletonClass) && value != null);
                    _immediateClass = value;
                }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            public int BaseGetHashCode() {
                return base.GetHashCode();
            }

            public bool BaseEquals(object other) {
                return base.Equals(other);
            }

            public string/*!*/ BaseToString() {
                return base.ToString();
            }

            private string GetDebuggerDisplayValue() {
                return RubyOps.GetDebuggerDisplayValue(_immediateClass, this);
            }

            private string GetDebuggerDisplayType() {
                return RubyOps.GetDebuggerDisplayType(_immediateClass);
            }
        }
    }
#endif
#region Generated
    public partial class MutableString {
        [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
        [DebuggerDisplay(RubyObject.DebuggerDisplayValueStr, Type = RubyObject.DebuggerDisplayTypeStr)]
        public sealed partial class Subclass : MutableString, IRubyObject {
            private RubyInstanceData _instanceData;
            private RubyClass/*!*/ _immediateClass;

            [Emitted]
            public RubyClass/*!*/ ImmediateClass {
                get {
                    return _immediateClass;
                }
                set {
                    // once a singleton immediate class is set it can't be changed:
                    Debug.Assert((_immediateClass == null || !_immediateClass.IsSingletonClass) && value != null);
                    _immediateClass = value;
                }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            public int BaseGetHashCode() {
                return base.GetHashCode();
            }

            public bool BaseEquals(object other) {
                return base.Equals(other);
            }

            public string/*!*/ BaseToString() {
                return base.ToString();
            }

            private string GetDebuggerDisplayValue() {
                return RubyOps.GetDebuggerDisplayValue(_immediateClass, this);
            }

            private string GetDebuggerDisplayType() {
                return RubyOps.GetDebuggerDisplayType(_immediateClass);
            }
        }
    }
    public partial class Proc {
        [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
        [DebuggerDisplay(RubyObject.DebuggerDisplayValueStr, Type = RubyObject.DebuggerDisplayTypeStr)]
        public sealed partial class Subclass : Proc, IRubyObject {
            private RubyInstanceData _instanceData;
            private RubyClass/*!*/ _immediateClass;

            [Emitted]
            public RubyClass/*!*/ ImmediateClass {
                get {
                    return _immediateClass;
                }
                set {
                    // once a singleton immediate class is set it can't be changed:
                    Debug.Assert((_immediateClass == null || !_immediateClass.IsSingletonClass) && value != null);
                    _immediateClass = value;
                }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            public int BaseGetHashCode() {
                return base.GetHashCode();
            }

            public bool BaseEquals(object other) {
                return base.Equals(other);
            }

            public string/*!*/ BaseToString() {
                return base.ToString();
            }

            private string GetDebuggerDisplayValue() {
                return RubyOps.GetDebuggerDisplayValue(_immediateClass, this);
            }

            private string GetDebuggerDisplayType() {
                return RubyOps.GetDebuggerDisplayType(_immediateClass);
            }
        }
    }
    public partial class Range {
        [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
        [DebuggerDisplay(RubyObject.DebuggerDisplayValueStr, Type = RubyObject.DebuggerDisplayTypeStr)]
        public sealed partial class Subclass : Range, IRubyObject {
            private RubyInstanceData _instanceData;
            private RubyClass/*!*/ _immediateClass;

            [Emitted]
            public RubyClass/*!*/ ImmediateClass {
                get {
                    return _immediateClass;
                }
                set {
                    // once a singleton immediate class is set it can't be changed:
                    Debug.Assert((_immediateClass == null || !_immediateClass.IsSingletonClass) && value != null);
                    _immediateClass = value;
                }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            public int BaseGetHashCode() {
                return base.GetHashCode();
            }

            public bool BaseEquals(object other) {
                return base.Equals(other);
            }

            public string/*!*/ BaseToString() {
                return base.ToString();
            }

            private string GetDebuggerDisplayValue() {
                return RubyOps.GetDebuggerDisplayValue(_immediateClass, this);
            }

            private string GetDebuggerDisplayType() {
                return RubyOps.GetDebuggerDisplayType(_immediateClass);
            }
        }
    }
    public partial class RubyRegex {
        [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
        [DebuggerDisplay(RubyObject.DebuggerDisplayValueStr, Type = RubyObject.DebuggerDisplayTypeStr)]
        public sealed partial class Subclass : RubyRegex, IRubyObject {
            private RubyInstanceData _instanceData;
            private RubyClass/*!*/ _immediateClass;

            [Emitted]
            public RubyClass/*!*/ ImmediateClass {
                get {
                    return _immediateClass;
                }
                set {
                    // once a singleton immediate class is set it can't be changed:
                    Debug.Assert((_immediateClass == null || !_immediateClass.IsSingletonClass) && value != null);
                    _immediateClass = value;
                }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            public int BaseGetHashCode() {
                return base.GetHashCode();
            }

            public bool BaseEquals(object other) {
                return base.Equals(other);
            }

            public string/*!*/ BaseToString() {
                return base.ToString();
            }

            private string GetDebuggerDisplayValue() {
                return RubyOps.GetDebuggerDisplayValue(_immediateClass, this);
            }

            private string GetDebuggerDisplayType() {
                return RubyOps.GetDebuggerDisplayType(_immediateClass);
            }
        }
    }
    public partial class RubyIO {
        [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
        [DebuggerDisplay(RubyObject.DebuggerDisplayValueStr, Type = RubyObject.DebuggerDisplayTypeStr)]
        public sealed partial class Subclass : RubyIO, IRubyObject {
            private RubyInstanceData _instanceData;
            private RubyClass/*!*/ _immediateClass;

            [Emitted]
            public RubyClass/*!*/ ImmediateClass {
                get {
                    return _immediateClass;
                }
                set {
                    // once a singleton immediate class is set it can't be changed:
                    Debug.Assert((_immediateClass == null || !_immediateClass.IsSingletonClass) && value != null);
                    _immediateClass = value;
                }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            public int BaseGetHashCode() {
                return base.GetHashCode();
            }

            public bool BaseEquals(object other) {
                return base.Equals(other);
            }

            public string/*!*/ BaseToString() {
                return base.ToString();
            }

            private string GetDebuggerDisplayValue() {
                return RubyOps.GetDebuggerDisplayValue(_immediateClass, this);
            }

            private string GetDebuggerDisplayType() {
                return RubyOps.GetDebuggerDisplayType(_immediateClass);
            }
        }
    }
    public partial class RubyArray {
        [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
        [DebuggerDisplay(RubyObject.DebuggerDisplayValueStr, Type = RubyObject.DebuggerDisplayTypeStr)]
        public sealed partial class Subclass : RubyArray, IRubyObject {
            private RubyInstanceData _instanceData;
            private RubyClass/*!*/ _immediateClass;

            [Emitted]
            public RubyClass/*!*/ ImmediateClass {
                get {
                    return _immediateClass;
                }
                set {
                    // once a singleton immediate class is set it can't be changed:
                    Debug.Assert((_immediateClass == null || !_immediateClass.IsSingletonClass) && value != null);
                    _immediateClass = value;
                }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            public int BaseGetHashCode() {
                return base.GetHashCode();
            }

            public bool BaseEquals(object other) {
                return base.Equals(other);
            }

            public string/*!*/ BaseToString() {
                return base.ToString();
            }

            private string GetDebuggerDisplayValue() {
                return RubyOps.GetDebuggerDisplayValue(_immediateClass, this);
            }

            private string GetDebuggerDisplayType() {
                return RubyOps.GetDebuggerDisplayType(_immediateClass);
            }
        }
    }
    public partial class Hash {
        [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
        [DebuggerDisplay(RubyObject.DebuggerDisplayValueStr, Type = RubyObject.DebuggerDisplayTypeStr)]
        public sealed partial class Subclass : Hash, IRubyObject {
            private RubyInstanceData _instanceData;
            private RubyClass/*!*/ _immediateClass;

            [Emitted]
            public RubyClass/*!*/ ImmediateClass {
                get {
                    return _immediateClass;
                }
                set {
                    // once a singleton immediate class is set it can't be changed:
                    Debug.Assert((_immediateClass == null || !_immediateClass.IsSingletonClass) && value != null);
                    _immediateClass = value;
                }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            public int BaseGetHashCode() {
                return base.GetHashCode();
            }

            public bool BaseEquals(object other) {
                return base.Equals(other);
            }

            public string/*!*/ BaseToString() {
                return base.ToString();
            }

            private string GetDebuggerDisplayValue() {
                return RubyOps.GetDebuggerDisplayValue(_immediateClass, this);
            }

            private string GetDebuggerDisplayType() {
                return RubyOps.GetDebuggerDisplayType(_immediateClass);
            }
        }
    }
    public partial class MatchData {
        [DebuggerTypeProxy(typeof(RubyObjectDebugView))]
        [DebuggerDisplay(RubyObject.DebuggerDisplayValueStr, Type = RubyObject.DebuggerDisplayTypeStr)]
        public sealed partial class Subclass : MatchData, IRubyObject {
            private RubyInstanceData _instanceData;
            private RubyClass/*!*/ _immediateClass;

            [Emitted]
            public RubyClass/*!*/ ImmediateClass {
                get {
                    return _immediateClass;
                }
                set {
                    // once a singleton immediate class is set it can't be changed:
                    Debug.Assert((_immediateClass == null || !_immediateClass.IsSingletonClass) && value != null);
                    _immediateClass = value;
                }
            }

            public RubyInstanceData/*!*/ GetInstanceData() {
                return RubyOps.GetInstanceData(ref _instanceData);
            }

            public RubyInstanceData TryGetInstanceData() {
                return _instanceData;
            }

            public int BaseGetHashCode() {
                return base.GetHashCode();
            }

            public bool BaseEquals(object other) {
                return base.Equals(other);
            }

            public string/*!*/ BaseToString() {
                return base.ToString();
            }

            private string GetDebuggerDisplayValue() {
                return RubyOps.GetDebuggerDisplayValue(_immediateClass, this);
            }

            private string GetDebuggerDisplayType() {
                return RubyOps.GetDebuggerDisplayType(_immediateClass);
            }
        }
    }
#endregion

    //
    // IRubyObjectState implementation
    //

#if GENERATOR
    Stateless = [:Proc, :Range, :RubyRegex, :RubyIO, :Exception]

    def generate
      (Stateless - [:Exception]).each do |cls| 
        @class = cls
        super
      end
    end

    def superclass
      @class
    end
#else
    public partial class /*$superclass{*/RubyException/*}*/ {
        public sealed partial class Subclass : IRubyObject {
            public bool IsFrozen {
                get { return _instanceData != null && _instanceData.IsFrozen; }
            }

            public bool IsTainted {
                get { return _instanceData != null && _instanceData.IsTainted; }
                set { GetInstanceData().IsTainted = value; }
            }

            public bool IsUntrusted {
                get { return _instanceData != null && _instanceData.IsUntrusted; }
                set { GetInstanceData().IsUntrusted = value; }
            }

            public void Freeze() {
                GetInstanceData().Freeze();
            }
        }
    }
#endif
#region Generated
    public partial class Proc {
        public sealed partial class Subclass : IRubyObject {
            public bool IsFrozen {
                get { return _instanceData != null && _instanceData.IsFrozen; }
            }

            public bool IsTainted {
                get { return _instanceData != null && _instanceData.IsTainted; }
                set { GetInstanceData().IsTainted = value; }
            }

            public bool IsUntrusted {
                get { return _instanceData != null && _instanceData.IsUntrusted; }
                set { GetInstanceData().IsUntrusted = value; }
            }

            public void Freeze() {
                GetInstanceData().Freeze();
            }
        }
    }
    public partial class Range {
        public sealed partial class Subclass : IRubyObject {
            public bool IsFrozen {
                get { return _instanceData != null && _instanceData.IsFrozen; }
            }

            public bool IsTainted {
                get { return _instanceData != null && _instanceData.IsTainted; }
                set { GetInstanceData().IsTainted = value; }
            }

            public bool IsUntrusted {
                get { return _instanceData != null && _instanceData.IsUntrusted; }
                set { GetInstanceData().IsUntrusted = value; }
            }

            public void Freeze() {
                GetInstanceData().Freeze();
            }
        }
    }
    public partial class RubyRegex {
        public sealed partial class Subclass : IRubyObject {
            public bool IsFrozen {
                get { return _instanceData != null && _instanceData.IsFrozen; }
            }

            public bool IsTainted {
                get { return _instanceData != null && _instanceData.IsTainted; }
                set { GetInstanceData().IsTainted = value; }
            }

            public bool IsUntrusted {
                get { return _instanceData != null && _instanceData.IsUntrusted; }
                set { GetInstanceData().IsUntrusted = value; }
            }

            public void Freeze() {
                GetInstanceData().Freeze();
            }
        }
    }
    public partial class RubyIO {
        public sealed partial class Subclass : IRubyObject {
            public bool IsFrozen {
                get { return _instanceData != null && _instanceData.IsFrozen; }
            }

            public bool IsTainted {
                get { return _instanceData != null && _instanceData.IsTainted; }
                set { GetInstanceData().IsTainted = value; }
            }

            public bool IsUntrusted {
                get { return _instanceData != null && _instanceData.IsUntrusted; }
                set { GetInstanceData().IsUntrusted = value; }
            }

            public void Freeze() {
                GetInstanceData().Freeze();
            }
        }
    }
#endregion

    //
    // ISerializable implementation
    //
#if FEATURE_SERIALIZATION

#if GENERATOR
    Serializable = [:Exception]

    def generate
      (Serializable - [:Exception]).each do |cls| 
        @class = cls
        super
      end
    end

    def superclass
      @class
    end
#else
    public partial class /*$superclass{*/RubyException/*}*/ {
        [Serializable]
        public sealed partial class Subclass : /*$superclass{*/Exception/*}*/, ISerializable {
            private Subclass(SerializationInfo info, StreamingContext context)
                : base(info, context) {
                RubyOps.DeserializeObject(out _instanceData, out _immediateClass, info);
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
                base.GetObjectData(info, context);
                RubyOps.SerializeObject(_instanceData, _immediateClass, info);
            }
        }
    }
#endif
#region Generated
    
#endregion

#endif
}
