require 'test/unit'
require 'yaml'

class Load_tests < Test::Unit::TestCase
  # def setup
  # end

  # def teardown
  # end

  def test_dump_string_to_string	  
    s  = YAML.dump('one')     
    assert(s == "--- one\n")
  end
   
  def test_dump_array	  
	s = YAML.dump( ['badger', 'elephant', 'tiger'])	
	expected = "--- \n- badger\n- elephant\n- tiger\n"
	assert(s == expected)	    
end

def test_dump_mapping  
	s = YAML.dump( {'one' => 1} )	
	expected = "--- \none: 1\n"
	assert(s == expected)	    
end

def test_dump_symbol
	s = YAML.dump( :foo )	
	expected = "--- :foo\n"
	assert(s == expected)	    
end

def test_number_to_yaml	
	s = 1.to_yaml	
	expected = "--- 1\n"
	assert(s == expected)	    
end

def test_dump_stream
	s = YAML.dump_stream(0, [1,2,], {'foo'=>'bar'})
	expected = "--- 0\n--- \n- 1\n- 2\n--- \nfoo: bar\n"
	assert(s == expected)	    
end


end
