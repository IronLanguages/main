describe :name_mangling, :shared => true do
  @methods = %w{Unique snake_case CamelCase PartialCapitalId __LeadingCamelCase __leading_snake_case fNNBar foNBar MyUIApp MyIdYA NaNa Mixed_Snake_case}
  before(:each) do
    @methods = %w{Unique snake_case CamelCase PartialCapitalId __LeadingCamelCase __leading_snake_case fNNBar foNBar MyUIApp MyIdYA NaNa Mixed_Snake_case}
    @a_methods = %w{a A}
    @methods << "foNBar" 
    @non_mangle_methods = %w{NNNBar NaN CAPITAL PartialCapitalID }
    @all_methods = @methods + @a_methods + @non_mangle_methods
  end
  
  it "works with correct .NET names" do
    @all_methods.each do |meth|
      @objs.each do |obj|
        obj.send(meth).should equal_clr_string(meth)
      end
    end
  end

  @methods.each do |meth|
    it "works with mangled name (#{meth}) if not conflicting" do
      @objs.each do |obj|
        obj.send(meth.to_snake_case).should equal_clr_string(meth)
      end
    end
  end

  it "doesn't work with conflicting method names (where the mangled name is another method)" do
    @a_methods.each do |meth|
      @objs.each do |obj|
        obj.send(meth.to_snake_case).should equal_clr_string(meth.to_snake_case)
      end
    end
  end

  it "doesn't work with extra trailing or leading underscores" do
    @all_methods.each do |meth|
      @objs.each do |obj|
        lambda { obj.send("_#{meth}") }.should raise_error(NoMethodError)
        lambda { obj.send("#{meth}_") }.should raise_error(NoMethodError)
        lambda { obj.send("_#{meth.to_snake_case}")}.should raise_error(NoMethodError)
        lambda { obj.send("#{meth.to_snake_case}_")}.should raise_error(NoMethodError)
      end
    end
  end

  it "doesn't work with extra internal underscores" do
    @all_methods.each do |meth|
      @objs.each do |obj|
        fake_meth = meth.to_snake_case.gsub(/([A-Za-z1-9])_([A-Za-z1-9])/, '\1__\2')
        next if (fake_meth == meth || fake_meth == meth.to_snake_case)
        lambda { obj.send("#{fake_meth}")}.should raise_error(NoMethodError)
      end
    end
  end

  it "doesn't work with mixed case" do
    test_methods = @methods + @methods.map {|m| m.to_snake_case}.uniq
    test_methods.each do |meth|
      @objs.each do |obj|
        fake_upper_meth = meth.sub(/([a-z])/) {|l| $1.upcase}
        fake_lower_meth = meth.sub(/([A-Z])/) {|l| $1.downcase}
        next if test_methods.include?(fake_upper_meth) || test_methods.include?(fake_lower_meth)
        lambda { obj.send("#{fake_upper_meth}")}.should raise_error(NoMethodError)
        lambda { obj.send("#{fake_lower_meth}")}.should raise_error(NoMethodError)
      end
    end
  end
  it "works with correct .NET names" do
    @all_methods.each do |meth|
      @objs.each do |obj|
        obj.send(meth).should equal_clr_string(meth)
      end
    end
  end

  @methods.each do |meth|
    it "works with mangled name (#{meth}) if not conflicting" do
      @objs.each do |obj|
        obj.send(meth.to_snake_case).should equal_clr_string(meth)
      end
    end
  end

  it "doesn't work with conflicting method names (where the mangled name is another method)" do
    @a_methods.each do |meth|
      @objs.each do |obj|
        obj.send(meth.to_snake_case).should equal_clr_string(meth.to_snake_case)
      end
    end
  end

  it "doesn't work with extra trailing or leading underscores" do
    @all_methods.each do |meth|
      @objs.each do |obj|
        lambda { obj.send("_#{meth}") }.should raise_error(NoMethodError)
        lambda { obj.send("#{meth}_") }.should raise_error(NoMethodError)
        lambda { obj.send("_#{meth.to_snake_case}")}.should raise_error(NoMethodError)
        lambda { obj.send("#{meth.to_snake_case}_")}.should raise_error(NoMethodError)
      end
    end
  end

  it "doesn't work with extra internal underscores" do
    @all_methods.each do |meth|
      @objs.each do |obj|
        fake_meth = meth.to_snake_case.gsub(/([A-Za-z1-9])_([A-Za-z1-9])/, '\1__\2')
        next if (fake_meth == meth || fake_meth == meth.to_snake_case)
        lambda { obj.send("#{fake_meth}")}.should raise_error(NoMethodError)
      end
    end
  end

  it "doesn't work with mixed case" do
    test_methods = @methods + @methods.map {|m| m.to_snake_case}.uniq
    test_methods.each do |meth|
      @objs.each do |obj|
        fake_upper_meth = meth.sub(/([a-z])/) {|l| $1.upcase}
        fake_lower_meth = meth.sub(/([A-Z])/) {|l| $1.downcase}
        next if test_methods.include?(fake_upper_meth) || test_methods.include?(fake_lower_meth)
        lambda { obj.send("#{fake_upper_meth}")}.should raise_error(NoMethodError)
        lambda { obj.send("#{fake_lower_meth}")}.should raise_error(NoMethodError)
      end
    end
  end
end
