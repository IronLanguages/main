describe "A .NET numeric", :shared => true do
  before(:each) do
    @class = @method
    @minvalue = NumericHelper.min_of(@class)
    @maxvalue = NumericHelper.max_of(@class)
    @bignum = Bignum.Create(0)
  end

  it "can be induced via an int" do
    a = @class.induced_from(100)
    a.should be_kind_of @class
  end

  it "can be induced from a Float" do
    a = @class.induced_from(100.0)
    a.should be_kind_of @class
    b = @class.induced_from(@minvalue.to_f)
    c = @class.induced_from(@maxvalue.to_f)
    b.should be_kind_of @class
    c.should be_kind_of @class
  end

  it "can be induced from a Bignum" do
    a = @class.induced_from(@minvalue + @bignum)
    b = @class.induced_from(@maxvalue + @bignum)
    a.should be_kind_of @class
    b.should be_kind_of @class
  end

  it "raises a RangeError if out of range for Float" do
    lambda { @class.induced_from(@minvalue.to_f - 1)}.should raise_error(RangeError)
    lambda { @class.induced_from(@maxvalue.to_f + 1)}.should raise_error(RangeError)
  end

  it "raises a RangeError if out of range for Bignum" do
    lambda { @class.induced_from(2**512)}.should raise_error(RangeError)
    lambda { @class.induced_from(-2**512)}.should raise_error(RangeError)
  end
end

describe "A .NET numeric, induceable from Fixnum", :shared => true do
  before(:each) do
    @class = @method
    @bignum = Bignum.Create(0)
    @minvalue = NumericHelper.min_of(@class)
    @maxvalue = NumericHelper.max_of(@class)
  end

  it "can be induced via an int" do
    a = @class.induced_from(@minvalue)
    b = @class.induced_from(@maxvalue)
    a.should be_kind_of @class
    b.should be_kind_of @class
  end

  it "raises a RangeError if out of range for int" do
    lambda { @class.induced_from(@minvalue - 1)}.should raise_error(RangeError)
    lambda { @class.induced_from(@maxvalue + 1)}.should raise_error(RangeError)
  end
end

describe :numeric_size, :shared => true do
  before(:each) do
    @class = @method
  end

  it "has a size" do
    @class.Parse("1").size.should == @size
  end
end

describe :numeric_conversion, :shared => true do
  before(:each) do
    @class = @method
    @obj = @class.Parse("0")
  end

  it "has a to_i method" do
    @class.should have_instance_method(:to_i)
  end

  it "returns a Fixnum from the to_i method" do
    @obj.to_i.should be_kind_of(Fixnum)
  end

  it "has a to_int method" do
    @class.should have_instance_method(:to_int)
  end

  it "returns a Fixnum from the to_int method" do
    @obj.to_int.should be_kind_of(Fixnum)
  end

  it "has a to_s method" do
    @class.should have_instance_method(:to_s)
  end

  it "returns a string from the to_s method" do
    @obj.to_s.should == "0"
  end

  it "has an inspect method" do
    @class.should have_instance_method(:inspect)
  end

  it "returns a string of the format 'value (Class) (unless int32)'" do
    if @class != Fixnum
      c = @class.name.match(/::([^:]*)$/)[1]
      @obj.inspect.should == "0 (#{c})"
    else
      @obj.inspect.should == "0"
    end
  end
end
