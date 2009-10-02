require File.dirname(__FILE__) + '/../fixtures/classes'

describe :io_tty, :shared => true do
  with_tty do
    platform_is_not :windows do
      it "returns true if this stream is a terminal device (TTY)" do
        File.open('/dev/tty') {|f| f.send @method }.should == true
      end
    end
    
    platform_is :windows do
      it "returns true if this stream is a terminal device (TTY)" do
        File.open('NUL') {|f| f.send @method }.should == true
      end
    end
    
    it "return false when called on a standard stream redirected to a file or a pipe" do
      sin, sout, serr = 1, 2, 4
    
      # standard output is redirected by %x{}
      %x{"#{RUBY_EXE}" "#{File.dirname(__FILE__)}/tty_probe.rb"}
      $?.exitstatus.should == sin | serr
    
      # redirect error output to std output:
      %x{"#{RUBY_EXE}" "#{File.dirname(__FILE__)}/tty_probe.rb" 2>&1}
      $?.exitstatus.should == sin
    
      # redirect input:
      %x{"#{RUBY_EXE}" "#{File.dirname(__FILE__)}/tty_probe.rb" < "#{File.dirname(__FILE__)}/tty_probe.rb"}
      $?.exitstatus.should == serr
    
      # redirect both input and error output:
      %x{"#{RUBY_EXE}" "#{File.dirname(__FILE__)}/tty_probe.rb" 2>1 < "#{File.dirname(__FILE__)}/tty_probe.rb"}
      $?.exitstatus.should == 0    
    end
  end

  it "returns false if this stream is not a terminal device (TTY)" do
    File.open(__FILE__) {|f| f.send @method }.should == false
  end

  it "raises IOError on closed stream" do
    lambda { IOSpecs.closed_file.send @method }.should raise_error(IOError)
  end
end
