require File.dirname(__FILE__) + "/../../spec_helper"
require "bigdecimal"

describe "Outputing in different cultures" do
  it "works for changing cultures" do
    puts BigDecimal("NaN").to_f
    ja = System::Globalization::CultureInfo.new("ja-JP", false)
    System::Threading::Thread.CurrentThread.CurrentCulture = ja
    lambda {puts BigDecimal("NaN").to_f}.should_not raise_error
  end
end
