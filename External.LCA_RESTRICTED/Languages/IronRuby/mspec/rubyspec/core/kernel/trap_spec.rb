require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'

describe "Kernel#trap" do
  it "is a private method" do
    Kernel.should have_private_instance_method(:trap)
  end
end

describe "Kernel.trap" do
  it "needs to be reviewed for spec completeness"
end

describe "Kernel#trap('INT')" do
  # it "raise Interrupt on the main thread"
  # it "does ??? if a second SIGINT is received while the previous SIGINT handler is still running"
  # it "propagates uncaught exception from the handler over to the main thread"
  # it "returns ???"
end