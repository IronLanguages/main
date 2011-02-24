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
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Enumerated list of the properties shown on the build property page
	/// </summary>
	internal enum BuildPropertyPageTag
	{
		OutputPath
	}

	/// <summary>
	/// Defines the properties on the build property page and the logic the binds the properties to project data (load and save)
	/// </summary>
	[CLSCompliant(false), ComVisible(true), Guid("9B3DEA40-7F29-4a17-87A4-00EE08E8241E")]
	public class BuildPropertyPage : SettingsPage
	{
		#region fields
		private string outputPath;

		public BuildPropertyPage()
		{
			this.Name = SR.GetString(SR.BuildCaption, CultureInfo.CurrentUICulture);
		}
		#endregion

		#region properties
		[SRCategoryAttribute(SR.BuildCaption)]
		[LocDisplayName(SR.OutputPath)]
		[SRDescriptionAttribute(SR.OutputPathDescription)]
		public string OutputPath
		{
			get { return this.outputPath; }
			set { this.outputPath = value; this.IsDirty = true; }
		}
		#endregion

		#region overridden methods
		public override string GetClassName()
		{
			return this.GetType().FullName;
		}

		protected override void BindProperties()
		{
			if(this.ProjectMgr == null)
			{
				Debug.Assert(false);
				return;
			}

			this.outputPath = this.GetConfigProperty(BuildPropertyPageTag.OutputPath.ToString());
		}

		protected override int ApplyChanges()
		{
			if(this.ProjectMgr == null)
			{
				Debug.Assert(false);
				return VSConstants.E_INVALIDARG;
			}

			this.SetConfigProperty(BuildPropertyPageTag.OutputPath.ToString(), this.outputPath);
			this.IsDirty = false;
			return VSConstants.S_OK;
		}
		#endregion
	}
}
