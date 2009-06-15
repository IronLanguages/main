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

desc "remove output files and generated debugging info from tests directory"
task :clean_tests do
  Dir.chdir("#{project_root + 'Languages/Ruby/Tests'}") do
    exec "del /s *.log"
    exec "del /s *.pdb"
    exec "del /s *.exe"
    exec "del /s *.dll"
  end
end

