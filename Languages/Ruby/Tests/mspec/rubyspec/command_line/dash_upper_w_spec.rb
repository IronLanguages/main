require File.expand_path('../../spec_helper', __FILE__)
require File.expand_path('../shared/verbose', __FILE__)

describe "The -W command line option" do
  before :each do
    @script = fixture __FILE__, "verbose.rb"
  end

  it "with 0 sets $VERBOSE to nil" do
    ruby_exe(@script, :options => "-W0").chomp.should == "nil"
  end

  it "with 1 sets $VERBOSE to false" do
    ruby_exe(@script, :options => "-W1").chomp.should == "false"
  end

  it "can be set from RUBYOPT environment variable" do
    begin
      ENV['RUBYOPT'] = "-W2"
      ruby_exe(@script, :options => "").chomp.should == "true"
    ensure
      ENV['RUBYOPT'] = nil
    end
  end
end

describe "The -W command line option with 2" do
  it_behaves_like :command_line_verbose, "-W2"
end
