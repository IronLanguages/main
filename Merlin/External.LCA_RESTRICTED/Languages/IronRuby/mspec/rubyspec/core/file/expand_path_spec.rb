require File.dirname(__FILE__) + '/../../spec_helper'
=begin
describe "File.expand_path" do
  before :each do
    @base = Dir.pwd

    platform_is(:windows) { @rootdir = "C:/" }
    platform_is_not(:windows) { @rootdir = "/" }
    @tmpdir = @rootdir + "tmp"
  end

  it "converts a pathname to an absolute pathname" do
    File.expand_path('').should == @base
    File.expand_path('a').should == File.join(@base, 'a')
    File.expand_path('a', nil).should == File.join(@base, 'a')
  end

  it "converts a pathname to an absolute pathname, Ruby-Talk:18512 " do
    # Because of Ruby-Talk:18512
    File.expand_path('.a').should == File.join(@base, '.a')
    File.expand_path('..a').should == File.join(@base, '..a')
    File.expand_path('a../b').should == File.join(@base, 'a../b')
  end
  
  it "converts a pathname with . in filename to an absolute pathname, Ruby-Talk:18512 " do
    File.expand_path('.a').should == File.join(@base, '.a')
    File.expand_path('..a').should == File.join(@base, '..a')
    File.expand_path('a../b').should == File.join(@base, 'a../b')
  end

  platform_is_not :windows do
    it "converts a pathname with trailing . to an absolute pathname, Ruby-Talk:18512 " do
      File.expand_path('a.').should == File.join(@base, 'a.')
      File.expand_path('a..').should == File.join(@base, 'a..')
    end
  end
  platform_is :windows do
    it "converts a pathname with trailing . to an absolute pathname, Ruby-Talk:18512 " do
      File.expand_path('a.').should == File.join(@base, 'a')
      File.expand_path('a..').should == File.join(@base, 'a')
    end
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
      File.expand_path("/tmp/x/../../bin").should == @rootdir + "bin"
      File.expand_path("/tmp/../../bin").should == @rootdir + "bin"
      File.expand_path("/../../bin").should == @rootdir + "bin"
      File.expand_path("tmp/x/../../bin").should == File.join(@base, 'bin')
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
    it "returns file system case only for the last component" do
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
    
    it "returns file system case only for the last component of both arguments" do
      File.expand_path("wInDoWs", "/").should == "c:/Windows"

      File.expand_path("nOtEpAd.exe", "/wInDoWs").should == "c:/Windows/notepad.exe"
      File.expand_path("sYsTeM32", "/wInDoWs").should == "c:/Windows/System32"
      File.expand_path("nOn-ExIsTeNt", "/wInDoWs").should == "c:/Windows/nOn-ExIsTeNt"

      File.expand_path("wInDoWs/nOtEpAd.exe", "/").should == "c:/wInDoWs/notepad.exe"
      File.expand_path("wInDoWs/sYsTeM32", "/").should == "c:/wInDoWs/System32"
      File.expand_path("wInDoWs/nOn-ExIsTeNt", "/").should == "c:/wInDoWs/nOn-ExIsTeNt"

      File.expand_path("foo", "/NoN-eXiStEnT").should == "c:/NoN-eXiStEnT/foo"
    end
    
    it "allows back slashes" do
      File.expand_path('\foo\bar').should == @rootdir + "foo/bar"
    end
    
    it "supports drive letter for relative path" do
      File.expand_path("c:foo").should == File.expand_path("foo")
      File.expand_path("x:foo").should == "x:/foo"
    end

    it "supports different drive letters" do
      File.expand_path("x:/foo").should == "x:/foo"
    end
  end
  
  it "leaves alone characters like : (line number separator in backtraces) which are invalid on some platforms" do
    File.expand_path("foo.ext:123").should == @base + "/foo.ext:123"
    File.expand_path("foo:xxx").should == @base + "/foo:xxx"
    File.expand_path("/dir1:xxx/dir2:xxx/../foo:xxx").should == @rootdir + "dir1:xxx/foo:xxx"
    File.expand_path("/foo/...").should == @rootdir + "foo/..."
  end
  
  it "expands /./dir to /dir" do
    File.expand_path("/./dir").should == @rootdir + "dir"
  end
  
  it "allows extra .." do
    File.expand_path(@rootdir + "/../../foo").should == @rootdir + "foo"
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
end

describe "File.expand_path(file_name, dir_string)" do
  before :each do
    @base = Dir.pwd
    platform_is(:windows) { @rootdir = "C:/" }
    platform_is_not(:windows) { @rootdir = "/" }
    @tmpdir = @rootdir + "tmp"
  end

  it "converts a pathname to an absolute pathname with dir_string" do
    File.expand_path('', '').should == @base
    File.expand_path('', 'a').should == @base + '/a'
    File.expand_path('b', 'a').should == @base + '/a/b'
    File.expand_path('b', '/a').should == @rootdir + 'a/b'
  end

  it "converts a pathname to an absolute pathname, using a complete path" do
    File.expand_path("", "#{@tmpdir}").should == "#{@tmpdir}"
    File.expand_path("a", "#{@tmpdir}").should =="#{@tmpdir}/a"
    File.expand_path("../a", "#{@tmpdir}/xxx").should == "#{@tmpdir}/a"
    File.expand_path(".", "#{@rootdir}").should == "#{@rootdir}"
  end

  it "ignores dir_string if file_name is an absolute path" do
    File.expand_path('/foo', '/bar').should == @rootdir + "foo"
  end
  
end
=end