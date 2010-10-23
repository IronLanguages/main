require 'test/unit'
require 'yaml'

class Load_tests < Test::Unit::TestCase
  # def setup
  # end

  # def teardown
  # end

  def test_load_from_string        
    foo = YAML::load('answer: 42')
    assert(foo.size == 1)
    assert(foo['answer'] == 42)    
  end
  
  def test_repetitive_loads
    test_load_from_string
    test_load_from_string
  end
  
  def test_load_from_file
    foo = YAML::load(File.open('yaml/rails_database.yml'))    
    assert(foo['production']['timeout'] == 5000)    
  end
  
  def test_load_from_file_with_block
    File.open('yaml/rails_database.yml') do |y| 
      foo = YAML::load(y)    
      assert(foo['production']['timeout'] == 5000)    
    end
  end
  
  def test_load_symbol
	assert(YAML.load("--- :foo") == :foo)
  end
   
   def test_load_mapping_with_symbols	   
	$t = {:one => "one val", :two => { :nested_first => "nested val" } }
	$y = YAML::dump $t
	$r = YAML::load $y
	assert($r == $t)
	assert($r[:one] == "one val")   
  end
end
