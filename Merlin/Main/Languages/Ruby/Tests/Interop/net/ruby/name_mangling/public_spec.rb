require File.dirname(__FILE__) + '/../../spec_helper'

describe "Name mangling on public types" do
  csc <<-EOL
    public class PublicNameHolder {
      public string a() { return "a";}
      public string A() { return "A";}
      public string Unique() { return "Unique"; }
      public string snake_case() {return "snake_case";}
      public string CamelCase() {return "CamelCase";}
      public string Mixed_Snake_case() {return "Mixed_Snake_case";}
      public string CAPITAL() { return "CAPITAL";}
      public string PartialCapitalID() { return "PartialCapitalID";}
      public string __LeadingCamelCase() { return "__LeadingCamelCase";}
      public string __leading_snake_case() { return "__leading_snake_case";}
    }

    public class SubPublicNameHolder : PublicNameHolder {
    }
  EOL

  before(:each) do
    @objs = [PublicNameHolder.new, SubPublicNameHolder.new, Class.new(PublicNameHolder).new, Class.new(SubPublicNameHolder).new]
    @a_methods = %w{a A}
    @methods = %w{Unique snake_case CamelCase Mixed_Snake_case CAPITAL PartialCapitalID __LeadingCamelCase __leading_snake_case}
    @all_methods = @methods + @a_methods
  end
  
  it "works with correct .NET names" do
    @all_methods.each do |meth|
      @objs.each do |obj|
        obj.send(meth).should equal_clr_string(meth)
      end
    end
  end

  it "works with mangled name if not conflicting" do
    @methods.each do |meth|
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
