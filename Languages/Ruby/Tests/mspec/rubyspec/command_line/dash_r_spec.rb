require File.expand_path('../../spec_helper', __FILE__)

describe "The -r command line option" do
  before :each do
    @script = fixture __FILE__, "require.rb"
    @test_file = fixture __FILE__, "test_file"
  end

  it "requires the specified file" do
    ["-r #{@test_file}", "-r#{@test_file}"].each do |o|
      result = ruby_exe(@script, :options => o)
      result.should include(@test_file + ".rb")
    end
  end

  it "can be specified multiple times" do
    ruby_exe("fixtures/test_file.rb", :options => "-r fixtures/file -r fixtures/hello", :dir => File.dirname(__FILE__)).should include("file.rb\nHello world")
  end

  it "can be specified multiple times, but will require the same file just once" do
    (ruby_exe("fixtures/test_file.rb", :options => "-r fixtures/hello -r fixtures/hello", :dir => File.dirname(__FILE__)) =~ /Hello.*Hello/m).should be_nil
  end

  it "stops processing remaining files if an exception is thrown" do
    ruby_exe("fixtures/hello", :options => "-r fixtures/raise", :dir => File.dirname(__FILE__)).should_not include("Hello")
  end

  it "can be set from RUBYOPT environment variable" do
    ["-r #{@test_file}", "-r#{@test_file}"].each do |o|
      begin
        ENV['RUBYOPT'] = o
        result = ruby_exe(@script, :options => "")
        result.should include(@test_file + ".rb")
      ensure
        ENV['RUBYOPT'] = nil
      end
    end
  end

end
