require File.dirname(__FILE__) + '/../spec_helper'
require File.dirname(__FILE__) + '/shared/verbose'
require File.dirname(__FILE__) + '/shared/version'

describe "The -v command line option" do
  it "parses other command line options too" do
    ruby_exe(nil, :args => %Q{-e "puts 'hello'"}, :options => "-v").split[-1].should == "hello"
  end

  it_behaves_like "version option", "-v"
  it_behaves_like "sets $VERBOSE to true", "-v"
end
