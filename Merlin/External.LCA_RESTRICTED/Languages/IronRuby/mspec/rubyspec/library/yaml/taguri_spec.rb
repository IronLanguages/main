require File.dirname(__FILE__) + '/../../spec_helper'
require File.dirname(__FILE__) + '/fixtures/common'
require File.dirname(__FILE__) + '/fixtures/example_class'

describe "Object#taguri" do
  it "returns standard YAML tag for primitive YAML types" do
    true.taguri.should == "tag:yaml.org,2002:bool#yes"
    false.taguri.should == "tag:yaml.org,2002:bool#no"
    nil.taguri.should == "tag:yaml.org,2002:null"
    1.taguri.should == "tag:yaml.org,2002:int:Fixnum"
    10000000000000000.taguri.should == "tag:yaml.org,2002:int:Bignum"
    1.2.taguri.should == "tag:yaml.org,2002:float"
    "str".taguri.should == "tag:yaml.org,2002:str"
    "\0".taguri.should == "tag:yaml.org,2002:str"
    [].taguri.should == "tag:yaml.org,2002:seq"
    {}.taguri.should == "tag:yaml.org,2002:map"
    Time.new.taguri.should == "tag:yaml.org,2002:timestamp"
    Date.new.taguri.should == "tag:yaml.org,2002:timestamp#ymd"
  end
  
  it "returns Ruby specific tag for Ruby types" do
    :sym.taguri.should == "tag:ruby.yaml.org,2002:sym"
    /regexp/.taguri.should == "tag:ruby.yaml.org,2002:regexp"
    (1..2).taguri.should == "tag:ruby.yaml.org,2002:range"
    Object.new.taguri.should == "tag:ruby.yaml.org,2002:object"
    Exception.new.taguri.should == "tag:ruby.yaml.org,2002:exception"
  end
  
  it "appends name of a subclass" do
    YamlSpecs::StringSubclass.new.taguri.should == "tag:yaml.org,2002:str:YamlSpecs::StringSubclass"
    YamlSpecs::ArraySubclass.new.taguri.should == "tag:yaml.org,2002:seq:YamlSpecs::ArraySubclass"
    YamlSpecs::HashSubclass.new.taguri.should == "tag:yaml.org,2002:map:YamlSpecs::HashSubclass"
    YamlSpecs::TimeSubclass.new.taguri.should == "tag:yaml.org,2002:timestamp:YamlSpecs::TimeSubclass"
    YamlSpecs::DateSubclass.new.taguri.should == "tag:yaml.org,2002:timestamp#ymd:YamlSpecs::DateSubclass"
    YamlSpecs::RangeSubclass.new(1,2).taguri.should == "tag:ruby.yaml.org,2002:range:YamlSpecs::RangeSubclass"
    YamlSpecs::RegexpSubclass.new(//).taguri.should == "tag:ruby.yaml.org,2002:regexp:YamlSpecs::RegexpSubclass"
    IOError.new.taguri.should == "tag:ruby.yaml.org,2002:exception:IOError"
  end
end