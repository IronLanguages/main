require 'test/unit'
require 'yaml'

class Load_documents_tests < Test::Unit::TestCase
  # def setup
  # end

  # def teardown
  # end

  def test_load_documents_from_string        
    y = <<-EOF
---
at: 2001-08-12 09:25:00.00 Z
type: GET
HTTP: '1.0'
url: '/index.html'
---
at: 2001-08-12 09:25:10.00 Z
type: GET
HTTP: '1.0'
url: '/toc.html'
    EOF
    
    counter = 0
    YAML::load_documents(y) do |doc|
      assert(doc.size == 4)
      if (counter == 0)        
        assert(doc['type'] == 'GET')
      else
        assert(doc['url'] == '/toc.html')
      end
      counter += 1
    end
    assert(counter == 2)
  end
  
  def test_load_documents_from_file
    counter = 0
    y = File.open('yaml/yts_strangekeys.yml')
    YAML::load_documents(y) do |doc|      
      if (counter == 0)        
        assert(doc['ruby'] == 0.4)      
      end
      counter += 1
    end
    assert(counter == 3)    
  end     
end
