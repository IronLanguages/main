require File.dirname(__FILE__) + '/../spec_helper'
require 'mspec/helpers/nan'

describe Object, "#nan" do
  it "returns NaN" do
    nan.nan?.should be_true
  end
end
