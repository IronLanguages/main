##
# RCov plugin for hoe.
#
# === Tasks Provided:
#
# rcov::               Analyze code coverage with tests

module Hoe::RCov
  ##
  # Define tasks for plugin.

  def define_rcov_tasks
    begin # take a whack at defining rcov tasks
      require 'rcov/rcovtask'

      Rcov::RcovTask.new do |t|
        pattern = ENV['PATTERN'] || test_globs

        t.test_files = FileList[pattern]
        t.verbose = true
        t.rcov_opts << "--no-color"
        t.rcov_opts << "--save coverage.info"
        t.rcov_opts << "-x ^/"
      end

      # this is for my emacs rcov overlay stuff on emacswiki.
      task :rcov_overlay do
        path = ENV["FILE"]
        rcov, eol = Marshal.load(File.read("coverage.info")).last[path], 1
        puts rcov[:lines].zip(rcov[:coverage]).map { |line, coverage|
          bol, eol = eol, eol + line.length
          [bol, eol, "#ffcccc"] unless coverage
        }.compact.inspect
      end
    rescue LoadError
      # skip
      task :clobber_rcov # in case rcov didn't load
    end
  end
end

task :clean => :clobber_rcov
