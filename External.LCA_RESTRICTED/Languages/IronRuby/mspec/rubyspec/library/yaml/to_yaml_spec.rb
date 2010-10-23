require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'
require File.dirname(__FILE__) + '/fixtures/example_class'

describe "Object#to_yaml" do

  it "returns the YAML representation of an Array object" do
    %w( 30 ruby maz irb 99 ).to_yaml.should == "--- \n- \"30\"\n- ruby\n- maz\n- irb\n- \"99\"\n"
  end

  it "returns the YAML representation of a Hash object" do
    { "a" => "b"}.to_yaml.should match_yaml("--- \na: b\n")
  end

  it "returns the YAML representation of a Class object" do
    FooBar.new("baz").to_yaml.should match_yaml("--- !ruby/object:FooBar\nname: baz\n")

  end

  it "returns the YAML representation of a Date object" do
    Date.parse('1997/12/30').to_yaml.should == "--- 1997-12-30\n"
  end

  it "returns the YAML representation of a FalseClass" do
    false_klass = false
    false_klass.should be_kind_of(FalseClass)
    false_klass.to_yaml.should == "--- false\n"
  end

  it "returns the YAML representation of a Float object" do
    float = 1.2
    float.should be_kind_of(Float)
    float.to_yaml.should == "--- 1.2\n"
  end
  
  it "returns the YAML representation of an Integer object" do
    int = 20
    int.should be_kind_of(Integer)
    int.to_yaml.should == "--- 20\n"
  end
  
  it "returns the YAML representation of a NilClass object" do
    nil_klass = nil
    nil_klass.should be_kind_of(NilClass)
    nil_klass.to_yaml.should == "--- \n"
  end
  
  it "returns the YAML represenation of a RegExp object" do
    Regexp.new('^a-z+:\\s+\w+').to_yaml.should == "--- !ruby/regexp /^a-z+:\\s+\\w+/\n"
  end
  
  it "returns the YAML representation of a String object" do
    "I love Ruby".to_yaml.should == "--- I love Ruby\n"
  end  
  
  it "uses Literal style for multiline strings with no indentation" do
    "Say\nhello\n\nworld".to_yaml.should == "--- |-\nSay\nhello\n\nworld\n"
    "Say\nhello\n\nworld\n".to_yaml.should == "--- |\nSay\nhello\n\nworld\n\n"
    "Say\nhello\n\nworld\n\n".to_yaml.should == "--- |+\nSay\nhello\n\nworld\n\n"
    "Say\nhello\n\nworld\n\n\n".to_yaml.should == "--- |+\nSay\nhello\n\nworld\n\n\n"
  end
  
  it "escapes binary data using base64" do
    ((0..0x1f).to_a + (0x7f..0xff).to_a - [0x0a, 0x0d]).each do |s|
      s.chr.to_yaml.should == "--- !binary |\n#{[s.chr].pack('m')}\n"
    end
  end
  
  it "doesn't escape or quote characters that are not special" do
    (('a'..'z').to_a + ('A'..'Z').to_a + ['<', '(', ')', '$', '+', '.', '/', '\\', '_', '`', '^', '\r']).each do |s|
      s.to_yaml.should == "--- #{s}\n"
    end
  end
  
  it "quotes special characters and characters that represent non-string scalars" do
    (('0'..'9').to_a + [' ', '!', '#', '%', '&', "'", '*', '-', ':', '=', '>', '?', '@', '[', ']', '{', '|', '}', ']']).each do |s|
      s.to_yaml.should == "--- \"#{s}\"\n"
    end
    
    '"'.to_yaml.should == "--- \"\\\"\"\n"
    
    # multi-line scalar
    "\n".to_yaml.should == "--- |\n\n\n"
  end
  
  it "quotes strings that would be parsed as scalars of different type if left unquoted" do
    ["~", "null", "Null", "null", "NULL", " ",
     "yes", "Yes", "YES", "true", "True", "TRUE", "on", "On", "ON", 
     "no", "No", "NO", "false", "False", "FALSE", "off", "Off", "OFF",
     "123", "0x123", "0123", 
     "-123", "-0x123", "-0123",
     "0.3", 
     ".inf", ".Inf", ".INF",
     "-.inf", "-.Inf", "-.INF",
     ".nan", ".NaN", ".NAN",
     "<<",
     "=",
     ":xxx"].each do |s|
       s.to_yaml.should == "--- \"#{s}\"\n"
    end
  end

  it "doesn't quote strings that can be parsed as Ruby integer" do
    ["1_2_3", "-1_2_3"].each do |s|
       s.to_yaml.should == "--- #{s}\n"
    end
  end

  it "returns the YAML representation of a Struct object" do
    Person = Struct.new(:name, :gender)
    Person.new("Jane", "female").to_yaml.should match_yaml("--- !ruby/struct:Person\nname: Jane\ngender: female\n")
  end

  it "returns the YAML representation of a Symbol object" do
    :symbol.to_yaml.should ==  "--- :symbol\n"
  end
  
  it "returns the YAML representation of a Time object" do
    Time.utc(2000,"jan",1,20,15,1).to_yaml.should == "--- 2000-01-01 20:15:01 Z\n"
  end
  
  it "returns the YAML representation of a TrueClass" do
    true_klass = true
    true_klass.should be_kind_of(TrueClass)
    true_klass.to_yaml.should == "--- true\n"
  end  

  it "returns the YAML representation of a Error object" do
    StandardError.new("foobar").to_yaml.should match_yaml("--- !ruby/exception:StandardError\nmessage: foobar\n")
  end

  it "returns the YAML representation for Range objects" do
    yaml = Range.new(1,3).to_yaml
    yaml.include?("!ruby/range").should be_true
    yaml.include?("begin: 1").should be_true
    yaml.include?("end: 3").should be_true
    yaml.include?("excl: false").should be_true
  end

  it "returns the YAML representation of numeric constants" do
    (0.0/0.0).to_yaml.should == "--- .NaN\n"
    (1.0/0.0).to_yaml.should == "--- .Inf\n"
    (-1.0/0.0).to_yaml.should == "--- -.Inf\n"
    (0.0).to_yaml.should == "--- 0.0\n"
  end

  it "returns the YAML representation of an array of hashes" do
    players = [{"a" => "b"}, {"b" => "c"}]
    players.to_yaml.should == "--- \n- a: b\n- b: c\n"
  end
  
  it "returns the YAML representation of a user class" do
    YamlSpecs::Outer.new.to_yaml.should == <<EOF
--- !ruby/object:YamlSpecs::Outer\s
outer1: 1
outer2: !ruby/object:YamlSpecs::Inner\s
  inner1: 1
  inner2: 2
EOF
  end

  it "calls to_yaml on nested objects" do
    YamlSpecs::OuterToYaml.new.to_yaml.should == <<EOF
--- !ruby/object:YamlSpecs::OuterToYaml\s
outer1: 1
outer2: !ruby/object:YamlSpecs::InnerToYaml\s
  inner1: 1
  inner2: 2
EOF
  end

  it "calls to_yaml on nested objects with Emitter" do
    ScratchPad.record({})
    YamlSpecs::OuterToYaml.new.to_yaml

    emitter = ScratchPad.recorded[:emitter]
    ScratchPad.recorded[:level].should == 1
  end
  
  it "indents keys and multiline values of maps" do
    x = Object.new
    y = Object.new
    x.instance_variable_set(:@foo1, y)
    x.instance_variable_set(:@foo2, "x\ny\nz\n")
    y.instance_variable_set(:@foo3, 1)
    y.instance_variable_set(:@bar, "hello\nworld\n")
    x.to_yaml.should == <<EOF
--- !ruby/object 
foo1: !ruby/object 
  bar: |
    hello
    world

  foo3: 1
foo2: |
  x
  y
  z

EOF
  end
   
  it "indents multiline values of sequences" do
     x = ["hello\nworld\n", 1, ["hello\nworld\n", 2], "hello\nworld\n", "hello\nworld\n"]
     x.to_yaml.should == <<EOF
--- 
- |
  hello
  world

- 1
- - |
    hello
    world

  - 2
- |
  hello
  world

- |
  hello
  world

EOF

  end
end