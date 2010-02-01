require File.dirname(__FILE__) + '/../spec_helper'

describe "The -r command line option" do
  it "requires the specified file" do
    ["-r fixtures/test_file",
     "-rfixtures/test_file"].each do |o|
      ruby_exe("fixtures/require.rb", :options => o, :dir => File.dirname(__FILE__)).should include("fixtures/test_file.rb")
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
end
