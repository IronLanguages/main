/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;
using IronRuby.Compiler.Ast;
using Microsoft.Scripting;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.IronRubyTools.Intellisense;
using Microsoft.Scripting.Utils;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.IronRubyTools.Navigation {
    /// <summary>
    /// An enum which is synchronized with our image list for the various
    /// kinds of images which are available.  This can be combined with the 
    /// ImageListOverlay to select an image for the appropriate member type
    /// and indicate the appropiate visiblity.  These can be combined with
    /// GetImageListIndex to get the final index.
    /// 
    /// Most of these are unused as we're just using an image list shipped
    /// by the VS SDK.
    /// </summary>
    internal enum ImageListKind {
        Class,
        Unknown1,
        Unknown2,
        Enum,
        Unknown3,
        Lightning,
        Unknown4,
        BlueBox,
        Key,
        BlueStripe,
        ThreeDashes,
        TwoBoxes,
        Method,
        StaticMethod,
        Unknown6,
        Namespace,
        Unknown7,
        Property,
        Unknown8,
        Unknown9,
        Unknown10,
        Unknown11,
        Unknown12,
        Unknown13,
        ClassMethod
    }

    /// <summary>
    /// Indicates the overlay kind which should be used for a drop down members
    /// image.  The overlay kind typically indicates visibility.
    /// 
    /// Most of these are unused as we're just using an image list shipped
    /// by the VS SDK.
    /// </summary>
    internal enum ImageListOverlay {
        ImageListOverlayNone,
        ImageListOverlayLetter,
        ImageListOverlayBlue,
        ImageListOverlayKey,
        ImageListOverlayPrivate,
        ImageListOverlayArrow,
    }

    internal sealed class Model {
        private readonly ModuleEntry/*!*/ _top;
        private readonly ModuleEntry[]/*!*/ _moduleEntries;                           // entries for modules of the file

        public Model(ModuleEntry/*!*/ top, ModuleEntry[]/*!*/ moduleEntries) {
            _top = top;
            _moduleEntries = moduleEntries;
        }

        public int ModuleCount {
            get { return _moduleEntries.Length; }
        }

        internal ModuleEntry/*!*/ LocateModule(int position) {
            return (ModuleEntry)_top.LocateModule(position);
        }

        internal int GetModuleImageIndex(int index) {
            return (index >= 0 && index < _moduleEntries.Length) ? _moduleEntries[index].ImageListIndex : 0;
        }

        internal string GetModuleName(int index) {
            return (index >= 0 && index < _moduleEntries.Length) ? _moduleEntries[index].DisplayName : null;
        }

        internal int GetModuleStart(int index) {
            return (index >= 0 && index < _moduleEntries.Length) ? _moduleEntries[index].Start : -1;
        }

        internal int GetMethodCount(int moduleIndex) {
            if (moduleIndex < 0 || moduleIndex >= _moduleEntries.Length) {
                return 0;
            }

            return _moduleEntries[moduleIndex].GetSortedMethods().Length;
        }

        internal int GetMethodImageIndex(int moduleIndex, int methodIndex) {
            if (moduleIndex < 0 || methodIndex < 0 || moduleIndex >= _moduleEntries.Length) {
                return 0;
            }

            var methods = _moduleEntries[moduleIndex].GetSortedMethods();
            return (methodIndex >= 0 && methodIndex < methods.Length) ? methods[methodIndex].ImageListIndex : 0;
        }

        internal string GetMethodName(int moduleIndex, int methodIndex) {
            if (moduleIndex < 0 || methodIndex < 0 || moduleIndex >= _moduleEntries.Length) {
                return null;
            }

            var methods = _moduleEntries[moduleIndex].GetSortedMethods();
            return (methodIndex < methods.Length) ? methods[methodIndex].DisplayName : null;
        }

        internal int GetMethodStart(int moduleIndex, int methodIndex) {
            if (moduleIndex < 0 || methodIndex < 0 || moduleIndex >= _moduleEntries.Length) {
                return -1;
            }

            var methods = _moduleEntries[moduleIndex].GetSortedMethods();
            return (methodIndex < methods.Length) ? methods[methodIndex].Start : -1;
        }
    }

    internal abstract class EntryBase {
        private int _index;
        private readonly string/*!*/ _displayName;
        private readonly Node/*!*/ _definition;

        public EntryBase(string/*!*/ displayName, Node/*!*/ definition) {
            Assert.NotNull(definition, displayName);
            _displayName = displayName;
            _definition = definition;
        }

        public string/*!*/ DisplayName {
            get { return _displayName; }
        }

        public int Index {
            get { return _index; }
            set { _index = value; }
        }

        internal bool Includes(int position) {
            // top-level "entry" includes all positions:
            return _definition.NodeType == NodeTypes.SourceUnitTree || position >= Start && position < End;
        }

        public Node/*!*/ Definition {
            get { return _definition; }
        }

        public int Start {
            get { return _definition.Location.Start.Index; }
        }

        public int End {
            get { return _definition.Location.End.Index; }
        }

        public abstract int ImageListIndex { get; }

        /// <summary>
        /// Turns an image list kind / overlay into the proper index in the image list.
        /// </summary>
        internal static int GetImageListIndex(ImageListKind kind, ImageListOverlay overlay) {
            return ((int)kind) * 6 + (int)overlay;
        }
    }

    [DebuggerDisplay("{DisplayName}")]
    internal sealed class ModuleEntry : EntryBase {
        private readonly bool _isExpressionBound;
        private readonly bool _isSingleton;
        private readonly string/*!*/ _qualifiedName;
        private List<ModuleEntry> _nestedModules;

        // methods in the order as they appear in the AST (order by location):
        private List<MethodEntry> _methods;

        // methods in the display order:
        private MethodEntry[] _sortedMethods;

        public ModuleEntry(string/*!*/ displayName, string/*!*/ qualifiedName, Node/*!*/ definition, bool isExpressionBound, bool isSingleton)
            : base(displayName, definition) {
            _qualifiedName = qualifiedName;
            _isExpressionBound = isExpressionBound;
            _isSingleton = isSingleton;
        }

        public bool IsExpressionBound {
            get { return _isExpressionBound; }
        }

        public bool IsSingleton {
            get { return _isSingleton; }
        }

        public string/*!*/ QualifiedName {
            get { return _qualifiedName; }
        }

        public void AddNested(ModuleEntry/*!*/ module) {
            if (_nestedModules == null) {
                _nestedModules = new List<ModuleEntry>();
            } else {
                // children need to be sorted by position:
                Debug.Assert(_nestedModules.Last().End <= module.Start);
            }
            _nestedModules.Add(module);
        }

        public ModuleEntry/*!*/ LocateModule(int position) {
            // binary search nested entries:
            int start = 0;
            int end = _nestedModules != null ? _nestedModules.Count : 0;
            while (start < end) {
                int mid = (start + end) / 2;
                var midEntry = _nestedModules[mid];
                if (position < midEntry.Start) {
                    end = mid;
                } else if (position >= midEntry.End) {
                    start = mid + 1;
                } else {
                    return midEntry.LocateModule(position);
                }
            }
            return this;
        }

        public MethodEntry LocateMethod(int position) {
            return MethodEntry.LocateMethod(_methods, position);
        }

        public void AddMethod(MethodEntry/*!*/ method) {
            if (_methods == null) {
                _methods = new List<MethodEntry>();
            }
            _methods.Add(method);
        }

        public MethodEntry[]/*!*/ GetSortedMethods() {
            if (_sortedMethods == null) {
                if (_methods != null) {
                    var array = _methods.ToArray();

                    Array.Sort(array, (x, y) => String.CompareOrdinal(x.DisplayName, y.DisplayName));
                    for (int i = 0; i < array.Length; i++) {
                        array[i].Index = i;
                    }

                    _sortedMethods = array;
                } else {
                    _sortedMethods = MethodEntry.EmptyArray;
                }
            }

            return _sortedMethods;
        }

        public override int ImageListIndex {
            get { return GetImageListIndex(ImageListKind.Class, ImageListOverlay.ImageListOverlayNone); }
        }
    }

    [DebuggerDisplay("{DisplayName}")]
    internal sealed class MethodEntry : EntryBase {
        public static readonly MethodEntry[] EmptyArray = new MethodEntry[0];

        private List<MethodEntry> _nestedMethods;
        private readonly bool _isSingleton;

        public MethodEntry(MethodDefinition/*!*/ definition, bool isSingleton)
            : base(definition.Name, definition) {
            _isSingleton = isSingleton;
        }

        public bool IsSingleton {
            get { return _isSingleton; }
        }

        public override int ImageListIndex {
            get { return GetImageListIndex(_isSingleton ? ImageListKind.ClassMethod : ImageListKind.Method, ImageListOverlay.ImageListOverlayNone); }
        }

        public void AddNested(MethodEntry/*!*/ method) {
            if (_nestedMethods == null) {
                _nestedMethods = new List<MethodEntry>();
            } else {
                // children need to be sorted by position:
                Debug.Assert(_nestedMethods.Last().End <= method.Start);
            }
            _nestedMethods.Add(method);
        }

        public MethodEntry/*!*/ LocateMethod(int position) {
            return LocateMethod(_nestedMethods, position) ?? this;
        }

        public static MethodEntry LocateMethod(IList<MethodEntry> methods, int position) {
            // binary search nested entries:
            int start = 0;
            int end = methods != null ? methods.Count : 0;
            while (start < end) {
                int mid = (start + end) / 2;
                var midEntry = methods[mid];
                if (position < midEntry.Start) {
                    end = mid;
                } else if (position >= midEntry.End) {
                    start = mid + 1;
                } else {
                    return midEntry.LocateMethod(position);
                }
            }
            return null;
        }
    }

    internal sealed class ModelBuilder : Walker {
        private readonly SourceUnitTree/*!*/ _tree;
        private readonly List<string>/*!*/ _outerName; // stack
        private readonly Stack<ModuleEntry>/*!*/ _outerModules;
        private readonly Stack<MethodEntry>/*!*/ _outerMethods;
        private readonly Dictionary<string, int>/*!*/ _definitions;
        private readonly List<ModuleEntry>/*!*/ _entries;

        public ModelBuilder(SourceUnitTree/*!*/ tree) {
            _tree = tree;
            var top = new ModuleEntry("<main>", "<main>", tree, false, false);

            _outerName = new List<string>();
            _outerName.Add(null);

            _outerModules = new Stack<ModuleEntry>();
            _outerModules.Push(top);

            _outerMethods = new Stack<MethodEntry>();

            _definitions = new Dictionary<string, int>();
            _entries = new List<ModuleEntry>();
            _entries.Add(top);
        }

        public Model/*!*/ Build() {
            Walk(_tree);
            Debug.Assert(_outerModules.Count == 1 && _outerName.Count == 1);

            var result = _entries.ToArray();

            Array.Sort(result, (x, y) =>
                x.IsExpressionBound == y.IsExpressionBound ? String.CompareOrdinal(x.DisplayName, y.DisplayName) : x.IsExpressionBound ? +1 : -1
            );

            for (int i = 0; i < result.Length; i++) {
                result[i].Index = i;
            }

            return new Model(_outerModules.Pop(), result);
        }

        private static string/*!*/ BuildName(string prefix, ConstantVariable/*!*/ constant) {
            var reversed = new List<string>();
            ConstantVariable constantQualifier;
            while (true) {
                reversed.Add(constant.Name);
                constantQualifier = constant.Qualifier as ConstantVariable;
                if (constantQualifier == null) {
                    break;
                }
                constant = constantQualifier;
            }

            // A::B
            // ::A::B
            // <expr>::A::B
            if (!constant.IsGlobal) {
                if (constant.IsBound) {
                    reversed.Add("<object>");
                } else if (prefix != null) {
                    reversed.Add(prefix);
                }
            }

            return String.Join("::", Enumerable.Reverse(reversed));
        }

        private bool IsSelfModuleReference(Expression target, out string moduleName) {
            if (target == null) {
                moduleName = null;
                return false;
            }

            if (target.NodeType == NodeTypes.SelfReference) {
                moduleName = _outerModules.Peek().QualifiedName;
                return true;
            }

            if (target.NodeType == NodeTypes.ConstantVariable) {
                string prefix = (_outerName.Count >= 2) ? _outerName[_outerName.Count - 2] : null;
                moduleName = BuildName(prefix, (ConstantVariable)target);
                return String.Equals(moduleName, _outerModules.Peek().QualifiedName, StringComparison.Ordinal);
            }

            moduleName = "<object>";
            return false;
        }

        private void EnterModule(string/*!*/ name, Node/*!*/ definition, bool isExpressionBound, bool isSingleton) {
            string displayName = name;
            int count;
            if (_definitions.TryGetValue(name, out count)) {
                count++;
                displayName = name + " (" + count + ")";
            } else {
                count = 1;
            }
            _definitions[name] = count;

            var entry = new ModuleEntry(displayName, name, definition, isExpressionBound, isSingleton);

            _entries.Add(entry);
            _outerModules.Peek().AddNested(entry);
            _outerName.Add(name);
            _outerModules.Push(entry);

            // add a boundary for method nesting:
            _outerMethods.Push(null);
        }

        private void ExitModule() {
            _outerMethods.Pop();
            _outerModules.Pop();
            _outerName.RemoveAt(_outerName.Count - 1);
        }

        public override bool Enter(ModuleDefinition/*!*/ node) {
            var name = BuildName(_outerName.Last(), node.QualifiedName);
            EnterModule(name, node, name[0] == '<', false);
            return true;
        }

        public override void Exit(ModuleDefinition/*!*/ node) {
            ExitModule();
        }

        public override bool Enter(ClassDefinition/*!*/ node) {
            return Enter((ModuleDefinition)node);
        }

        public override void Exit(ClassDefinition/*!*/ node) {
            Exit((ModuleDefinition)node);
        }

        public override bool Enter(SingletonDefinition/*!*/ node) {
            string name;
            bool isSelfSingleton = IsSelfModuleReference(node.Singleton, out name);
            Debug.Assert(name != null);
            name += ".singleton";
            EnterModule(name, node, name[0] == '<', isSelfSingleton);
            return true;
        }

        public override void Exit(SingletonDefinition/*!*/ node) {
            Exit((ModuleDefinition)node);
        }

        public override bool Enter(MethodDefinition/*!*/ node) {
            var outerModule = _outerModules.Peek();

            string targetName;
            bool isSelfSingleton =
                !outerModule.IsSingleton && IsSelfModuleReference(node.Target, out targetName) ||
                outerModule.IsSingleton && node.Target == null;

            var entry = new MethodEntry(node, isSelfSingleton);

            // add to the outer method:
            if (_outerMethods.Count > 0) {
                var outerMethod = _outerMethods.Peek();
                if (outerMethod != null) {
                    outerMethod.AddNested(entry);
                }
            }

            // add to the outer module:
            // TODO: lookup target name...
            if (isSelfSingleton || node.Target == null) {
                outerModule.AddMethod(entry);
            }

            _outerName.Add(null);
            _outerMethods.Push(entry);
            return true;
        }

        public override void Exit(MethodDefinition/*!*/ node) {
            _outerName.RemoveAt(_outerName.Count - 1);
            _outerMethods.Pop();
        }
    }
}
