require File.dirname(__FILE__) + '/../spec_helper'
require File.dirname(__FILE__) + '/matchers'
require File.dirname(__FILE__) + '/fixtures.generated'

if ENV["ROWAN_BIN"]
  $: << ENV["ROWAN_BIN"]
else
  $: << (ENV["MERLIN_ROOT"] + "\\Bin\\Debug")
end
