using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace Microsoft.Scripting.Utils {
    /// <summary>
    /// Caches type member lookup.
    /// </summary>
    /// <remarks>
    /// When enumerating members (methods, properties, events) of a type (declared or inherited) Reflection enumerates all
    /// runtime members of the type and its base types and caches the result.
    /// When looking for a member of a specific name Reflection still enumerates all and filters out those that don't match the name.
    /// That's inefficient when looking for members of multiple names one by one.
    /// Instead we build a map of name to member list and then answer subsequent queries by simply looking up the dictionary.
    /// </remarks>
    public sealed class TypeMemberCache<T>
        where T : MemberInfo {

        // TODO: some memory can be saved here
        // { queried-type -> immutable { member-name, members } }
#if CLR2 || WP75
        private readonly Dictionary<Type, Dictionary<string, List<T>>> _typeMembersByName =
            new Dictionary<Type, Dictionary<string, List<T>>>();

        private Dictionary<string, List<T>> GetMembers(Type type) {
            lock (_typeMembersByName) {
                Dictionary<string, List<T>> result;
                if (_typeMembersByName.TryGetValue(type, out result)) {
                    return result;
                }

                result = ReflectMembers(type);
                _typeMembersByName[type] = result;
                return result;
            }
        }
#else
        private readonly ConditionalWeakTable<Type, Dictionary<string, List<T>>> _typeMembersByName = 
            new ConditionalWeakTable<Type, Dictionary<string, List<T>>>();

        private Dictionary<string, List<T>> GetMembers(Type type) {
            return _typeMembersByName.GetValue(type, t => ReflectMembers(t));
        }
#endif

        private readonly Func<Type, IEnumerable<T>> _reflector;

        public TypeMemberCache(Func<Type, IEnumerable<T>> reflector) {
            _reflector = reflector;
        }

        public IEnumerable<T> GetMembers(Type type, string name = null, bool inherited = false) {
            var membersByName = GetMembers(type);

            if (name == null) {
                var allMembers = membersByName.Values.SelectMany(memberList => memberList);
                if (inherited) {
                    return allMembers;
                } else {
                    return allMembers.Where(overload => overload.DeclaringType == type);
                }
            }

            List<T> inheritedOverloads;
            if (!membersByName.TryGetValue(name, out inheritedOverloads)) {
                return Enumerable.Empty<T>();
            }

            if (inherited) {
                return new ReadOnlyCollection<T>(inheritedOverloads);
            }

            return inheritedOverloads.Where(overload => overload.DeclaringType == type);
        }

        private Dictionary<string, List<T>> ReflectMembers(Type type) {
            var result = new Dictionary<string, List<T>>();
            
            foreach (T member in _reflector(type)) {
                List<T> overloads;
                if (!result.TryGetValue(member.Name, out overloads)) {
                    result.Add(member.Name, overloads = new List<T>());
                }

                overloads.Add(member);
            }

            foreach (var list in result.Values) {
                list.TrimExcess();
            }

            return result;
        }
    }
}
