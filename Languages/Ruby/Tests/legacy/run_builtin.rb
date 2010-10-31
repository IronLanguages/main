# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

require 'Find'

ARGV.each do |type|
  Find.find "Builtin/#{type}" do |path|
    if ( /_spec\.rb/ =~ path ) then
      print path, '(ruby): '
      system("ruby #{path}")
      puts
      print path, '(ir): '
      system("..\\..\\..\\Test\\Scripts\\ir.cmd #{path}")
      puts
    end
  end
end
