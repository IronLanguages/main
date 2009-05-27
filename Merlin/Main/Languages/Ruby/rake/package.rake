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

PACKAGE_DIR           = 'c:\ironruby'  # directory that binary package is created in

desc "Generate an IronRuby binary redist package from the layout"
task :package do
  # Directory layouts
  system %Q{rmdir /S /Q #{PACKAGE_DIR}}
  FileUtils.mkdir_p "#{PACKAGE_DIR}\\bin"
  
  # Copy Licenses
  system %Q{copy "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\Licenses\\*" #{PACKAGE_DIR}}

  # Copy binaries
  system %Q{copy "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\IronRuby.BinaryLayout.config" "#{PACKAGE_DIR}\\bin\\ir.exe.config"}
  system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\ir.exe" #{PACKAGE_DIR}\\bin\\}
  system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\IronRuby*.dll" #{PACKAGE_DIR}\\bin\\}
  system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\Microsoft.Scripting.Core.dll" #{PACKAGE_DIR}\\bin\\}
  system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\Microsoft.Scripting.dll" #{PACKAGE_DIR}\\bin\\}
  system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\Microsoft.Dynamic.dll" #{PACKAGE_DIR}\\bin\\}
  system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\Microsoft.Scripting.ExtensionAttribute.dll" #{PACKAGE_DIR}\\bin\\}
  system %Q{copy "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\Scripts\\bin\\*" #{PACKAGE_DIR}\\bin\\}

  # Generate ir.exe.config
  IronRubyCompiler.transform_config_file 'Binary', project_root + "Config\\Signed\\app.config", "#{PACKAGE_DIR}\\bin\\ir.exe.config"

  # Copy standard library
  system %Q{xcopy /E /I "#{ENV['MERLIN_ROOT']}\\..\\External.LCA_RESTRICTED\\Languages\\Ruby\\redist-libs\\ruby" #{PACKAGE_DIR}\\lib\\ruby}
  system %Q{xcopy /E /I "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\Libs" #{PACKAGE_DIR}\\lib\\IronRuby}

  # Generate compressed package
  if ENV['ZIP']
    system %Q{del "#{ENV['TEMP']}\\ironruby.7z"}
    system %Q{"#{ENV['PROGRAM_FILES_32']}/7-Zip/7z.exe" a -bd -t7z -mx9 "#{ENV['TEMP']}\\ironruby.7z" "#{PACKAGE_DIR}\\"}
    system %Q{"#{ENV['PROGRAM_FILES_32']}/7-Zip/7z.exe" a -bd -tzip -mx9 "c:\\ironruby.zip" "#{PACKAGE_DIR}\\"}
    system %Q{copy /b /Y "#{ENV['PROGRAM_FILES_32']}\\7-Zip\\7zSD.sfx" + "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\sfx_config.txt" + "#{ENV['TEMP']}\\ironruby.7z" "c:\\ironruby.exe"}
  end
end
