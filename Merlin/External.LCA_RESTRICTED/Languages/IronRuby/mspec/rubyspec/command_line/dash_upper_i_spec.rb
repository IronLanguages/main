require File.dirname(__FILE__) + '/../spec_helper'

describe "The -I command line option" do
  it "adds the path to the load path ($:)" do
    ruby_exe("fixtures/loadpath.rb", :options => '-I fixtures', :dir => File.dirname(__FILE__)).should include("fixtures")
  end
  
  it "allows different formats" do
    ['-I fixtures', '-Ifixtures', '-I"fixtures"', '-I./fixtures', '-I.\fixtures'].each do |format|
      ruby_exe("fixtures/loadpath.rb", :options => format, :dir => File.dirname(__FILE__)).should include("fixtures")
    end
  end
end
