require File.expand_path('../../spec_helper', __FILE__)
require File.expand_path('../shared/verbose', __FILE__)

describe "The -v command line option" do
  it "parses other command line options too" do
    ruby_exe(nil, :args => %Q{-e "puts 'hello'"}, :options => "-v").split[-1].should == "hello"
  end

  it_behaves_like "version option", "-v"
  it_behaves_like :command_line_verbose, "-v"
end
