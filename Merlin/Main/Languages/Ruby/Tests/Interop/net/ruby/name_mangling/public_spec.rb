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
    @obj = PublicNameHolder.new
    @subobj = SubPublicNameHolder.new
    @a_methods = %w{a A}
    @methods = %w{Unique snake_case CamelCase Mixed_Snake_case CAPITAL PartialCapitalID __LeadingUnderCamel __leading_snake_case}
    @all_methods = @methods + @a_methods
  end

  it "doesn't work with extra trailing or leading underscores" do
    @all_methods.each do |meth|
      [@obj, @subobj].each do |obj|
        lambda { obj.send(:"_#{meth}") }.should raise_error(NoMethodError)
        lambda { obj.send(:"_#{meth}") }.should raise_error(NoMethodError)
      end
    end
  end
end
