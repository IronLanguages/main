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
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project
{
	/// <summary>
	/// Defines the config dependent properties exposed through automation
	/// </summary>
	[ComVisible(true)]
	[Guid("21f73a8f-91d7-4085-9d4f-c48ee235ee5b")]
	public interface IProjectConfigProperties
	{
		string OutputPath { get; set; }
	}

	/// <summary>
	/// Implements the configuration dependent properties interface
	/// </summary>
	[CLSCompliant(false), ComVisible(true)]
	[ClassInterface(ClassInterfaceType.None)]
	public class ProjectConfigProperties : IProjectConfigProperties
	{
		#region fields
		private ProjectConfig projectConfig;
		#endregion

		#region ctors
		public ProjectConfigProperties(ProjectConfig projectConfig)
		{
			this.projectConfig = projectConfig;
		}
		#endregion

		#region IProjectConfigProperties Members

		public virtual string OutputPath
		{
			get
			{
				return this.projectConfig.GetConfigurationProperty(BuildPropertyPageTag.OutputPath.ToString(), true);
			}
			set
			{
				this.projectConfig.SetConfigurationProperty(BuildPropertyPageTag.OutputPath.ToString(), value);
			}
		}

		#endregion
	}
}
