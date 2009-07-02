require File.dirname(__FILE__) + '/../spec_helper'
require File.dirname(__FILE__) + '/shared/access'

describe "Reading .NET Fields" do
  csc <<-EOL
  #pragma warning disable 414
  public partial class ClassWithFields {
    public string field = "field";
    public const string constField = "const";
    public readonly string readOnlyField = "readonly";
    public static string staticField = "static";
    public static readonly string staticReadOnlyField = "static readonly";
  
    private string privateField = "private field";
    private const string privateConstField = "private const";
    private readonly string privateReadOnlyField = "private readonly";
    private static string privateStaticField = "private static";
    private static readonly string privateStaticReadOnlyField = "private static readonly";
   
    protected string protectedField = "protected field";
    protected const string protectedConstField = "protected const";
    protected readonly string protectedReadOnlyField = "protected readonly";
    protected static string protectedStaticField = "protected static";
    protected static readonly string protectedStaticReadOnlyField = "protected static readonly";
  }

  #pragma warning disable 649
  public class InternalFieldTester {
    internal string MyField;

    public InternalFieldTester() {
      var runtime = ScriptRuntime.CreateFromConfiguration();
      var engine = runtime.GetEngine("IronRuby");
      var scope = engine.CreateScope();
      scope.SetVariable("foo", this);
      engine.Execute("foo.MyField = 'Hello'", scope);
    }
  }
  #pragma warning restore 414, 649
  EOL
  before :each do
    @klass = ClassWithFields.new
  end

  describe "works with public" do
    before(:each) do
      @result = {
        :field => [:field, "field"],
        :const_field => [:constField, "const"],
        :readonly => [:readOnlyField, "readonly"],
        :static => [:staticField, "static"],
        :static_ro => [:staticReadOnlyField, "static readonly"]
      }
    end
    it_behaves_like :accessing_fields, nil
  end

  describe "works with protected" do
    before(:each) do
      @result = {
        :field => [:protectedField, "protected field"],
        :const_field => [:protectedConstField, "protected const"],
        :readonly => [:protectedReadOnlyField, "protected readonly"],
        :static => [:protectedStaticField, "protected static"],
        :static_ro => [:protectedStaticReadOnlyField, "protected static readonly"]
      }
    end
    it_behaves_like :accessing_fields, nil
  end
  
  if IronRuby.dlr_config.private_binding
    describe "works with private" do
      before(:each) do
        @result = {
          :field => [:privateField, "private field"],
          :const_field => [:privateConstField, "private const"],
          :readonly => [:privateReadOnlyField, "private readonly"],
          :static => [:privateStaticField, "private static"],
          :static_ro => [:privateStaticReadOnlyField, "private static readonly"]
        }
        end
      it_behaves_like :accessing_fields, nil
    end
  end
end

describe "Setting .NET Fields" do
  before :each do
    @klass = ClassWithFields.new
  end

  describe "with public" do
    it "fields works" do
      @klass.field = "bar"
      @klass.field.should equal_clr_string("bar")
    end

    it "const fields raises NoMethodError" do
      lambda {ClassWithFields.constField = "foo"}.should raise_error(NoMethodError)
    end

    it "readonly fields" do
      lambda {ClassWithFields.readOnlyField = "foo"}.should raise_error(NoMethodError)
    end

    it "static fields" do
      ClassWithFields.staticField = "foo"
      ClassWithFields.staticField.should equal_clr_string("foo")
    end

    it "static readonly fields" do
      lambda {ClassWithFields.staticReadOnlyField = "foo"}.should raise_error(NoMethodError)
    end
  end

  describe "with protected" do
    it "fields works" do
      @klass.protectedField = "bar"
      @klass.protectedField.should equal_clr_string("bar")
    end

    it "const fields raises NoMethodError" do
      lambda {ClassWithFields.protectedConstField = "foo"}.should raise_error(NoMethodError)
    end

    it "readonly fields" do
      lambda {ClassWithFields.protectedReadOnlyField = "foo"}.should raise_error(NoMethodError)
    end

    it "static fields" do
      ClassWithFields.protectedStaticField = "foo"
      ClassWithFields.protectedStaticField.should equal_clr_string("foo")
    end

    it "static readonly fields" do
      lambda {ClassWithFields.protectedStaticReadOnlyField = "foo"}.should raise_error(NoMethodError)
    end
  end

  if IronRuby.dlr_config.private_binding
    describe "with private" do
      it "fields works" do
        @klass.privateField = "bar"
        @klass.privateField.should equal_clr_string("bar")
      end

      it "const fields raises NoMethodError" do
        lambda {ClassWithFields.privateConstField = "foo"}.should raise_error(NoMethodError)
      end

      it "readonly fields" do
        lambda {ClassWithFields.privateReadOnlyField = "foo"}.should raise_error(NoMethodError)
      end

      it "static fields" do
        ClassWithFields.privateStaticField = "foo"
        ClassWithFields.privateStaticField.should equal_clr_string("foo")
      end

      it "static readonly fields" do
        lambda {ClassWithFields.privateStaticReadOnlyField = "foo"}.should raise_error(NoMethodError)
      end
    end
  end

  describe "Internal fields" do
    #TODO: shared behavior when 1651 is fixed
    it "can't be assigned from within a IronRuby engine in the constructor" do
      lambda {InternalFieldTester.new}.should raise_error NoMethodError
    end
  end
end

