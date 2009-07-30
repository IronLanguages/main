require File.dirname(__FILE__) + "/../spec_helper"

describe "IronRuby.configuration" do
  it "returns a DlrConfiguration object" do
    require 'microsoft.scripting'
    IronRuby.configuration.should be_kind_of Microsoft::Scripting::Runtime::DlrConfiguration
  end
end

describe "IronRuby.configuration" do
  it "needs to be reviewed for spec completeness"
end
