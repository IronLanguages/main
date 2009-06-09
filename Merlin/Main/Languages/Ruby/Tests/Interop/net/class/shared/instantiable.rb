describe :instantiable_class, :shared => true do 
  it "are able to be instantiated" do
    @method.new.should be_kind_of @method
  end
end
