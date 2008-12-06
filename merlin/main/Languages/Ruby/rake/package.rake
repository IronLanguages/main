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

def transform_dlr_project(csproj_filename) 
  transform_project(:dlr_core, csproj_filename) do |contents|
    replace_output_path contents, '..\..\..\..\..\..\Merlin\Main\Bin\Debug\\', '..\..\build\debug\\'
    replace_output_path contents, '..\..\..\..\..\..\Merlin\Main\Bin\Release\\', '..\..\build\release\\'
    replace_output_path contents, '..\..\..\..\..\..\Merlin\Main\Bin\Silverlight Debug\\', '..\..\build\silverlight debug\\'
    replace_output_path contents, '..\..\..\..\..\..\Merlin\Main\Bin\Silverlight Release\\', '..\..\build\silverlight release\\'
    replace_import_project contents, '..\..\..\..\..\..\Merlin\Main\SpecSharp.targets', '..\..\SpecSharp.targets'
    replace_doc_path contents,    '..\..\..\..\..\..\Merlin\Main\Bin\Debug\Microsoft.Scripting.Core.xml', '..\..\build\debug\Microsoft.Scripting.Core.xml'
    replace_doc_path contents,    '..\..\..\..\..\..\Merlin\Main\Bin\Release\Microsoft.Scripting.Core.xml', '..\..\build\release\Microsoft.Scripting.Core.xml'
    replace_doc_path contents,    '..\..\..\..\..\..\Merlin\Main\Bin\Silverlight Debug\Microsoft.Scripting.Core.xml', '..\..\build\silverlight debug\Microsoft.Scripting.Core.xml'
    replace_doc_path contents,    '..\..\..\..\..\..\Merlin\Main\Bin\Silverlight Release\Microsoft.Scripting.Core.xml', '..\..\build\silverlight release\Microsoft.Scripting.Core.xml'
    if IronRuby.is_merlin?
      contents.change_configuration! 'FxCop|AnyCPU', 'TRACE;MICROSOFT_SCRIPTING_CORE'
      contents.change_configuration! 'SpecSharp|AnyCPU', 'TRACE;DEBUG;MICROSOFT_SCRIPTING_CORE'
      contents.change_configuration! 'Debug|AnyCPU', 'TRACE;DEBUG;MICROSOFT_SCRIPTING_CORE'
      contents.change_configuration! 'Release|AnyCPU', 'TRACE;MICROSOFT_SCRIPTING_CORE'
      contents.change_configuration! 'Silverlight Debug|AnyCPU', 'TRACE;DEBUG;SILVERLIGHT;MICROSOFT_SCRIPTING_CORE'
      contents.change_configuration! 'Silverlight Release|AnyCPU', 'TRACE;SILVERLIGHT;MICROSOFT_SCRIPTING_CORE'
    else
      contents.change_configuration! 'FxCop|AnyCPU', 'TRACE;MICROSOFT_SCRIPTING_CORE;SIGNED'
      contents.change_configuration! 'SpecSharp|AnyCPU', 'TRACE;DEBUG;MICROSOFT_SCRIPTING_CORE;SIGNED'
      contents.change_configuration! 'Debug|AnyCPU', 'TRACE;DEBUG;MICROSOFT_SCRIPTING_CORE;SIGNED'
      contents.change_configuration! 'Release|AnyCPU', 'TRACE;MICROSOFT_SCRIPTING_CORE;SIGNED'
      contents.change_configuration! 'Silverlight Debug|AnyCPU', 'TRACE;DEBUG;SILVERLIGHT;MICROSOFT_SCRIPTING_CORE;SIGNED'
      contents.change_configuration! 'Silverlight Release|AnyCPU', 'TRACE;SILVERLIGHT;MICROSOFT_SCRIPTING_CORE;SIGNED'
    end
  end
end

def transform_config(source_path, target_path, signed, paths)
  file = File.new source_path
  doc = Document.new file

  # disable signing
  unless signed 
    configSections = XPath.each(doc, '/configuration/configSections/section') do |node|
      node.attributes['type'].gsub!(/31bf3856ad364e35/, 'null')
    end

    # disable signing in IronRuby and replace the paths
    languages = XPath.each(doc, '/configuration/microsoft.scripting/languages/language') do |node|
      if node.attributes['names'] == 'IronRuby;Ruby;rb'
        node.attributes['type'].gsub!(/31bf3856ad364e35/, 'null')
      end
    end
  end

  # replace LibraryPaths
  options = XPath.each(doc, '/configuration/microsoft.scripting/options/set') do |node|
    if node.attributes['language'] == 'Ruby' && node.attributes['option'] == 'LibraryPaths'
      node.attributes['value'] = paths
    end
  end

  File.open(target_path, 'w+') do |f|
    f.write doc.to_s
  end
end

def transform_config_file(configuration, source_path, target_build_path)
  # signing is on for IronRuby in Merlin, off for SVN and Binary
  layout = {'Merlin' => { :signing => false, :LibraryPaths => '..\..\Languages\Ruby\libs;..\..\..\External\Languages\Ruby\Ruby-1.8.6\lib\ruby\site_ruby\1.8;..\..\..\External\Languages\Ruby\Ruby-1.8.6\lib\ruby\site_ruby;..\..\..\External\Languages\Ruby\Ruby-1.8.6\lib\ruby\1.8' }, 
            'Svn'    => { :signing => false, :LibraryPaths => '..\..\lib\IronRuby;..\..\lib\ruby\site_ruby\1.8;..\..\lib\ruby\site_ruby;..\..\lib\ruby\1.8' },
            'Binary' => { :signing => true,  :LibraryPaths => '..\lib\IronRuby;..\lib\ruby\site_ruby\1.8;..\lib\ruby\site_ruby;..\lib\ruby\1.8' } }
  
  transform_config source_path, target_build_path, layout[configuration][:signing], layout[configuration][:LibraryPaths]
end
# Source repository synchronization tasks

def push
  IronRuby.source_context do
    rake_output_message "#{'=' * 78}\nTransforming source to target layout\n\n"

    # Copy to temporary directory and transform layout to match target layout
    # This lets us diff the temp target layout with the actual layout

    temp_dir = generate_temp_dir
    nodes = [:root, :gppg, :dlr_core, :dlr_libs, :dlr_com, :ironruby, :libraries, :tests, :console, :generator, :test_runner, :scanner, :yaml, :stdlibs, :ironlibs]
    nodes.each do |node|
      # special case tests directory to avoid filtering sub-directories
      if node == :tests
        copy_to_temp_dir node, temp_dir
      else
        copy_to_temp_dir node, temp_dir, ['bin'] # always filter out bin subdirectories except in tests
      end
    end

    # Do some post-transform filtering of files

    if IronRuby.is_merlin?
      del temp_dir, 'Ruby.sln'
    else
      puts "copying #{IronRuby.source + 'IronRuby.sln'} to #{temp_dir + 'Merlin/Main/languages/ruby/IronRuby.sln'}"
      copy IronRuby.source + 'IronRuby.sln', temp_dir + 'Merlin/Main/languages/ruby/Ruby.sln'
    end

    # Special-cased one-way copy of app.config to external layout

    if IronRuby.is_merlin?
      transform_config_file 'Svn', get_source_dir(:lang_root) + 'app.config', temp_dir + 'app.config'
    end

    # Diff and push temp directory files to the target

    push_to_target temp_dir

    # Transform the project files

    transform_project :ironruby, 'ruby.csproj'
    transform_project :libraries, 'ironruby.libraries.csproj'
    transform_dlr_project 'microsoft.scripting.core.csproj'
    transform_dlr_project 'microsoft.scripting.extensionattribute.csproj'
    transform_project(:dlr_libs, 'microsoft.scripting.csproj') do |contents|
      replace_output_path contents, '..\..\Bin\Debug\\', '..\..\build\debug\\'
      replace_output_path contents, '..\..\Bin\Release\\', '..\..\build\release\\'
      replace_output_path contents, '..\..\Bin\Silverlight Debug\\', '..\..\build\silverlight debug\\'
      replace_output_path contents, '..\..\Bin\Silverlight Release\\', '..\..\build\silverlight release\\'
      replace_doc_path    contents, '..\..\Bin\Debug\Microsoft.Scripting.xml', '..\..\build\debug\Microsoft.Scripting.xml'
      replace_doc_path    contents, '..\..\Bin\Release\Microsoft.Scripting.xml', '..\..\build\release\Microsoft.Scripting.xml'
      replace_doc_path    contents, '..\..\Bin\Silverlight Debug\Microsoft.Scripting.xml', '..\..\build\silverlight debug\Microsoft.Scripting.xml'
      replace_doc_path    contents, '..\..\Bin\Silverlight Release\Microsoft.Scripting.xml', '..\..\build\silverlight release\Microsoft.Scripting.xml'
      if IronRuby.is_merlin?
        contents.change_configuration! 'FxCop|AnyCPU', 'TRACE'
        contents.change_configuration! 'SpecSharp|AnyCPU', 'TRACE;DEBUG'
        contents.change_configuration! 'Debug|AnyCPU', 'TRACE;DEBUG'
        contents.change_configuration! 'Release|AnyCPU', 'TRACE'
        contents.change_configuration! 'Silverlight Debug|AnyCPU', 'TRACE;DEBUG;SILVERLIGHT'
        contents.change_configuration! 'Silverlight Release|AnyCPU', 'TRACE;SILVERLIGHT'
      else
        contents.change_configuration! 'FxCop|AnyCPU', 'TRACE;SIGNED'
        contents.change_configuration! 'SpecSharp|AnyCPU', 'TRACE;DEBUG;SIGNED'
        contents.change_configuration! 'Debug|AnyCPU', 'TRACE;DEBUG;SIGNED'
        contents.change_configuration! 'Release|AnyCPU', 'TRACE;SIGNED'
        contents.change_configuration! 'Silverlight Debug|AnyCPU', 'TRACE;DEBUG;SILVERLIGHT'
        contents.change_configuration! 'Silverlight Release|AnyCPU', 'TRACE;SILVERLIGHT'
      end
    end
    transform_project(:dlr_com, 'system.dynamic.cominterop.csproj') do |contents|
      replace_output_path contents, '..\..\..\..\Merlin\Main\Bin\Debug\\', '..\..\build\debug\\'
      replace_output_path contents, '..\..\..\..\Merlin\Main\Bin\Release\\', '..\..\build\release\\'
      replace_output_path contents, '..\..\..\..\Merlin\Main\Bin\Silverlight Debug\\', '..\..\build\silverlight debug\\'
      replace_output_path contents, '..\..\..\..\Merlin\Main\Bin\Silverlight Release\\', '..\..\build\silverlight release\\'
      replace_import_project contents, '..\..\..\..\Merlin\Main\SpecSharp.targets', '..\..\SpecSharp.targets'
      replace_doc_path contents,    '..\..\..\..\Merlin\Main\Bin\Debug\Microsoft.Scripting.Core.xml', '..\..\build\debug\Microsoft.Scripting.Core.xml'
      replace_doc_path contents,    '..\..\..\..\Merlin\Main\Bin\Release\Microsoft.Scripting.Core.xml', '..\..\build\release\Microsoft.Scripting.Core.xml'
      replace_doc_path contents,    '..\..\..\..\Merlin\Main\Bin\Silverlight Debug\Microsoft.Scripting.Core.xml', '..\..\build\silverlight debug\Microsoft.Scripting.Core.xml'
      replace_doc_path contents,    '..\..\..\..\Merlin\Main\Bin\Silverlight Release\Microsoft.Scripting.Core.xml', '..\..\build\silverlight release\Microsoft.Scripting.Core.xml'
      if IronRuby.is_merlin?
        contents.change_configuration! 'FxCop|AnyCPU', 'TRACE;MICROSOFT_SCRIPTING_CORE'
        contents.change_configuration! 'Debug|AnyCPU', 'TRACE;DEBUG;MICROSOFT_SCRIPTING_CORE'
        contents.change_configuration! 'Release|AnyCPU', 'TRACE;MICROSOFT_SCRIPTING_CORE'
      else
        contents.change_configuration! 'FxCop|AnyCPU', 'TRACE;MICROSOFT_SCRIPTING_CORE;SIGNED'
        contents.change_configuration! 'Debug|AnyCPU', 'TRACE;DEBUG;MICROSOFT_SCRIPTING_CORE;SIGNED'
        contents.change_configuration! 'Release|AnyCPU', 'TRACE;MICROSOFT_SCRIPTING_CORE;SIGNED'
      end
    end
    # TODO: add signing to this project
    transform_project(:yaml, 'IronRuby.Libraries.Yaml.csproj') do |contents|
      replace_output_path contents, '..\..\..\..\..\Main\Bin\Debug\\', '..\..\build\debug\\'
      replace_output_path contents, '..\..\..\..\..\Main\Bin\Release\\', '..\..\build\release\\'
      replace_output_path contents, '..\..\..\..\..\Main\Bin\Silverlight Debug\\', '..\..\build\silverlight debug\\'
      replace_output_path contents, '..\..\..\..\..\Main\Bin\Silverlight Release\\', '..\..\build\silverlight release\\'
      replace_import_project contents, '..\..\..\..\Main\SpecSharp.targets', '..\..\SpecSharp.targets'
    end
    transform_project :generator, 'classinitgenerator.csproj'
    transform_project(:console, 'ruby.console.csproj') do |contents|
      replace_output_path contents, '..\..\..\Bin\Debug\\', '..\..\build\debug\\'
      replace_output_path contents, '..\..\..\Bin\Release\\', '..\..\build\release\\'
      replace_output_path contents, '..\..\..\Bin\Silverlight Debug\\', '..\..\build\silverlight debug\\'
      replace_output_path contents, '..\..\..\Bin\Silverlight Release\\', '..\..\build\silverlight release\\'
      replace_post_build_event contents, 'copy $(ProjectDir)..\merlin_ir.exe.config $(TargetDir)ir.exe.config',
                                         'copy $(ProjectDir)..\..\external_ir.exe.config $(TargetDir)ir.exe.config'

      replace_app_config_path contents, '..\..\..\App.config', '..\..\App.config'
    end
    transform_project :test_runner, 'ironruby.tests.csproj' do |contents|
      replace_output_path contents, '..\..\..\Bin\Debug\\', '..\..\build\debug\\'
      replace_output_path contents, '..\..\..\Bin\Release\\', '..\..\build\release\\'
      replace_output_path contents, '..\..\..\Bin\Silverlight Debug\\', '..\..\build\silverlight debug\\'
      replace_output_path contents, '..\..\..\Bin\Silverlight Release\\', '..\..\build\silverlight release\\'        
      
      replace_app_config_path contents, '..\..\..\App.config', '..\..\App.config'
      unless IronRuby.is_merlin?
        replace_key_path    contents, '..\..\RubyTestKey.snk', '..\..\..\MSSharedLibKey.snk'
      end      
    end

    transform_project :scanner, 'ironruby.libraries.scanner.csproj'
  end
end

desc "push TFS source tree to Subversion"
task :to_svn => [:happy, :clean_tests] do
  raise "must be in MERLIN enlistment to run to_svn" unless IronRuby.is_merlin?
  push
end

desc "push Subversion source tree to TFS"
task :to_merlin => [:happy, :clean_tests] do
  raise "must be in SVN enlistment to run to_merlin" if IronRuby.is_merlin?
  push
end

desc "Generate an IronRuby binary redist package from the layout"
task :package do
  IronRuby.source_context do 
    # Directory layouts
    system %Q{rmdir /S /Q #{PACKAGE_DIR}}
    mkdir_p "#{PACKAGE_DIR}\\bin"
    
    # Copy Licenses
    system %Q{copy "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\Licenses\\*" #{PACKAGE_DIR}}

    # Copy binaries
    system %Q{copy "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\IronRuby.BinaryLayout.config" "#{PACKAGE_DIR}\\bin\\ir.exe.config"}
    system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\ir.exe" #{PACKAGE_DIR}\\bin\\}
    system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\IronRuby*.dll" #{PACKAGE_DIR}\\bin\\}
    system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\Microsoft.Scripting.Core.dll" #{PACKAGE_DIR}\\bin\\}
    system %Q{copy "#{ENV['MERLIN_ROOT']}\\bin\\release\\Microsoft.Scripting.dll" #{PACKAGE_DIR}\\bin\\}
    system %Q{copy "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\Scripts\\bin\\*" #{PACKAGE_DIR}\\bin\\}

    # Generate ir.exe.config
    transform_config_file 'Binary', get_source_dir(:lang_root) + 'app.config', "#{PACKAGE_DIR}\\bin\\ir.exe.config"

    # Copy standard library
    system %Q{xcopy /E /I "#{ENV['MERLIN_ROOT']}\\..\\External\\Languages\\Ruby\\redist-libs\\ruby" #{PACKAGE_DIR}\\lib\\ruby}
    system %Q{xcopy /E /I "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\Libs" #{PACKAGE_DIR}\\lib\\IronRuby}

    # Generate compressed package
    if ENV['ZIP']
      system %Q{del "#{ENV['TEMP']}\\ironruby.7z"}
      system %Q{"#{ENV['PROGRAM_FILES_32']}/7-Zip/7z.exe" a -bd -t7z -mx9 "#{ENV['TEMP']}\\ironruby.7z" "#{PACKAGE_DIR}\\"}
      system %Q{"#{ENV['PROGRAM_FILES_32']}/7-Zip/7z.exe" a -bd -tzip -mx9 "c:\\ironruby.zip" "#{PACKAGE_DIR}\\"}
      system %Q{copy /b /Y "#{ENV['PROGRAM_FILES_32']}\\7-Zip\\7zSD.sfx" + "#{ENV['MERLIN_ROOT']}\\Languages\\Ruby\\sfx_config.txt" + "#{ENV['TEMP']}\\ironruby.7z" "c:\\ironruby.exe"}
    end
  end
end
