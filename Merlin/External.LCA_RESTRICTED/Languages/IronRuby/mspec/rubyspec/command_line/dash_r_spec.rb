require File.dirname(__FILE__) + '/../spec_helper'

describe "The -r command line option" do
  it "requires the specified file" do
    ["-r fixtures/test_file",
     "-rfixtures/test_file"].each do |o|
      ruby_exe("fixtures/require.rb", :options => o, :dir => File.dirname(__FILE__)).chomp.should include("fixtures/test_file.rb")
    end
  end
end
