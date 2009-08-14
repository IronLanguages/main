require File.dirname(__FILE__) + '/../../../spec_helper'
require File.dirname(__FILE__) + '/../shared/enum_for'

describe "Kernel#to_enum" do
  it_behaves_like :enum_for, :to_enum
end
