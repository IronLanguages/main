require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/classes'
require File.dirname(__FILE__) + '/shared/send'

describe "Kernel#send" do
  it_behaves_like(:kernel_send, :send)
end
