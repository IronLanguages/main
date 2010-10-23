require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'
require File.dirname(__FILE__) + '/shared/send'

describe "Kernel#__send__" do
  it_behaves_like(:kernel_send, :__send__)
  it "needs to be reviewed for spec completeness"
end
