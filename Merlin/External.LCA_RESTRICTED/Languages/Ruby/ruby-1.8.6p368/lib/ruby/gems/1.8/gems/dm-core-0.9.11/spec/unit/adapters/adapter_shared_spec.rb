
describe "a DataMapper Adapter", :shared => true do

  it "should initialize the connection uri" do
    new_adapter = @adapter.class.new(:default, Addressable::URI.parse('some://uri/string'))
    new_adapter.instance_variable_get('@uri').to_s.should == Addressable::URI.parse('some://uri/string').to_s
  end

  %w{create read_many read_one update delete create_model_storage alter_model_storage destroy_model_storage create_property_storage alter_property_storage destroy_property_storage} .each do |meth|
    it "should have a #{meth} method" do
      @adapter.should respond_to(meth.intern)
    end
  end

end
