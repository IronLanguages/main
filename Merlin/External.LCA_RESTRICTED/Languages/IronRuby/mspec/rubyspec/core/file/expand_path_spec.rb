require File.dirname(__FILE__) + '/../../spec_helper'

describe "File.expand_path" do
  before :each do
    platform_is :windows do
      @base = `cd`.chomp.tr '\\', '/'
      @tmpdir = "c:/tmp"
      @rootdir = "c:/"
    end

    platform_is_not :windows do
      @base = Dir.pwd
      @tmpdir = "/tmp"
      @rootdir = "/"
    end
  end

  it "converts a pathname to an absolute pathname" do
    File.expand_path('').should == @base
    File.expand_path('a').should == File.join(@base, 'a')
    File.expand_path('a', nil).should == File.join(@base, 'a')
  end

  not_compliant_on :ironruby do
    it "converts a pathname to an absolute pathname, Ruby-Talk:18512 " do
      # Because of Ruby-Talk:18512
      File.expand_path('a.').should == File.join(@base, 'a.')
      File.expand_path('.a').should == File.join(@base, '.a')
      File.expand_path('a..').should == File.join(@base, 'a..')
      File.expand_path('..a').should == File.join(@base, '..a')
      File.expand_path('a../b').should == File.join(@base, 'a../b')
    end
  end

  it "converts a pathname to an absolute pathname, using a complete path" do
    File.expand_path("", "#{@tmpdir}").should == "#{@tmpdir}"
    File.expand_path("a", "#{@tmpdir}").should =="#{@tmpdir}/a"
    File.expand_path("../a", "#{@tmpdir}/xxx").should == "#{@tmpdir}/a"
    File.expand_path(".", "#{@rootdir}").should == "#{@rootdir}"
  end

  it "converts a pathname to an absolute pathname, using ~ (home) as base" do
    home = ENV['HOME']
    initial_home = home
    begin
      platform_is :windows do
        if not home then
          home = "c:\\Users\\janedoe"
          ENV['HOME'] = home
        end
        home = home.tr '\\', '/'
      end
      File.expand_path('~').should == home
      File.expand_path('~', '/tmp/gumby/ddd').should == home
      File.expand_path('~/a', '/tmp/gumby/ddd').should == File.join(home, 'a')
      File.expand_path('~a').should == '~a'
      File.expand_path('~/').should == home
      File.expand_path('~/..badfilename').should == "#{home}/..badfilename"
      File.expand_path('~/a','~/b').should == "#{home}/a"
      
      
      ENV['HOME'] = nil
      lambda { File.expand_path('~') }.should raise_error(ArgumentError)
    ensure
      ENV['HOME'] = initial_home
    end
  end
 
  # FIXME: these are insane!
  it "expand_path for commoms unix path  give a full path" do
    File.expand_path('/tmp/').should == @rootdir + 'tmp'
    File.expand_path('/tmp/../../../tmp').should == @rootdir + 'tmp'
    File.expand_path('').should == Dir.pwd
    File.expand_path('./////').should == Dir.pwd
    File.expand_path('.').should == Dir.pwd
    File.expand_path(Dir.pwd).should == Dir.pwd
    File.expand_path('..').should == Dir.pwd.split('/')[0...-1].join("/")
    File.expand_path('//').should == '//'
  end

  platform_is_not :windows do
    it "expand path with .." do
      File.expand_path("../../bin", "/tmp/x").should == @rootdir + "bin"
      File.expand_path("../../bin", "/tmp").should == @rootdir + "bin"
      File.expand_path("../../bin", "/").should == @rootdir + "bin"
      File.expand_path("../../bin", "tmp/x").should == File.join(@base, 'bin')
    end

    it "raises an ArgumentError if the path is not valid" do
      lambda { File.expand_path("~a_fake_file") }.should raise_error(ArgumentError)
    end

    it "expands ~ENV['USER'] to the user's home directory" do
      File.expand_path("~#{ENV['USER']}").should == ENV['HOME']
      File.expand_path("~#{ENV['USER']}/a").should == "#{ENV['HOME']}/a"
    end
  end

  platform_is :windows do
    it "sometimes returns file system case with one argument" do
      File.expand_path("/wInDoWs").should == "c:/Windows"
      File.expand_path("/nOn-ExIsTeNt").should == "c:/nOn-ExIsTeNt"
      File.expand_path("/wInDoWs/nOtEpAd.exe").should == "c:/wInDoWs/notepad.exe"
      File.expand_path("/wInDoWs/sYsTeM32").should == "c:/wInDoWs/System32"
      File.expand_path("/wInDoWs/nOn-ExIsTeNt").should == "c:/wInDoWs/nOn-ExIsTeNt"

      File.expand_path("/./wInDoWs").should == "c:/Windows"
      File.expand_path("/./wInDoWs/nOtEpAd.exe").should == "c:/wInDoWs/notepad.exe"
      File.expand_path("/./wInDoWs/sYsTeM32").should == "c:/wInDoWs/System32"
      File.expand_path("/./wInDoWs/nOn-ExIsTeNt").should == "c:/wInDoWs/nOn-ExIsTeNt"

      File.expand_path("/./wInDoWs/../WiNdOwS/nOtEpAd.exe").should == "c:/WiNdOwS/notepad.exe"
    end
    
    it "sometimes returns file system case with two arguments" do
      File.expand_path("wInDoWs", "/").should == "c:/Windows"

      File.expand_path("nOtEpAd.exe", "/wInDoWs").should == "c:/Windows/notepad.exe"
      File.expand_path("sYsTeM32", "/wInDoWs").should == "c:/Windows/System32"
      File.expand_path("nOn-ExIsTeNt", "/wInDoWs").should == "c:/Windows/nOn-ExIsTeNt"

      File.expand_path("wInDoWs/nOtEpAd.exe", "/").should == "c:/wInDoWs/notepad.exe"
      File.expand_path("wInDoWs/sYsTeM32", "/").should == "c:/wInDoWs/System32"
      File.expand_path("wInDoWs/nOn-ExIsTeNt", "/").should == "c:/wInDoWs/nOn-ExIsTeNt"

      File.expand_path("foo", "/NoN-eXiStEnT").should == "c:/NoN-eXiStEnT/foo"
    end
  end
  
  it "raises an ArgumentError is not passed one or two arguments" do
    lambda { File.expand_path }.should raise_error(ArgumentError)
    lambda { File.expand_path '../', 'tmp', 'foo' }.should raise_error(ArgumentError)
  end

  it "raises a TypeError if not passed a String type" do
    lambda { File.expand_path(1)    }.should raise_error(TypeError)
    lambda { File.expand_path(nil)  }.should raise_error(TypeError)
    lambda { File.expand_path(true) }.should raise_error(TypeError)
  end

  it "expands /./dir to /dir" do
    File.expand_path("/./dir").should == "/dir"
  end
end
