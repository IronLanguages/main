describe :file_identical, :shared => true do
  platform_is_not :windows do
    before :each do
      @file1 = tmp('file_identical.txt')
      @file2 = tmp('file_identical2.txt')
      @link  = tmp('file_identical.lnk')


      touch(@file1) { |f| f.puts "file1" }
      touch(@file2) { |f| f.puts "file2" }

      rm_r @link
      File.link(@file1, @link)
    end

    after :each do
      rm_r @link, @file1, @file2
    end
  end

  platform_is :windows do
    before :each do
      @file1 = tmp('file_identical.txt')
      @file2 = tmp('file_identical2.txt')


      touch(@file1) { |f| f.puts "file1" }
      touch(@file2) { |f| f.puts "file2" }
    end

    after :each do
      rm_r @file1, @file2
    end
  end
  
  it "return true if they are identical" do
    @object.send(@method, @file1, @file1).should == true
    @object.send(@method, @file1, @file2).should == false
  end
  
  it "return false if one of the file doesn't exist" do
    @object.send(@method, @file1, "file that doesn't exist").should == false
    @object.send(@method, "file that doesn't exist", @file2).should == false
    @object.send(@method, "file that doesn't exist", "file that doesn't exist").should == false
  end

  platform_is_not :windows do
    it "returns true if one is a symlink and they are identical" do
      @object.send(@method, @file1, @link).should == true
    end
  end

  ruby_version_is "1.9" do
    platform_is_not :windows do
      it "accepts an object that has a #to_path method" do
        @object.send(@method, mock_to_path(@file1), mock_to_path(@link)).should == true
      end
    end

    platform_is :windows do
      it "accepts an object that has a #to_path method" do
        @object.send(@method, mock_to_path(@file1), mock_to_path(@file1)).should == true
      end
    end
  end


  it "raises an ArgumentError if not passed two arguments" do
    lambda { @object.send(@method, @file1, @file2, @file3) }.should raise_error(ArgumentError)
    lambda { @object.send(@method, @file1) }.should raise_error(ArgumentError)
  end

  it "raises a TypeError if not passed String types" do
    lambda { @object.send(@method, 1,1) }.should raise_error(TypeError)
  end

  it "returns true if both named files are identical" do
    @object.send(@method, @file1, @file1).should be_true
    @object.send(@method, @file1, @file2).should be_false
  end
end
