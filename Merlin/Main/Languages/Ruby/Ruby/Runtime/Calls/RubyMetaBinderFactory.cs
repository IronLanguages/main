using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Utils;
using System.Threading;

namespace IronRuby.Runtime.Calls {
    internal sealed class RubyMetaBinderFactory {
        private static RubyMetaBinderFactory _Shared;

        /// <summary>
        /// Sites shared across runtimes.
        /// </summary>
        internal static RubyMetaBinderFactory Shared {
            get {
                if (_Shared == null) {
                    Interlocked.CompareExchange(ref _Shared, new RubyMetaBinderFactory(null), null);
                }
                return _Shared;
            }
        }

        private readonly RubyContext _context;

        // (name, signature) => binder:
        private readonly Dictionary<Key<string/*!*/, RubyCallSignature>, RubyCallAction/*!*/>/*!*/ _callActions;

        // (scope id, signature) => binder:
        private readonly Dictionary<Key<int, RubyCallSignature>, SuperCallAction/*!*/>/*!*/ _superCallActions;

        // (typeof(action)) => binder:
        private readonly Dictionary<Type, RubyConversionAction/*!*/>/*!*/ _conversionActions;

        // (CompositeConversion) => binder:
        private readonly Dictionary<CompositeConversion, CompositeConversionAction/*!*/>/*!*/ _compositeConversionActions;
        
        internal RubyMetaBinderFactory(RubyContext context) {
            _context = context;
            _callActions = new Dictionary<Key<string, RubyCallSignature>, RubyCallAction>();
            _superCallActions = new Dictionary<Key<int, RubyCallSignature>, SuperCallAction>();
            _conversionActions = new Dictionary<Type, RubyConversionAction>();
            _compositeConversionActions = new Dictionary<CompositeConversion, CompositeConversionAction>();
        }

        public RubyCallAction/*!*/ Call(string/*!*/ methodName, RubyCallSignature signature) {
            var key = Key.Create(methodName, signature);

            lock (_callActions) {
                RubyCallAction result;
                if (!_callActions.TryGetValue(key, out result)) {
                    _callActions.Add(key, result = new RubyCallAction(_context, methodName, signature));
                }
                return result;
            }
        }

        public SuperCallAction/*!*/ SuperCall(int/*!*/ lexicalScopeId, RubyCallSignature signature) {
            var key = Key.Create(lexicalScopeId, signature);

            lock (_superCallActions) {
                SuperCallAction result;
                if (!_superCallActions.TryGetValue(key, out result)) {
                    _superCallActions.Add(key, result = new SuperCallAction(_context, signature, lexicalScopeId));
                }
                return result;
            }
        }

        public TAction/*!*/ Conversion<TAction>() where TAction : RubyConversionAction, new() {
            var key = typeof(TAction);

            lock (_conversionActions) {
                RubyConversionAction result;
                if (!_conversionActions.TryGetValue(key, out result)) {
                    _conversionActions.Add(key, result = new TAction() { Context = _context });
                }
                return (TAction)result;
            }
        }

        public CompositeConversionAction/*!*/ CompositeConversion(CompositeConversion conversion) {
            var key = conversion;

            lock (_conversionActions) {
                CompositeConversionAction result;
                if (!_compositeConversionActions.TryGetValue(key, out result)) {
                    _compositeConversionActions.Add(key, result = CompositeConversionAction.Make(_context, conversion));
                }
                return result;
            }
        }
    }
}
 