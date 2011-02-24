using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project {
    class BuildItemGroupCollectionShim : IEnumerable<BuildItemGroupShim> {
        private Build.BuildEngine.BuildItemGroupCollection buildItemGroupCollection;

        public BuildItemGroupCollectionShim(Build.BuildEngine.BuildItemGroupCollection buildItemGroupCollection) {
            // TODO: Complete member initialization
            this.buildItemGroupCollection = buildItemGroupCollection;
        }

        #region IEnumerable<BuildItemGroupShim> Members

        public IEnumerator<BuildItemGroupShim> GetEnumerator() {
            foreach (var v in buildItemGroupCollection) {
                yield return new BuildItemGroupShim((BuildItemGroup)v);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            foreach (var v in buildItemGroupCollection) {
                yield return new BuildItemGroupShim((BuildItemGroup)v);
            }
        }

        #endregion
    }
}
