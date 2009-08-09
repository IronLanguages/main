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

include Microsoft::Scripting::Silverlight
if Repl.current
  $stdout = Repl.current.output_buffer
  $stderr = Repl.current.output_buffer
end

def load_assembly_from_path(path)
  DynamicApplication.current.runtime.host.platform_adaptation_layer.load_assembly_from_path(path)
end

# From Silverlight 3 SDK
load_assembly_from_path "System.Windows.Controls.dll"
require 'System.Windows.Controls'

# From Silverlight 3 Toolkit
load_assembly_from_path "System.Windows.Controls.Toolkit.dll"
require 'System.Windows.Controls.Toolkit'

require 'gui_tutorial'

Application.Current.RootVisual = GuiTutorial::Window.current
