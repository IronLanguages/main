require File.dirname(__FILE__) + '/../spec_helper'
require File.dirname(__FILE__) + '/matchers'
require File.dirname(__FILE__) + '/fixtures.generated'

if ENV["DLR_BIN"]
  $: << ENV["DLR_BIN"]
else
  $: << (ENV["DLR_ROOT"] + "\\Bin\\Debug")
end
