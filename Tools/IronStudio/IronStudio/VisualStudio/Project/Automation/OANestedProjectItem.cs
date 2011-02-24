/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Project.Automation
{
	[SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	[ComVisible(true), CLSCompliant(false)]
	public class OANestedProjectItem : OAProjectItem<NestedProjectNode>
	{
		#region fields
		EnvDTE.Project nestedProject;
		#endregion

		#region ctors
		public OANestedProjectItem(OAProject project, NestedProjectNode node)
			: base(project, node)
		{
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

			object nestedproject;
			if(ErrorHandler.Succeeded(node.NestedHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out nestedproject)))
			{
				this.nestedProject = nestedproject as EnvDTE.Project;
			}
		}

		#endregion

		#region overridden methods
		/// <summary>
		/// Returns the collection of project items defined in the nested project
		/// </summary>
		public override EnvDTE.ProjectItems ProjectItems
		{
			get
			{
				if(this.nestedProject != null)
				{
					return this.nestedProject.ProjectItems;
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the nested project.
		/// </summary>
		public override EnvDTE.Project SubProject
		{
			get
			{
				return this.nestedProject;
			}
		}
		#endregion
	}
}
