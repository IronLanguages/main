require 'autotest'

$VERBOSE = false

class RspecCommandError < StandardError; end

# Autotest has no full-blown snake_case to CamelCase
class Autotest::DmserializerRspec < Autotest
  WHOLE_SUITE_REGEXP = %r{^spec/(unit|integration)/.*_spec\.rb}

  Autotest.add_hook :initialize do |at|
    at.clear_mappings
    at.add_exception(/\.git|TAGS/)

    # Updating a spec runs that spec.
    at.add_mapping(%r{^spec/.*_spec\.rb$}) do |filename, _|
      filename
    end

    # Updating spec helper runs the whole suite.
    at.add_mapping(%r{^spec/spec_helper\.rb}) do |_, m|
      at.files_matching WHOLE_SUITE_REGEXP
    end

    # Updating of library file runs the whole suite.
    at.add_mapping(%r{^lib/.*\.rb}) do |_, m|
      at.files_matching WHOLE_SUITE_REGEXP
    end

    # Updating of fixture resources runs the whole suite.
    at.add_mapping(%r{^spec/fixtures/.*\.rb}) do |_, m|
      at.files_matching WHOLE_SUITE_REGEXP
    end
  end


  def initialize(kernel = Kernel, separator = File::SEPARATOR, alt_separator = File::ALT_SEPARATOR) # :nodoc:
    super() # need parens so that Ruby doesn't pass our args
    # to the superclass version which takes none..

    @kernel, @separator, @alt_separator = kernel, separator, alt_separator
    @spec_command = spec_command
  end

  attr_accessor :failures

  def failed_results(results)
    results.scan(/^\d+\)\n(?:\e\[\d*m)?(?:.*?Error in )?'([^\n]*)'(?: FAILED)?(?:\e\[\d*m)?\n(.*?)\n\n/m)
  end

  def handle_results(results)
    @failures = failed_results(results)
    @files_to_test = consolidate_failures @failures
    unless $TESTING
      if @files_to_test.empty?
        hook :green
      else
        hook :red
      end
    end
    @tainted = true unless @files_to_test.empty?
  end

  def consolidate_failures(failed)
    filters = Hash.new { |h,k| h[k] = [] }
    failed.each do |spec, failed_trace|
      find_files.keys.select { |f| f =~ /spec\// }.each do |f|
        if failed_trace =~ Regexp.new(f)
          filters[f] << spec
          break
        end
      end
    end
    filters
  end

  def make_test_cmd(files_to_test)
    "#{ruby} -S #{@spec_command} #{test_cmd_options} #{files_to_test.keys.flatten.join(' ')}"
  end

  def test_cmd_options
    # '-O specs/spec.opts' if File.exist?('specs/spec.opts')
  end

  # Finds the proper spec command to use. Precendence is set in the
  # lazily-evaluated method spec_commands.  Alias + Override that in
  # ~/.autotest to provide a different spec command then the default
  # paths provided.
  def spec_command(separator=File::ALT_SEPARATOR)
    unless defined?(@spec_command)
      @spec_command = spec_commands.find { |cmd| File.exists?(cmd) }

      raise RspecCommandError, "No spec command could be found" unless @spec_command

      @spec_command.gsub!(File::SEPARATOR, separator) if separator
    end
    @spec_command
  end

  # Autotest will look for spec commands in the following
  # locations, in this order:
  #
  #   * default spec bin/loader installed in Rubygems
  #   * any spec command found in PATH
  def spec_commands
    [File.join(Config::CONFIG['bindir'], 'spec'), 'spec']
  end
end
