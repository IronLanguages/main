require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/shared/values_at'

describe "ENV.indices" do
  it_behaves_like(:env_values_at, :indices)
end
