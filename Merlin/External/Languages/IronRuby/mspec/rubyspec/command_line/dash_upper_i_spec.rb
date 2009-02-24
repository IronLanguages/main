require File.dirname(__FILE__) + '/../spec_helper'

describe "The -I command line option" do
  it "adds the path to the load path ($:)" do
    ruby_exe("fixtures/loadpath.rb", :options => "-I fixtures").chomp.should include("fixtures")
  end
end
