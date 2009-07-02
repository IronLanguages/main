describe "A .NET numeric", :shared => true do
  before(:each) do
    @class = @method
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
    lambda { @class.induced_from(@minvalue -1 + @bignum)}.should raise_error(RangeError)
    lambda { @class.induced_from(@maxvalue + 1 + @bignum)}.should raise_error(RangeError)
  end
end

describe "A .NET numeric, induceable from Fixnum", :shared => true do
  before(:each) do
    @class = @method
    @bignum = Bignum.Create(0)
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
    @class.new(1).size.should == @size
  end
end
