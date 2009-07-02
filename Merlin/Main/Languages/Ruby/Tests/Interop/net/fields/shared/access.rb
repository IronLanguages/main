describe :accessing_fields, :shared => true do
  it "fields" do
    @klass.send(@result[:field][0]).should equal_clr_string(@result[:field][1])
  end

  it "const fields" do
    ClassWithFields.send(@result[:const_field][0]).should equal_clr_string(@result[:const_field][1])
  end

  it "readonly fields" do
    @klass.send(@result[:readonly][0]).should equal_clr_string(@result[:readonly][1])
  end

  it "static fields" do
    ClassWithFields.send(@result[:static][0]).should equal_clr_string(@result[:static][1])
  end

  it "static readonly fields" do
    ClassWithFields.send(@result[:static_ro][0]).should equal_clr_string(@result[:static_ro][1])
  end
end

#TODO:Can't use these shared behaviors due to Codeplex bug #1651
describe :modifying_fields, :shared => true do
  it "fields works" do
    @klass.send("#{@methods[0]}=", "bar")
    @klass.send(@methods[0]).should equal_clr_string("bar")
  end

  it "const fields raises NoMethodError" do
    lambda {ClassWithFields.send("#{@methods[1]}=", "foo")}.should raise_error(NoMethodError)
  end

  it "readonly fields raises NoMethodError" do
    lambda {ClassWithFields.send("#{@methods[2]}=", "foo")}.should raise_error(NoMethodError)
  end

  it "static fields work" do
    ClassWithFields.send("#{@methods[3]}=", "foo")
    ClassWithFields.send(@methods[3]).should equal_clr_string("foo")
  end

  it "static readonly fields raise NoMethodError" do
    lambda {ClassWithFields.send("#{@methods[4]}=", "foo")}.should raise_error(NoMethodError)
  end
end
