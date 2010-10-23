require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/../shared/mangling'

describe "Name mangling on public methods" do
  before(:each) do
    @objs = [PublicNameHolder.new, SubPublicNameHolder.new, Class.new(PublicNameHolder).new, Class.new(SubPublicNameHolder).new]
  end
  it_behaves_like :name_mangling, nil
end
