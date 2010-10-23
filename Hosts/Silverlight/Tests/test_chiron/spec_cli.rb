require 'helper'
load_all

include ChironSpecHelper

describe 'Chiron command-line interface' do

  describe '/help' do
    it 'should print help' do
      chiron.should =~ /Chiron - Silverlight Dynamic Language Development Utility/
    end
  end

  describe '/webserver' do
    it 'should start the webserver on port 2060' do
      c = chiron :webserver do |c|
        res = http_get('http://localhost:2060', '/')
        res.code.should == '200'
        res.body.should =~ /test.html/
        res.body.should =~ /Chiron\/1.0.0.0/
      end
      c.split("\n")[1].should == "Chiron serving '#{$DIR.gsub('/', '\\')}' as http://localhost:2060/"
    end
    
    it 'should start the webserver on a custom port' do
      c = chiron :webserver => 2067 do |c|
        res = http_get('http://localhost:2067', '/')
        res.code.should == '200'
        res.body.should =~ /test.html/
        res.body.should =~ /Chiron\/1.0.0.0/
      end
      c.split("\n")[1].should == "Chiron serving '#{$DIR.gsub('/', '\\')}' as http://localhost:2067/"
    end
  end

end