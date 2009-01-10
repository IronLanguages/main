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

def run_specs(method_name)
  ARGV.delete_at 0
  runner = MSpecRunner.new

  IronRuby.source_context do
    iruby = $ruby_imp

    if ARGV.length == 0
      runner.all_core(method_name, iruby)
    elsif ARGV.length == 1
      runner.send(:"#{method_name}", iruby, ARGV.first)
    elsif ARGV.length == 2
      runner.send(:"#{method_name}", iruby, ARGV[0], ARGV[1])
    else
      rake_output_message "usage: rake #{method_name} [class] [method]"
      exit(-1)
    end
  end
  runner
end

def split_args
  klass = ARGV.length == 1 ? '-' : ARGV[1]
  name = ARGV.length <= 2 ? '-' : ARGV[2]
  reporter = ARGV.length == 4 ? ARGV[3] : "summary"
  [klass, name, reporter]
end

def path_to_ir
  (build_path + IRONRUBY_COMPILER).gsub '\\', '/'
end

def extract_reporter(reporter)
   case reporter
   when 'dox'
     '-f specdoc'
   when 'fail'
     '-f fail'
   when 'cov'
     ['-f coverage','run -G critical']
   when 'tag'
     ['-f dotted','tag -G critical']
   when 'run'
     ['-f dotted','run -G critical']
   else
     '-f dotted'
   end
end

def invoke_mspec(path_to_ruby, root_path = "core")
  unless root_path == "language"
    klass, name, reporter = split_args
  else
    name, reporter, _ = split_args
  end
  IronRuby.source_context do
    root = UserEnvironment.rubyspec
    spec_file = name == '-' ? '' : "#{name}_spec.rb"
    spec_dir = klass == '-' ? '' : "#{klass}/"
    spec_suite = spec_dir + spec_file
    run_spec = root + "/1.8/#{root_path}/#{spec_suite}"
    reporter,tag  = extract_reporter(reporter)

    chdir(get_source_dir(:tests) +'util'){
      cmd =  "\"#{UserEnvironment.mri_binary}\" \"#{UserEnvironment.mspec}/bin/mspec\" #{tag || 'ci'} -t #{path_to_ruby} -B \"#{UserEnvironment.config}\" \"#{run_spec}\" #{reporter}"
      exec_net cmd
    }
  end
end

desc "[deprecated] run old school spec tests"
task :test_libs do
  IronRuby.source_context do
    get_source_dir(:tests).filtered_subdirs.each do |dir|
      chdir(dir) {
        dir.glob('test_*.rb').each { |file| exec_net "\"#{build_path + IRONRUBY_COMPILER}\" \"#{file}\"" }
      }
    end
  end
end

desc "run compiler only tests using IronRuby.Tests.exe C# driver"
task :test_compiler do
  IronRuby.source_context do
    exec_net "#{build_path + 'ironruby.tests.exe'}"
    # TODO: make run.rb do the right thing in external svn-only install
    chdir(:tests) { exec "ruby run.rb" }
  end
end

desc "Alias for mspec:core"
task :mspec => "mspec:core"

namespace :mspec do
  desc "Run RubySpec core suite"
  task :core => ["ruby_imp", :testhappy] do
    IronRuby.source_context { invoke_mspec($ruby_imp) }
    exit
  end

  desc "Run core suite with both CRuby and Ironruby"
  task :dual => [:testhappy] do
    IronRuby.source_context do
      rake_output_message "Ruby\n"
      invoke_mspec(UserEnvironment.mri_binary)
      rake_output_message "IronRuby\n"
      invoke_mspec(path_to_ir)
      exit
    end
  end

  desc "Run RubySpec language suite"
  task :lang => ["ruby_imp", :testhappy] do
    IronRuby.source_context { invoke_mspec($ruby_imp, 'language')}
    exit
  end

  desc "Run RubySpec library suite"
  task :lib => ["ruby_imp", :testhappy] do
    IronRuby.source_context { invoke_mspec($ruby_imp, 'library')}
    exit
  end
end

desc "remove output files and generated debugging info from tests directory"
task :clean_tests do
  IronRuby.source_context do
    chdir(:tests) do
      exec "del /s *.log"
      exec "del /s *.pdb"
      exec "del /s *.exe"
      exec "del /s *.dll"
    end
  end
end

# New mspec tasks for developers - these set a new regression baseline, run the
# regression tests, reports why a regression test failed, and test a specific class
# while ignoring exclusions.

desc "yell if the rubyspec test environment is not setup"
task :testhappy do
  spec_env_ready = UserEnvironment.rubyspec? && UserEnvironment.mspec? && UserEnvironment.tags? && UserEnvironment.config?

  if not spec_env_ready
    rake_output_message "\n"
    rake_output_message "***** Missing rubyspec test environment! You must have rubyspec, mspec, ironruby-tags and default.mspec"
    rake_output_message "***** 1. get GIT from http://code.google.com/p/msysgit/"
    rake_output_message "***** 2. run: git clone git://github.com/ironruby/ironruby-tags.git"
    rake_output_message "***** 3. run: git clone git://github.com/ironruby/mspec.git"
    rake_output_message "***** 4. run: git clone git://github.com/ironruby/rubyspec.git"
    rake_output_message "***** 5. run: runfirst.cmd"
    rake_output_message "***** 6. edit: #{ENV['USERPROFILE']}\\.irconfig.rb"
    rake_output_message "***** 7. edit: #{ENV['USERPROFILE']}\\default.mspec"
    exit(-1)
  end
end

desc "generate new baseline against a class"
task :baseline => [:testhappy, :ruby_imp] do
  run_specs(:baseline)
  exit
end

desc "run specs against a class, ignoring all exclusions"
task :test => [:testhappy, :ruby_imp] do
  run_specs :test
  exit
end

desc "show report of why a regression test failed"
task :why_regression => [:testhappy, :ruby_imp] do
  run_specs :why_regression
  exit
end

desc "run regression tests using mspec"
task :regression => [:testhappy, :ruby_imp] do
  run_specs(:regression).report
  exit
end

desc "regenerate critical tags"
task :regen_tags => [:testhappy] do
  IronRuby.source_context { MSpecRunner.new.generate_critical_tags }
end

desc "Set ruby runner to CRuby"
task :ruby do
  begin
    old_verbose,$VERBOSE = $VERBOSE,nil
    $ruby_imp = [UserEnvironment.mri_binary] 
    ARGV = [ARGV[0],*ARGV[2..-1]]
  ensure
    $VERBOSE = old_verbose
  end
end

task :ruby_imp do
  IronRuby.source_context do
    $ruby_imp ||= %Q{#{path_to_ir} -T "-X:Interpret"}
  end
end

desc "Run PEVerify on the generated IL"
task :peverify do
  begin
    old_verbose, $VERBOSE = $VERBOSE, nil
    IronRuby.source_context {$ruby_imp ||= %Q{#{path_to_ir} -T "-X:SaveAssemblies"} }
    ARGV = [ARGV[0], *ARGV[2..-1]]
  ensure
    $VERBOSE = old_verbose
  end
end
