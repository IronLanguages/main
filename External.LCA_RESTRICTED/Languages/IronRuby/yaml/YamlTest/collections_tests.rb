require 'test/unit'
require 'yaml'

class Collections_tests < Test::Unit::TestCase
  # def setup
  # end

  # def teardown
  # end

  #System::Console.ReadLine()

  def test_load_mapping_from_string    
    y = <<-EOF
    dog: canine
    cat: feline
    badger: malign
    EOF
    foo = YAML::load(y)
    assert(foo.size == 3)
    assert(foo['dog'] == 'canine')
    assert(foo['cat'] == 'feline')
    assert(foo['badger'] == 'malign')
  end
  
  def test_load_nested_mapping_from_string    
    y = <<-EOF
    Joey:
      age: 22
      sex: M
    Laura:
      age: 24
      sex: F
    EOF
    foo = YAML::load(y)
    assert(foo.size == 2)
    assert(foo['Joey']['age'] == 22)
    assert(foo['Joey']['sex'] == 'M')    
    assert(foo['Laura']['age'] == 24)
    assert(foo['Laura']['sex'] == 'F')    
  end
  
  def test_load_list_from_string
    y = <<-EOF
    - dogs
    - cats
    - badgers
    EOF
    foo = YAML::load(y)
    assert(foo[0] == 'dogs')
    assert(foo[1] == 'cats')
    assert(foo[2] == 'badgers')
    assert(foo.size == 3)    
  end
  
  def test_load_nested_list_from_string
    y = <<-EOF
    - 
      - pineapple
      - coconut
    -
      - umbrella
      - raincoat
    EOF
    foo = YAML::load(y)    
    assert(foo[0][0] == 'pineapple')
    assert(foo[0][1] == 'coconut')
    assert(foo[1][0] == 'umbrella')
    assert(foo[1][1] == 'raincoat')
    #assert(foo.size == 2)
    #assert(foo[0].size == 2)
    #assert(foo[1].size == 2)
  end
  
end
