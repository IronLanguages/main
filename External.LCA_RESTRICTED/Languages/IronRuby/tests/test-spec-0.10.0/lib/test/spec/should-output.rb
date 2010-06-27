# Code adapted from rs, written by Eero Saynatkari.
require 'fileutils'
require 'tmpdir'

class Test::Spec::Should
  # Captures output from the IO given as
  # the second argument (STDIN by default)
  # and matches it against a String or 
  # Regexp given as the first argument.
  def output(expected, to = STDOUT)
    # Store the old stream
    old_to = to.dup

    # Obtain a filehandle to replace (works with Readline)
    to.reopen File.open(File.join(Dir.tmpdir, "should_output_#{$$}"), "w+")
    
    # Execute
    @object.call

    # Restore
    out = to.dup
    to.reopen old_to

    # Grab the data
    out.rewind
    output = out.read

    # Match up
    case expected
      when Regexp
        output.should.match expected
      else
        output.should.equal expected
    end                               # case expected

  # Clean up
  ensure
    out.close

    # STDIO redirection will break else
    begin
      to.seek 0, IO::SEEK_END
    rescue Errno::ESPIPE
    rescue Errno::EPIPE
    end

    FileUtils.rm_f out.path
  end                                 # output
end                                   # Test::Spec::Should
