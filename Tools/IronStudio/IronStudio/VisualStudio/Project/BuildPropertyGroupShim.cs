using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project {
    class BuildPropertyGroupShim : IEnumerable<BuildPropertyShim> {
        private readonly BuildPropertyGroup _buildPropertyGroup;

        public BuildPropertyGroupShim(BuildPropertyGroup buildPropertyGroup) {
            _buildPropertyGroup = buildPropertyGroup;
        }

        public bool IsImported {
            get {
                return _buildPropertyGroup.IsImported;
            }
        }

        #region IEnumerable<BuildPropertyShim> Members

        public IEnumerator<BuildPropertyShim> GetEnumerator() {
            foreach (var property in _buildPropertyGroup) {
                yield return new BuildPropertyShim((BuildProperty)property);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }

        #endregion
    }
}
