require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/values_at'

describe "ENV.values_at" do
  it_behaves_like(:env_values_at, :values_at)
  ruby_version_is "1.9" do
    it "uses the locale encoding" do
      ENV.values_at(ENV.keys.first).first.encoding.should == Encoding.find('locale')
    end
  end
end
