require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel#`" do
  it "is a private method" do
    Kernel.should have_private_instance_method(:`)
  end
  
  it "returns the standard output of the executed sub-process" do
    ip = 'world'
    `echo disc #{ip}`.should == "disc world\n"
  end
  
  it "tries to convert the given argument to String using #to_str" do
    (obj = mock('echo test')).should_receive(:to_str).and_return("echo test")
    Kernel.`(obj).should == "test\n"
  end

  platform_is_not :windows do
    it "sets $? to the exit status of the executed sub-process" do
      ip = 'world'
      `echo disc #{ip}`
      $?.class.should == Process::Status
      $?.stopped?.should == false
      $?.exited?.should == true
      $?.exitstatus.should == 0
      $?.success?.should == true
      `echo disc #{ip}; exit 99`
      $?.class.should == Process::Status
      $?.stopped?.should == false
      $?.exited?.should == true
      $?.exitstatus.should == 99
      $?.success?.should == false
    end
  end

  platform_is :windows do
    it "sets $? to the exit status of the executed sub-process" do
      ip = 'world'
      `echo disc #{ip}`
      $?.class.should == Process::Status
      $?.stopped?.should == false
      $?.exited?.should == true
      $?.exitstatus.should == 0
      $?.success?.should == true
      `echo disc #{ip}& exit 99`
      $?.class.should == Process::Status
      $?.stopped?.should == false
      $?.exited?.should == true
      $?.exitstatus.should == 99
      $?.success?.should == false
    end
  
    def test_comspec(commands)
      comspec_mock = File.dirname(__FILE__) + '/fixtures/comspec.cmd'
      
      query = "ENV['COMSPEC'] = '#{comspec_mock}'; puts "
      query += commands.collect { |cmd| "`#{cmd}`" }.join(', ')
      
      result = ruby_exe(query)
      
      # The comspec mock prints all its command arguments and we check if it gets exactly two: /c "COMMAND".
      i = 0
      result.each_line do |line|
        line.strip.should == %Q{/c "#{commands[i]}"}
        i += 1
      end
    end
  
    it "special cases Windows shell commands" do    
      test_comspec [
        "ASSOC",
        "BREAK",
        "CALL",
        "CD",
        "CHDIR",
        "CLS",
        "COLOR",
        "COPY", 
        "DATE",
        "DEL",
        "DIR",
        "ECHO",
        "ENDLOCAL",
        "ERASE",
        "EXIT",
        "FOR",
        "FTYPE",
        "GOTO",
        "IF",
        "MD",
        "MKDIR",
        "MOVE",
        "PATH",
        "PAUSE",
        "POPD",
        "PROMPT",
        "PUSHD",
        "RD",
        "REM",
        "REN",
        "RENAME",
        "RMDIR",
        "SET",
        "SETLOCAL",
        "SHIFT",
        "START",
        "TIME",
        "TITLE",
        "TYPE",
        "VER",
        "VERIFY",
        "VOL",
      ]
    end
    
    ruby_bug "", "1.9" do
    ruby_bug "", "1.8" do
      it "special cases Windows Vista+ shell commands" do
        test_comspec [
          "MKLINK",
        ]
      end
    end
    end
  end 
  
end

describe "Kernel.`" do
  it "needs to be reviewed for spec completeness"
end
