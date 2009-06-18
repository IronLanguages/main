require 'test/unit'
require 'yaml'

class Module      
      def const_missing name                
        $S
      end
end

  module YAML
    $S = Stream
    remove_const "Stream"
  end

class Load_stream_tests < Test::Unit::TestCase
  # def setup
  # end

  # def teardown
  # end

  def test_load_stream_from_string        
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
    
    
    s = YAML::load_stream(y)    
    assert(s.documents[0].size == 4)      
    assert(s.documents[0]['type'] == 'GET')      
    assert(s.documents[1]['url'] == '/toc.html')      
    assert(s.documents.size == 2)
  end
  
 def test_load_stream_from_file    
    y = File.open('yaml/yts_strangekeys.yml')
    s = YAML::load_stream(y)
    assert(s[0]['ruby'] == 0.4)              
    assert(s.documents.size == 3)    
  end     
  
  def test_stream_constant_lookup
  s = YAML::load_stream('--- one')  
  assert(s[0] == 'one')
  end

class MyStream < YAML::Stream
end

 def test_stream_options
  s = YAML::Stream.new
  assert(s.options.size == 0)
end

def test_stream_options_inherited
  s = MyStream.new
  assert(s.options.size == 0)
  end

def test_stream_adding_nil
  s = MyStream.new
  s.add(nil)
  assert(s.documents.size == 1)
  assert(s[0] == nil)
  end

def test_stream_edit_expanding
  s = MyStream.new
  s.edit(2, 'foo')
  assert(s.documents.size == 3)
  assert(s[2] == 'foo')
end

def test_stream_index_out_of_range
  s = MyStream.new
  assert(s[100] == nil)
end

def test_stream_assign_documents
  s = MyStream.new
  s.add("one")
  s.add("two")
  s.documents = ['three', 'four']
  assert(s.documents.size == 2)
  assert(s[0] == 'three')
  assert(s[1] == 'four')
end

def test_stream_assign_options
  s = YAML::Stream.new(:Indent => 4, :UseHeader => true )  
  s.options = {:Indent => 2}
  assert(s.options.size == 1)
  assert(s.options[:Indent] == 2)  
end

def test_stream_edit_reassign
  s = YAML::Stream.new()  
  s.add('one')
  s.add('two')
  s.edit(0, 'three')
  assert(s.documents.size == 2)
  assert(s[0] == 'three')
  assert(s[1] == 'two')
end

end

module YAML
    const_set :Stream, $S    
  end
  
  class Module
      def const_missing name
        #puts "missing #{name}"
        $S if name == 'Stream'
      end
    end
    
    class Module      
      def const_missing name                        	
	  puts "missing #{name}"	  
      end
    end