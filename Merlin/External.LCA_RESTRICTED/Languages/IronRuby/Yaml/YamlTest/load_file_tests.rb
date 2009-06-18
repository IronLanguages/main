require 'test/unit'
require 'yaml'

class Load_file_tests < Test::Unit::TestCase
  # def setup
  # end

  # def teardown
  # end

  def test_load_file
    foo = YAML::load_file('yaml/rails_database.yml')    
    assert(foo['production']['timeout'] == 5000)    
  end
   
end
