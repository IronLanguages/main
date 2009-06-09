require File.dirname(__FILE__) + '/../spec_helper'

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
  #pragma warning restore 414
  EOL
  before :each do
    @klass = ClassWithFields.new
  end

  describe "works with public" do
    it "fields" do
      @klass.field.should equal_clr_string("field")
    end

    it "const fields" do
      ClassWithFields.constField.should equal_clr_string("const")
    end

    it "readonly fields" do
      @klass.readOnlyField.should equal_clr_string("readonly")
    end

    it "static fields" do
      ClassWithFields.staticField.should equal_clr_string("static")
    end

    it "static readonly fields" do
      ClassWithFields.staticReadOnlyField.should equal_clr_string("static readonly")
    end
  end

  describe "works with protected" do
    it "fields" do
      @klass.protectedField.should equal_clr_string("protected field")
    end

    it "const fields" do
      ClassWithFields.protectedConstField.should equal_clr_string("protected const")
    end

    it "readonly fields" do
      @klass.protectedReadOnlyField.should equal_clr_string("protected readonly")
    end

    it "static fields" do
      ClassWithFields.protectedStaticField.should equal_clr_string("protected static")
    end

    it "static readonly fields" do
      ClassWithFields.protectedStaticReadOnlyField.should equal_clr_string("protected static readonly")
    end
  end
  
  if IronRuby.dlr_config.private_binding
    describe "works with private" do
      it "fields" do
        @klass.privateField.should equal_clr_string("private field")
      end

      it "const fields" do
        @klass.privateConstField.should equal_clr_string("private const")
      end

      it "readonly fields" do
        @klass.privateReadOnlyField.should equal_clr_string("private readonly")
      end

      it "static fields" do
        ClassWithFields.privateStaticField.should equal_clr_string("private static")
      end

      it "static readonly fields" do
        ClassWithFields.privateStaticReadOnlyField.should equal_clr_string("private static readonly")
      end
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
end

