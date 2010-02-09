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

MERLIN_ROOT           = (ENV['MERLIN_ROOT'] || File.expand_path(File.dirname(__FILE__) + '/../../..')).gsub(/\\/, '/')     # paths need forward slashes or Dir.glob isn't happy
PACKAGE_DIR           = mono? ? "#{MERLIN_ROOT}/../../dist/ironruby#{"-debug" if ENV['configuration'] == "debug"}" : 'c:/ironruby'  # directory that binary package is created in
PYTHON_PACKAGE_DIR    = mono? ? "#{MERLIN_ROOT}/../../python-dist/ironpython#{"-debug" if ENV['configuration'] == "debug"}" : 'c:/ironpython'  # directory that binary package is created in
BUILD_BIN             = "#{MERLIN_ROOT}/Bin/#{'mono_' if mono?}#{ENV['configuration'] || "release"}"
DIST_DIR              = mono? ? "#{MERLIN_ROOT}/../../pkg" : "C:/"        
IRONRUBY_VERSION      = `git rev-parse HEAD`[0..6] # replace to version for a release "0.9"

desc "Generate an IronRuby & IronPython binary redist package from the layout"
task :package => [:'package:ironruby']

namespace :package do         
  
  desc "Generate an IronRuby binary redist package from the layout"
  task :ironruby do
    # Directory layouts
    FileUtils.remove_dir(PACKAGE_DIR, true) if File.exist? PACKAGE_DIR
    FileUtils.mkdir_p "#{PACKAGE_DIR}/bin"

    # Copy Licenses
    FileUtils.cp Dir.glob("#{MERLIN_ROOT}/Languages/Ruby/Licenses/*"), PACKAGE_DIR

    # Copy binaries    
    # %w(ir ipy ipyw).each { |cmd| FileUtils.cp "#{BUILD_BIN}/#{cmd}.exe", "#{PACKAGE_DIR}/bin/" }
    %w(ir ir64).each { |cmd| FileUtils.cp "#{BUILD_BIN}/#{cmd}.exe", "#{PACKAGE_DIR}/bin/" }

    FileUtils.cp Dir.glob("#{BUILD_BIN}/IronRuby*.dll"), "#{PACKAGE_DIR}/bin"   
#    FileUtils.cp Dir.glob("#{BUILD_BIN}/IronPython*.dll"), "#{PACKAGE_DIR}/bin"
    FileUtils.cp Dir.glob("#{BUILD_BIN}/Microsoft.Scripting*.dll"), "#{PACKAGE_DIR}/bin"
    FileUtils.cp "#{BUILD_BIN}/Microsoft.Dynamic.dll", "#{PACKAGE_DIR}/bin"

    FileUtils.cp Dir.glob("#{MERLIN_ROOT}/Languages/Ruby/Scripts/bin/*"), "#{PACKAGE_DIR}/bin" 
#    FileUtils.cp Dir.glob("#{MERLIN_ROOT}/Languages/IronPython/Scripts/*"), "#{PACKAGE_DIR}/bin"

    # Generate ir.exe.config
    IronRubyCompiler.transform_config_file((mono? ? 'MonoRL' : 'Binary'), project_root + "Config/#{mono? ? "Unsigned" : "Signed"}/App.config", "#{PACKAGE_DIR}/bin/ir.exe.config")

    FileUtils.cp "#{PACKAGE_DIR}/bin/ir.exe.config", "#{PACKAGE_DIR}/bin/ir64.exe.config"

    # Copy standard library
    FileUtils.mkdir_p "#{PACKAGE_DIR}/lib" unless File.exist? "#{PACKAGE_DIR}/lib"
    FileUtils.cp_r "#{MERLIN_ROOT}/../External.LCA_RESTRICTED/Languages/Ruby/redist-libs/ruby", "#{PACKAGE_DIR}/lib/ruby"
    FileUtils.cp_r "#{MERLIN_ROOT}/Languages/Ruby/Libs", "#{PACKAGE_DIR}/lib/ironruby"

    FileUtils.cp_r "#{MERLIN_ROOT}/Languages/Ruby/Samples", "#{PACKAGE_DIR}/samples"    

    %w(igem iirb irackup irails irake irdoc iri ir).each { |exs| FileUtils.chmod 0755, "#{PACKAGE_DIR}/bin/#{exs}" } if mono?

    # Generate compressed package
    if ENV['ZIP']
      if mono?       
        FileUtils.mkdir_p DIST_DIR unless File.exist? DIST_DIR
#        system "rm #{DIST_DIR}/*"                    
        system "cd #{PACKAGE_DIR}; tar czf #{DIST_DIR}/ironruby-#{IRONRUBY_VERSION}.tar.gz *;cd #{MERLIN_ROOT}/../..;"
        system "cd #{PACKAGE_DIR}; tar cjf #{DIST_DIR}/ironruby-#{IRONRUBY_VERSION}.tar.bz2 *;cd #{MERLIN_ROOT}/../..;"
      else
        system %Q{del "#{ENV['TEMP']}\\ironruby.7z"}
        system %Q{"#{ENV['PROGRAM_FILES_32']}/7-Zip/7z.exe" a -bd -t7z -mx9 "#{ENV['TEMP']}\\ironruby.7z" "#{PACKAGE_DIR}\\"}
        system %Q{"#{ENV['PROGRAM_FILES_32']}/7-Zip/7z.exe" a -bd -tzip -mx9 "c:\\ironruby.zip" "#{PACKAGE_DIR}\\"}
        system %Q{copy /b /Y "#{ENV['PROGRAM_FILES_32']}\\7-Zip\\7zSD.sfx" + "#{MERLIN_ROOT}\\Languages\\Ruby\\sfx_config.txt" + "#{ENV['TEMP']}\\ironruby.7z" "c:\\ironruby.exe"}
      end
    end  


  end
  
  desc "Generate an IronPython binary redist package from the layout"
  task :ironpython do
    # Directory layouts
    FileUtils.remove_dir(PYTHON_PACKAGE_DIR, true) if File.exist? PYTHON_PACKAGE_DIR
    FileUtils.mkdir_p "#{PYTHON_PACKAGE_DIR}/bin"

    # Copy Licenses
    FileUtils.cp Dir.glob("#{MERLIN_ROOT}/Languages/Ruby/Licenses/*"), PYTHON_PACKAGE_DIR

    # Copy binaries    
    %w(ir ipy ipyw).each { |cmd| FileUtils.cp "#{BUILD_BIN}/#{cmd}.exe", "#{PYTHON_PACKAGE_DIR}/bin/" }

    FileUtils.cp Dir.glob("#{BUILD_BIN}/IronRuby*.dll"), "#{PYTHON_PACKAGE_DIR}/bin"   
    FileUtils.cp Dir.glob("#{BUILD_BIN}/IronPython*.dll"), "#{PYTHON_PACKAGE_DIR}/bin"
    FileUtils.cp Dir.glob("#{BUILD_BIN}/Microsoft.Scripting*.dll"), "#{PYTHON_PACKAGE_DIR}/bin"
    FileUtils.cp "#{BUILD_BIN}/Microsoft.Dynamic.dll", "#{PYTHON_PACKAGE_DIR}/bin"           
    
    %w(iirb irdoc iri ir).each { |exs| FileUtils.cp "#{MERLIN_ROOT}/Languages/Ruby/Scripts/bin/#{exs}", "#{PYTHON_PACKAGE_DIR}/bin" }   
    FileUtils.cp Dir.glob("#{MERLIN_ROOT}/Languages/IronPython/Scripts/*"), "#{PYTHON_PACKAGE_DIR}/bin"

    # Generate ir.exe.config
    IronRubyCompiler.transform_config_file((mono? ? 'MonoRL' : 'Binary'), project_root + "Config/#{mono? ? "Unsigned" : "Signed"}/App.config", "#{PYTHON_PACKAGE_DIR}/bin/ir.exe.config")

    # Copy standard library
    FileUtils.cp_r "#{MERLIN_ROOT}/Languages/IronPython/IronPython/Lib", "#{PYTHON_PACKAGE_DIR}/Lib"
    
    %w(ipy ipyw iirb irdoc iri ir).each { |exs| FileUtils.chmod 0755, "#{PYTHON_PACKAGE_DIR}/bin/#{exs}" } if mono?

    # Generate compressed package
    if ENV['ZIP']
      if mono?       
        FileUtils.mkdir_p DIST_DIR unless File.exist? DIST_DIR
#        system "rm #{DIST_DIR}/*"                    
        system "cd #{PYTHON_PACKAGE_DIR}; tar czf #{DIST_DIR}/ironpython-#{IRONRUBY_VERSION}.tar.gz *;cd #{MERLIN_ROOT}/../..;"
        system "cd #{PYTHON_PACKAGE_DIR}; tar cjf #{DIST_DIR}/ironpython-#{IRONRUBY_VERSION}.tar.bz2 *;cd #{MERLIN_ROOT}/../..;"
      else
        system %Q{del "#{ENV['TEMP']}\\ironpython.7z"}
        system %Q{"#{ENV['PROGRAM_FILES_32']}/7-Zip/7z.exe" a -bd -t7z -mx9 "#{ENV['TEMP']}\\ironpython.7z" "#{PYTHON_PACKAGE_DIR}\\"}
        system %Q{"#{ENV['PROGRAM_FILES_32']}/7-Zip/7z.exe" a -bd -tzip -mx9 "c:\\ironpython.zip" "#{PYTHON_PACKAGE_DIR}\\"}
        system %Q{copy /b /Y "#{ENV['PROGRAM_FILES_32']}\\7-Zip\\7zSD.sfx" + "#{MERLIN_ROOT}\\Languages\\Ruby\\sfx_config.txt" + "#{ENV['TEMP']}\\ironpython.7z" "c:\\ironpython.exe"}
      end
    end  


  end
end
