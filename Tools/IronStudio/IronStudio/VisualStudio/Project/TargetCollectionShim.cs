using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.BuildEngine;

namespace Microsoft.VisualStudio.Project {
    class TargetCollectionShim : IEnumerable<TargetShim> {
        private Build.BuildEngine.TargetCollection targetCollection;

        public TargetCollectionShim(Build.BuildEngine.TargetCollection targetCollection) {
            this.targetCollection = targetCollection;
        }



        #region IEnumerable<Target> Members

        public IEnumerator<TargetShim> GetEnumerator() {
            foreach (var v in targetCollection) {
                yield return new TargetShim((Target)v);
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            foreach (var v in targetCollection) {
                yield return new TargetShim((Target)v);
            }
        }

        #endregion
    }
}
