require File.dirname(__FILE__) + "/../../spec_helper"

describe "IronPython interop" do
  it "allows evaluation of IDOMPs" do
    load_assembly "IronPython"
    e = IronPython::Hosting::Python.CreateEngine
    lambda { e.execute "str" }.should_not raise_error
  end
end
