require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'
require File.dirname(__FILE__) + '/fixtures/strings'

describe "YAML.load" do
  after :each do
    rm_r $test_file
  end
  
  it "returns a document from current io stream when io provided" do
    File.open($test_file, 'w') do |io|
      YAML.dump( ['badger', 'elephant', 'tiger'], io )
    end
    File.open($test_file) { |yf| YAML.load( yf ) }.should == ['badger', 'elephant', 'tiger']
  end
  
  it "loads strings" do
    strings = ["str",
               "\s\tstr\s\t",
               "'str'",
               "str",
               " str",
               "'str'",
               "\"str\"",
                "\n str",
                "---  str",
                "---\nstr",
                "--- \nstr",
                "--- \n str",
                "--- 'str'"
              ]
    strings.each do |str|
      YAML.load(str).should == "str"
    end
  end  

  it "fails on invalid keys" do
    lambda { YAML.load("key1: value\ninvalid_key") }.should raise_error(ArgumentError)
  end

  it "accepts symbols" do
    YAML.load( "--- :locked" ).should == :locked
  end

  it "accepts numbers" do
    YAML.load("47").should == 47
    YAML.load("-1").should == -1
  end

  it "accepts collections" do
    expected = ["a", "b", "c"]
    YAML.load("--- \n- a\n- b\n- c\n").should == expected
    YAML.load("--- [a, b, c]").should == expected
    YAML.load("[a, b, c]").should == expected
  end

  it "parses start markers" do
    YAML.load("---\n").should == nil
    YAML.load("--- ---\n").should == "---"
    YAML.load("--- abc").should == "abc"
  end

  it "does not escape symbols" do   
    YAML.load("*.").should == "*."
    YAML.load("&.").should == "&."
    YAML.load("@a").should == "@a"
    YAML.load(":a").should == :a
    YAML.load("?a").should == "?a"
    YAML.load("-a").should == "-a"
    YAML.load(">a").should == ">a"
    YAML.load("> a").should == "a"
    YAML.load("- x").should == ["x"]
    YAML.load(",").should == ","
    YAML.load(",,").should == ",,"
    YAML.load(".").should == "."
    YAML.load("..").should == ".."
    YAML.load(",\t").should == ","
    YAML.load(",\r").should == ",\r"
    YAML.load("*").should == "*"
    YAML.load("&").should == "&"

    YAML.load("foobar: >= 123").should == { "foobar" => ">= 123"}
    YAML.load("foobar: |= 567").should == { "foobar" => "|= 567"}
    YAML.load("foobar: .").should == { "foobar" => "."}
  end
  
  it "accepts anchors" do
    YAML.load("&abc@").should == "@"
    YAML.load("&abc{}").should == {}
    YAML.load("&abc sda").should ==  "sda"
  end
  
  it "terminates plain scalars with a space or a line break" do
    YAML.load("{hello: uv}").should == { "hello" => "uv" }
    YAML.load("{hello\txy:\tuv}").should == { "hello\txy:\tuv" => nil }
    YAML.load("{hello\rxy:\ruv}").should == { "hello\rxy:\ruv" => nil }

    YAML.load("[hello,\nx,y]").should == ["hello", "x,y"]
    YAML.load("[hello, ,,a]").should == ["hello", ",,a"]
    YAML.load("[hello, x,y]").should == ["hello", "x,y"]
    
    YAML.load("[hello,\tx,y]").should == ["hello,\tx,y"]
    YAML.load("[hello,\rx,y]").should == ["hello,\rx,y"]
  end
  
  it "normalizes \\r\\n and \\n line breaks and removes the first one" do
    YAML.load("str\r\nx").should == "str x"
    YAML.load("str\r\n\r\nx").should == "str\nx"
    YAML.load("str\r\n\r\n\r\nx").should == "str\n\nx"

    YAML.load("str\nx").should == "str x"
    YAML.load("str\n\nx").should == "str\nx"
    YAML.load("str\n\n\nx").should == "str\n\nx"
  end
  
  it "trims whitespace if not followed by a non-whitespace" do
    YAML.load("str \t\t\t\tx").should == "str \t\t\t\tx"
    YAML.load("str\t\t\t\tx").should == "str\t\t\t\tx"
    YAML.load("str\r\r\r\r").should == "str\r\r\r\r"
    YAML.load("str      x").should == "str      x"
    YAML.load("str  \t    ").should == "str"
    YAML.load("str \t\t\r").should == "str \t\t\r"
    YAML.load("str\nx").should == "str x"
    YAML.load("str\rx").should == "str\rx"
    YAML.load("str\x85").should == "str\205"
  end
  
  it "terminates plain scalars with a space or a line break" do
    YAML.load("{hello: uv}").should == { "hello" => "uv" }
    YAML.load("{hello\txy:\tuv}").should == { "hello\txy:\tuv" => nil }
    YAML.load("{hello\rxy:\ruv}").should == { "hello\rxy:\ruv" => nil }

    YAML.load("[hello,\nx,y]").should == ["hello", "x,y"]
    YAML.load("[hello, ,,a]").should == ["hello", ",,a"]
    YAML.load("[hello, x,y]").should == ["hello", "x,y"]
    
    YAML.load("[hello,\tx,y]").should == ["hello,\tx,y"]
    YAML.load("[hello,\rx,y]").should == ["hello,\rx,y"]
  end
  
  it "normalizes \\r\\n and \\n line breaks and removes the first one" do
    YAML.load("str\r\nx").should == "str x"
    YAML.load("str\r\n\r\nx").should == "str\nx"
    YAML.load("str\r\n\r\n\r\nx").should == "str\n\nx"

    YAML.load("str\nx").should == "str x"
    YAML.load("str\n\nx").should == "str\nx"
    YAML.load("str\n\n\nx").should == "str\n\nx"
  end
  
  it "trims whitespace if not followed by a non-whitespace" do
    YAML.load("str \t\t\t\tx").should == "str \t\t\t\tx"
    YAML.load("str\t\t\t\tx").should == "str\t\t\t\tx"
    YAML.load("str\r\r\r\r").should == "str\r\r\r\r"
    YAML.load("str      x").should == "str      x"
    YAML.load("str  \t    ").should == "str"
    YAML.load("str \t\t\r").should == "str \t\t\r"
    YAML.load("str\nx").should == "str x"
    YAML.load("str\rx").should == "str\rx"
    YAML.load("str\x85").should == "str\205"
  end

  it "works with block sequence shortcuts" do
    block_seq = "- - - one\n    - two\n    - three"
    YAML.load(block_seq).should == [[["one", "two", "three"]]]
  end

  it "works on complex keys" do
    expected = { 
      [ 'Detroit Tigers', 'Chicago Cubs' ] => [ Date.new( 2001, 7, 23 ) ],
      [ 'New York Yankees', 'Atlanta Braves' ] => [ Date.new( 2001, 7, 2 ), 
                                                    Date.new( 2001, 8, 12 ), 
                                                    Date.new( 2001, 8, 14 ) ] 
    }
    YAML.load($complex_key_1).should == expected
  end
  
  it "loads a symbol key that contains spaces" do
    string = ":user name: This is the user name."
    expected = { :"user name" => "This is the user name."}
    YAML.load(string).should == expected
  end
  
  it "ignores whitespace" do
    YAML.load("!timestamp \s\t '\s\t 2009-03-22 \s\t 00:00:00 \s\t'").class.should == Time
  end

  ### http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=375
  it 'accepts symbols in an array' do
    string   = ['[:a]', '[:a, :b, :c, :d]' ]
    expected = [ [:a] ,  [:a, :b, :c, :d]  ]
    string.size.times do |i|
      YAML.load(string[i]).should == expected[i]
    end
  end

  ### http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=375
  it 'accepts symbols in value' do
    YAML.load('foo: [:a, :b]').should == {'foo' => [:a, :b]}
  end

  ### http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=2044
  it 'accepts mappings nested into sequences' do
    YAML.load('[{ a: b}]').should == [{"a" => "b"}]
  end
  
  it 'loads structs' do
    Struct.new('YamlSpecs_Struct1', :f, :g)
    begin
      s = YAML.load("--- !ruby/struct:YamlSpecs_Struct1 \nf: 1\ng: 2\n")
      s.class.should == Struct::YamlSpecs_Struct1
      s.f.should == 1
      s.g.should == 2
    ensure
      Struct.send(:remove_const, :YamlSpecs_Struct1)
    end
  end
  
  it 'ignores non-string keys' do
    Struct.new('YamlSpecs_Struct1', :f, :g, :true)
    begin
      s = YAML.load("--- !ruby/struct:YamlSpecs_Struct1 \n:f: 1\ng: 2\ntrue: 3")
      s.class.should == Struct::YamlSpecs_Struct1
      s.f.should == nil
      s.g.should == 2
      s.true.should == nil
    ensure
      Struct.send(:remove_const, :YamlSpecs_Struct1)
    end
  end
  
  it 'resolves the structs by name, first tries Struct::{name} then Object::{name}' do
    Struct.new('YamlSpecs_Struct2', :f, :g)
    YamlSpecs_Struct2 = Struct.new(:f, :g)
    begin
      begin
        s = YAML.load("--- !ruby/struct:YamlSpecs_Struct2 \nf: 1\ng: 2")
        s.class.should == Struct::YamlSpecs_Struct2
        s.f.should == 1
        s.g.should == 2
      ensure
        Struct.send(:remove_const, :YamlSpecs_Struct2)
      end
  
      s = YAML.load("--- !ruby/struct:YamlSpecs_Struct2 \nf: 1\ng: 2")
      s.class.should == Object::YamlSpecs_Struct2
      s.f.should == 1
      s.g.should == 2
    ensure
      Object.send(:remove_const, :YamlSpecs_Struct2)
    end
  end
end
