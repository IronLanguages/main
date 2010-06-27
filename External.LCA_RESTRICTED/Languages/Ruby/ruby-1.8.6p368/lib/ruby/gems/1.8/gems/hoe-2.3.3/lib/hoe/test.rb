##
# Test plugin for hoe.
#
# === Tasks Provided:
#
# audit::              Run ZenTest against the package.
# default::            Run the default task(s).
# multi::              Run the test suite using multiruby.
# test::               Run the test suite.
# test_deps::          Show which test files fail when run alone.

module Hoe::Test
  ##
  # Configuration for the supported test frameworks for test task.

  SUPPORTED_TEST_FRAMEWORKS = {
    :testunit => "test/unit",
    :minitest => "minitest/autorun",
  }

  ##
  # Used to add flags to test_unit (e.g., -n test_borked).
  #
  # eg FILTER="-n test_blah"

  FILTER = ENV['FILTER'] || ENV['TESTOPTS']

  ##
  # Optional: Array of incompatible versions for multiruby filtering.
  # Used as a regex.

  attr_accessor :multiruby_skip

  ##
  # Optional: What test library to require [default: :testunit]

  attr_accessor :testlib

  ##
  # Optional: RSpec dirs. [default: %w(spec lib)]

  attr_accessor :rspec_dirs

  ##
  # Optional: RSpec options. [default: []]

  attr_accessor :rspec_options

  ##
  # Initialize variables for plugin.

  def initialize_test
    self.multiruby_skip ||= []
    self.testlib        ||= :testunit
    self.rspec_dirs     ||= %w(spec lib)
    self.rspec_options  ||= []
  end

  ##
  # Define tasks for plugin.

  def define_test_tasks
    default_tasks = []

    if File.directory? "test" then
      desc 'Run the test suite. Use FILTER or TESTOPTS to add flags/args.'
      task :test do
        ruby make_test_cmd
      end

      desc 'Run the test suite using multiruby.'
      task :multi do
        ruby make_test_cmd(:multi)
      end

      desc 'Show which test files fail when run alone.'
      task :test_deps do
        tests = Dir["test/**/test_*.rb"]  +  Dir["test/**/*_test.rb"]

        paths = ['bin', 'lib', 'test'].join(File::PATH_SEPARATOR)
        null_dev = Hoe::WINDOZE ? '> NUL 2>&1' : '&> /dev/null'

        tests.each do |test|
          if not system "ruby -I#{paths} #{test} #{null_dev}" then
            puts "Dependency Issues: #{test}"
          end
        end
      end

      default_tasks << :test
    end

    if File.directory? "spec" then
      begin
        require 'spec/rake/spectask'

        desc "Run all specifications"
        Spec::Rake::SpecTask.new(:spec) do |t|
          t.libs = self.rspec_dirs
          t.spec_opts = self.rspec_options
        end
      rescue LoadError
        # do nothing
      end
      default_tasks << :spec
    end

    desc 'Run the default task(s).'
    task :default => default_tasks

    desc 'Run ZenTest against the package.'
    task :audit do
      libs = %w(lib test ext).join(File::PATH_SEPARATOR)
      sh "zentest -I=#{libs} #{spec.files.grep(/^(lib|test)/).join(' ')}"
    end
  end

  ##
  # Generate the test command-line.

  def make_test_cmd multi = false # :nodoc:
    framework = SUPPORTED_TEST_FRAMEWORKS[testlib]
    raise "unsupported test framework #{testlib}" unless framework

    tests = ["rubygems", framework] +
      test_globs.map { |g| Dir.glob(g) }.flatten
    tests.map! {|f| %(require "#{f}")}

    cmd = "#{Hoe::RUBY_FLAGS} -e '#{tests.join("; ")}' #{FILTER}"

    if multi then
      ENV['EXCLUDED_VERSIONS'] = multiruby_skip.join ":"
      cmd = "-S multiruby #{cmd}"
    end

    cmd
  end
end
