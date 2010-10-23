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

#if !SILVERLIGHT // ICustomTypeDescriptor
#if !CLR2
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions.Calls;
using Microsoft.Scripting.Utils;
using IronRuby.Builtins;
using IronRuby.Compiler.Generation;
using IronRuby.Runtime.Calls;

namespace IronRuby.Runtime {
    /// <summary>
    /// Helper class that all custom type descriptor implementations call for
    /// the bulk of their implementation.
    /// </summary>
    public static class CustomTypeDescHelpers {

        #region ICustomTypeDescriptor helper functions

        private static RubyClass/*!*/ GetImmediateClass(object self) {
            IRubyObject rubyObj = self as IRubyObject;
            ContractUtils.RequiresNotNull(rubyObj, "self");
            return rubyObj.ImmediateClass;
        }

        [Emitted]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "self")]
        public static AttributeCollection GetAttributes(object self) {
            return AttributeCollection.Empty;
        }

        [Emitted]
        public static string GetClassName(object self) {
            return GetImmediateClass(self).NominalClass.Name;
        }

        [Emitted]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "self")]
        public static string GetComponentName(object self) {
            return null;
        }

        [Emitted]
        public static TypeConverter GetConverter(object self) {
            return new TypeConverter();
        }

        [Emitted]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "self")]
        public static EventDescriptor GetDefaultEvent(object self) {
            return null;
        }

        [Emitted]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "self")]
        public static PropertyDescriptor GetDefaultProperty(object self) {
            return null;
        }

        [Emitted]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "self"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "editorBaseType")]
        public static object GetEditor(object self, Type editorBaseType) {
            return null;
        }

        [Emitted]
        public static EventDescriptorCollection GetEvents(object self, Attribute[] attributes) {
            // TODO: update when we support attributes on Ruby types
            if (attributes == null || attributes.Length == 0) {
                return GetEvents(self);
            }

            // you want things w/ attributes?  we don't have attributes!
            return EventDescriptorCollection.Empty;
        }

        [Emitted]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "self")]
        public static EventDescriptorCollection GetEvents(object self) {
            return EventDescriptorCollection.Empty;
        }

        [Emitted]
        public static PropertyDescriptorCollection GetProperties(object self) {
            return new PropertyDescriptorCollection(GetPropertiesImpl(self, new Attribute[0]));
        }

        [Emitted]
        public static PropertyDescriptorCollection GetProperties(object self, Attribute[] attributes) {
            return new PropertyDescriptorCollection(GetPropertiesImpl(self, attributes));
        }

        static PropertyDescriptor[] GetPropertiesImpl(object self, Attribute[] attributes) {
            bool ok = true;
            foreach (var attr in attributes) {
                if (attr.GetType() != typeof(BrowsableAttribute)) {
                    ok = false;
                    break;
                }
            }
            if (!ok) {
                return new PropertyDescriptor[0];
            }
            
            RubyClass immediateClass = GetImmediateClass(self);
            RubyContext context = immediateClass.Context;

            const int readable = 0x01;
            const int writable = 0x02;

            var properties = new Dictionary<string, int>();
            using (context.ClassHierarchyLocker()) {
                immediateClass.ForEachMember(true, RubyMethodAttributes.DefaultVisibility, delegate(string/*!*/ name, RubyModule/*!*/ module, RubyMemberInfo/*!*/ member) {
                    ExpressionType operatorType;
                    
                    int flag = 0;
                    if (member is RubyAttributeReaderInfo) {
                        flag = readable;
                    } else if (member is RubyAttributeWriterInfo) {
                        flag = writable;
                    } else if (name == "initialize" || RubyUtils.TryMapOperator(name, out operatorType) != 0) {
                        // Special case; never a property
                    } else {
                        int arity = member.GetArity();
                        if (arity == 0) {
                            flag = readable;
                        } else if (arity == 1 && name.LastCharacter() == '=') {
                            flag = writable;
                        }
                    }
                    if (flag != 0) {
                        if (flag == writable) {
                            name = name.Substring(0, name.Length - 1);
                        }
                        int oldFlag;
                        properties.TryGetValue(name, out oldFlag);
                        properties[name] = oldFlag | flag;
                    }
                });
            }

            var result = new List<PropertyDescriptor>(properties.Count);
            foreach (var pair in properties) {
                if (pair.Value == (readable | writable)) {
                    result.Add(new RubyPropertyDescriptor(context, pair.Key, self, immediateClass.GetUnderlyingSystemType()));
                }
            }
            return result.ToArray();
        }

        [Emitted]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pd")]
        public static object GetPropertyOwner(object self, PropertyDescriptor pd) {
            return self;
        }

        #endregion

#if EXPOSE_INSTANCE_VARS
        class RubyInstanceVariableDescriptor : PropertyDescriptor {
            private readonly string _name;
            private readonly Type _propertyType;
            private readonly Type _componentType;

            internal RubyInstanceVariableDescriptor(string name, Type propertyType, Type componentType)
                : base(name, null) {
                _name = name;
                _propertyType = propertyType;
                _componentType = componentType;
            }

            public override object GetValue(object component) {
                object value;
                GetClass(component).Context.TryGetInstanceVariable(component, _name, out value);
                return value;
            }
            public override void SetValue(object component, object value) {
                GetClass(component).Context.SetInstanceVariable(component, _name, value);
            }

            public override bool CanResetValue(object component) {
                return false;
            }

            public override Type ComponentType {
                get { return _componentType; }
            }

            public override bool IsReadOnly {
                get { return false; }
            }

            public override Type PropertyType {
                get { return _propertyType; }
            }

            public override void ResetValue(object component) {
            }

            public override bool ShouldSerializeValue(object component) {
                return false;
            }
        }
#endif // EXPOSE_INSTANCE_VARS

        class RubyPropertyDescriptor : PropertyDescriptor {
            private readonly string/*!*/ _name;
            private readonly Type/*!*/ _propertyType;
            private readonly Type/*!*/ _componentType;
            private readonly CallSite<Func<CallSite, object, object>>/*!*/ _getterSite;
            private readonly CallSite<Func<CallSite, object, object, object>>/*!*/ _setterSite;

            internal RubyPropertyDescriptor(RubyContext/*!*/ context, string/*!*/ name, object testObject, Type/*!*/ componentType)
                : base(name, null) {
                _name = name;
                _componentType = componentType;

                _getterSite = CallSite<Func<CallSite, object, object>>.Create(
                    RubyCallAction.Make(context, _name, RubyCallSignature.WithImplicitSelf(0))
                );

                _setterSite = CallSite<Func<CallSite, object, object, object>>.Create(
                    RubyCallAction.Make(context, _name + "=", RubyCallSignature.WithImplicitSelf(0))
                );

                try {
                    var value = GetValue(testObject);
                    _propertyType = (value != null) ? value.GetType() : typeof(object);
                } catch (Exception) {
                    _propertyType = typeof(object);
                }
            }

            public override object GetValue(object obj) {
                return _getterSite.Target.Invoke(_getterSite, obj);
            }

            public override void SetValue(object obj, object value) {
                if (_setterSite != null) {
                    _setterSite.Target.Invoke(_setterSite, obj, value);
                }
            }

            public override bool CanResetValue(object component) {
                return false;
            }

            public override Type ComponentType {
                get { return _componentType; }
            }

            public override bool IsReadOnly {
                get { return (_setterSite == null); }
            }

            public override Type PropertyType {
                get { return _propertyType; }
            }

            public override void ResetValue(object component) {
            }

            public override bool ShouldSerializeValue(object component) {
                return false;
            }
        }

#if TODO
        private class TypeConv : TypeConverter {
            object convObj;

            public TypeConv(object self) {
                convObj = self;
            }

            #region TypeConverter overrides

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
                object result;
                return Converter.TryConvert(convObj, destinationType, out result);
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
                return Converter.CanConvertFrom(sourceType, convObj.GetType(), NarrowingLevel.All, true);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
                return Converter.Convert(value, convObj.GetType());
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
                return Converter.Convert(convObj, destinationType);
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
                return false;
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
                return false;
            }

            public override bool IsValid(ITypeDescriptorContext context, object value) {
                object result;
                return Converter.TryConvert(value, convObj.GetType(), out result);
            }

            #endregion
        }
#endif
    }
}

#endif
