require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "File.expand_path" do
  before :each do
    @base = Dir.pwd
    platform_is(:windows) { @rootdir = @base[0..2] }
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
      File.expand_path("~\\testdir").should == "#{home}/testdir"
      
      
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
    # TODO: doesn't work in D:\, D:\x
    #File.expand_path('..').should == Dir.pwd.split('/')[0...-1].join("/")
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
    it "returns file system case only for the last component of input(s)" do
      FileSpecs.with_upper_case_folders do |t|
        File.expand_path("#{t}/aaa/bbb").should == "#{t}/aaa/BBB"      
        File.expand_path("#{t}/./aaa/../AaA/BBB").should == "#{t}/AaA/BBB"
        File.expand_path("ccc/ddd", "#{t}/aaa/bbb").should == "#{t}/aaa/BBB/ccc/DDD"
      end
    end
    
    it "allows back slashes" do
      File.expand_path('\foo\bar').should == @rootdir + "foo/bar"
    end
    
    it "supports drive letter for relative path" do
      File.expand_path("#{Dir.pwd[0..1]}foo").should == File.expand_path("foo")
      File.expand_path(FileSpecs.non_existent_drive + "foo").should == FileSpecs.non_existent_drive + "/foo"
    end

    it "supports non-existent drive letters" do
      File.expand_path(FileSpecs.non_existent_drive + "/foo").should == FileSpecs.non_existent_drive + "/foo"
    end
  end
  
  it "leaves alone characters like : (line number separator in backtraces) which are invalid on some platforms" do
    File.expand_path("foo.ext:123").should == File.join(@base, "foo.ext:123")
    File.expand_path("foo:xxx").should == File.join(@base, "foo:xxx")
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
    platform_is(:windows) { @rootdir = @base[0..2] }
    platform_is_not(:windows) { @rootdir = "/" }
    @tmpdir = @rootdir + "tmp"
  end

  it "converts a pathname to an absolute pathname with dir_string" do
    File.expand_path('', '').should == @base
    File.expand_path('', 'a').should == File.join(@base, 'a')
    File.expand_path('b', 'a').should == File.join(@base, 'a/b')
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

  ruby_version_is "1.9" do
    it "accepts objects that have a #to_path method" do
      File.expand_path(mock_to_path("a"), mock_to_path("#{@tmpdir}"))
    end
  end


  ruby_version_is "1.9" do
    it "produces a String in the default external encoding" do
      old_external = Encoding.default_external
      Encoding.default_external = Encoding::SHIFT_JIS
      File.expand_path("./a").encoding.should == Encoding::SHIFT_JIS
      File.expand_path("./\u{9876}").encoding.should == Encoding::SHIFT_JIS
      Encoding.default_external = old_external
    end
  end
end
