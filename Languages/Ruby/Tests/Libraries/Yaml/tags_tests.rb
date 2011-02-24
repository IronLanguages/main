require 'test/unit'
require 'yaml'
require 'bigdecimal'

class Load_tests < Test::Unit::TestCase
  # def setup
  # end

  # def teardown
  # end

  def test_tagged_classes		  
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:array'] == Array	
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:exception'] == Exception	
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:hash'] == Hash
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:object'] == Object
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:range'] == Range
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:regexp'] == Regexp
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:string'] == String
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:struct'] == Struct
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:sym'] == Symbol
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:symbol'] == Symbol
	assert YAML::tagged_classes['tag:ruby.yaml.org,2002:time'] == Time
	assert YAML::tagged_classes['tag:yaml.org,2002:binary'] == String
	assert YAML::tagged_classes['tag:yaml.org,2002:bool#no'] == FalseClass
	assert YAML::tagged_classes['tag:yaml.org,2002:bool#yes'] == TrueClass
	assert YAML::tagged_classes['tag:yaml.org,2002:float'] == BigDecimal
	assert YAML::tagged_classes['tag:yaml.org,2002:int'] == Integer
	assert YAML::tagged_classes['tag:yaml.org,2002:map'] == Hash
	assert YAML::tagged_classes['tag:yaml.org,2002:null'] == NilClass
	#assert YAML::tagged_classes['tag:yaml.org,2002:omap'] == YAML::Omap
	#assert YAML::tagged_classes['tag:yaml.org,2002:pairs'] == YAML::Pairs
	assert YAML::tagged_classes['tag:yaml.org,2002:seq'] == Array
	#assert YAML::tagged_classes['tag:yaml.org,2002:set'] == YAML::Set
	assert YAML::tagged_classes['tag:yaml.org,2002:str'] == String
	assert YAML::tagged_classes['tag:yaml.org,2002:timestamp'] == Time
	#assert YAML::tagged_classes['tag:yaml.org,2002:timestamp#ymd'] == Date	
end
  
  def test_tag_class 	 	  
	  YAML::tag_class('tag:foo@foo.org,2004:bar', Foo)
	  assert(YAML::tagged_classes['tag:foo@foo.org,2004:bar'] == Foo)
  end
  
  def test_tag_class_override
	  YAML::tag_class('tag:ruby.yaml.org,2002:symbol', Foo)
	  assert(YAML::tagged_classes['tag:ruby.yaml.org,2002:symbol'] == Foo)
	  YAML::tag_class('tag:ruby.yaml.org,2002:symbol', Symbol)
  end
  
  def test_tag_class_override_nils
	  YAML::tag_class('tag:foo@foo.org,2004:bar', nil)	  
	  assert(YAML::tagged_classes['tag:foo@foo.org,2004:bar'] == nil)	  
	  YAML::tag_class(nil, Foo)
	  assert(YAML::tagged_classes[nil] == Foo)	  
  end  
  
  class Coordinate
	yaml_as "tag:ruby.yaml.org,2002:Coordinate"
  end
  
  def test_yaml_as_tags_class
    assert(YAML::tagged_classes['tag:ruby.yaml.org,2002:Coordinate'] == Coordinate)
  end

  class Coordinate2
	yaml_as "tag:ruby.yaml.org,2002:Coordinate2"
	yaml_as "tag:ruby.yaml.org,2002:Coordinate22"
  end

  def test_yaml_as_double_tag
    assert(YAML::tagged_classes['tag:ruby.yaml.org,2002:Coordinate2'] == Coordinate2)
    assert(YAML::tagged_classes['tag:ruby.yaml.org,2002:Coordinate22'] == Coordinate2)	
  end

  def test_to_yaml 
	assert_equal "--- 1.0\n", BigDecimal.new('1.0').to_yaml 
	assert_equal "--- 100000.30020320320000000000001\n", BigDecimal.new('100000.30020320320000000000001').to_yaml 
	assert BigDecimal.new('123.45').to_s == YAML::load(BigDecimal.new('123.45').to_yaml).to_s
  end 

end

class Foo 
end	

class BigDecimal 
   yaml_as "tag:yaml.org,2002:float" 
   def to_yaml( opts = {} ) 
     YAML::quick_emit( nil, opts ) do |out| 
             # This emits the number without any scientific notation.  
             # I prefer it to using self.to_f.to_s, which would loose precision. 
             # 
             # Note that YAML allows that when reconsituting floats  
             # to native types, some precision may get lost.  
             # There is no full precision real YAML tag that I am aware of. 
             str = self.to_s 
             if str == "Infinity" 
                 str = ".Inf" 
             elsif str == "-Infinity" 
                 str = "-.Inf" 
             elsif str == "NaN" 
                 str = ".NaN" 
             end 
             out.scalar( "tag:yaml.org,2002:float", str, :plain ) 
         end 
   end 
 end 

