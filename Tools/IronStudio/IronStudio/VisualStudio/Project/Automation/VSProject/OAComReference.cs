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
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.Project.Automation
{
	[SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	[ComVisible(true)]
	public class OAComReference : OAReferenceBase<ComReferenceNode>
	{
		public OAComReference(ComReferenceNode comReference) :
			base(comReference)
		{
		}

		#region Reference override
		public override string Culture
		{
			get
			{
				int locale = 0;
				try
				{
					locale = int.Parse(BaseReferenceNode.LCID, CultureInfo.InvariantCulture);
				}
				catch(System.FormatException)
				{
					// Do Nothing
				}
				if(0 == locale)
				{
					return string.Empty;
				}
				CultureInfo culture = new CultureInfo(locale);
				return culture.Name;
			}
		}
		public override string Identity
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", BaseReferenceNode.TypeGuid.ToString("B"), this.Version);
			}
		}
		public override int MajorVersion
		{
			get { return BaseReferenceNode.MajorVersionNumber; }
		}
		public override int MinorVersion
		{
			get { return BaseReferenceNode.MinorVersionNumber; }
		}
		public override string Name
		{
			get { return BaseReferenceNode.Caption; }
		}
		public override VSLangProj.prjReferenceType Type
		{
			get
			{
				return VSLangProj.prjReferenceType.prjReferenceTypeActiveX;
			}
		}
		public override string Version
		{
			get
			{
				Version version = new Version(BaseReferenceNode.MajorVersionNumber, BaseReferenceNode.MinorVersionNumber);
				return version.ToString();
			}
		}
		#endregion
	}
}
