require 'debugger'

module ArbitraryModuleSoConsoleDoesNotConflict
  include System
  include System::IO
  include System::Windows
  include System::Windows::Resources

  shared 'BrowserVirtualFilesystem' do
    def get_stream_contents(stream)
      sr = StreamReader.new(stream)
      contents = sr.read_to_end
      sr.close
      contents
    end

    it 'should get file contents from a string' do
      @pkg.get_file_contents(@path).should.match @contents
    end

    it 'should get file contents from a Uri' do
      @pkg.get_file_contents(@uri).should.match @contents
    end
 
    it 'should not get a file contents' do
      @pkg.get_file_contents(@doesnotexist).should.be.nil
    end
  
    it 'should get file stream from string' do
      get_stream_contents(@pkg.get_file(@path)).should.match @contents
    end
  
    it 'should get file stream from Uri' do
      get_stream_contents(@pkg.get_file(@uri)).should.match @contents
    end
  
    it 'should not get a file' do
      @pkg.get_file(@doesnotexist).should.be.nil
    end 
    
    it 'should normalize a path' do
      @pkg.normalize_path('this\is\a\path\to\foo.txt').should.equal 'this/is/a/path/to/foo.txt'
    end

    it 'should get file contents in another package' do
      @pkg.get_file_contents(@other_pkg, @other_pkg_file).should.match @other_pkg_contents
    end

    it 'should get file stream from another package' do
      get_stream_contents(@pkg.get_file(@other_pkg, @other_pkg_file)).should.match @other_pkg_contents
    end
  end

  describe "HttpVirtualFilesystem" do
    before do
      @path ||= 'rblib/bacon.rb'
      @uri ||= Uri.new(@path, UriKind.relative)
      @contents ||= /^# Bacon -- small RSpec clone\./
      @doesnotexist ||= "pylib/doesnotexist.txt"
      @other_pkg_file ||= "bacon.rb"
      @other_pkg_contents ||= @contents
      @other_pkg_uri ||= DynamicApplication.make_uri(@path)
      @other_pkg ||= @other_pkg_uri
      
      # Don't just do HttpVirtualFilesystem.new here, as there is no way to
      # copy the download cache from the app's cache.
      @pkg ||= DynamicApplication.current.engine.runtime.host.platform_adaptation_layer.virtual_filesystem
    end

    behaves_like 'BrowserVirtualFilesystem'
  end

  describe 'XapVirtualFilesystem' do
    before do
      @path ||= "unit/assets/tmp.txt"
      @uri ||= Uri.new(@path, UriKind.relative)
      @contents ||= /Hello!/
      @doesnotexist ||= "unit/assets/doesnotexist.txt"
      @other_pkg_file ||= "pkg/foo.txt"
      @other_pkg_contents ||= /hello world/
      @other_pkg_uri ||= Uri.new("unit/assets/pkg.xap", UriKind.relative)
      @other_pkg ||= Application.get_resource_stream(@other_pkg_uri)
      @pkg ||= XapVirtualFilesystem.new
    end

    behaves_like 'BrowserVirtualFilesystem'
  end
end
