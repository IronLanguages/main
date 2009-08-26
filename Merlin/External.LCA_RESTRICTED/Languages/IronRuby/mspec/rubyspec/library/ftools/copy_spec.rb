require File.dirname(__FILE__) + '/../../spec_helper'

ruby_version_is ""..."1.9" do
  require 'ftools'

  describe "File.copy" do
    before(:each) do
      @src = tmp("copy_test")
      @dest = tmp("copy_test_dest")
      File.open(@src, "w") {|f| f.puts "hello ruby"}
      File.chmod(0777, @src)
    end
    
    after(:each) do
      File.unlink @src
      File.unlink @dest rescue nil
    end
    
    it "copies the file at 1st arg to the file at 2nd arg" do
      File.copy @src, @dest
      fd = File.open @dest
      data = fd.read
      data.should == "hello ruby\n"
      fd.close
    end

    it "copies the file mode to the dest file" do
      File.copy @src, @dest 
      omode = File.stat(@src).mode
      mode = File.stat(@dest).mode
      
      omode.should == mode
    end
  end
end
