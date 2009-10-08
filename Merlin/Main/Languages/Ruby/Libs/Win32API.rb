# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

case System::Environment.OSVersion.Platform
  when System::PlatformID.Win32S:
  when System::PlatformID.Win32Windows:
  when System::PlatformID.Win32NT:
    load_assembly 'IronRuby.Libraries', 'IronRuby.StandardLibrary.Win32API'
  else
    raise LoadError, "Win32API is only available on Windows"
end


