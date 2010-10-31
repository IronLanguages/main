describe :file_stat, :shared => true do
  before :each do
    @file = tmp('i_exist')
    touch(@file) { |f| f.write 'rubinius' }
  end

  after :each do
    rm_r @file
  end

  it "returns a File::Stat object if the given file exists" do
    st = File.send(@method, @file)
    st.class.should == File::Stat
  end

  it "should be able to use the instance methods" do
    begin
      file = File.new(@file)
      st = file.send(@method)
      
      st.file?.should == true
      st.zero?.should == false
      st.size.should == 8
      st.size?.should == 8
      (st.blksize.should > 0) if st.blksize
      st.atime.class.should == Time
      st.ctime.class.should == Time
      st.mtime.class.should == Time
    ensure
      file.close
    end
  end

  ruby_version_is "1.9" do
    it "accepts an object that has a #to_path method" do
      st = File.send(@method, mock_to_path(@file))
    end
  end

  it "raises an Errno::ENOENT if the file does not exist" do
    lambda {
      File.send(@method, "fake_file")
    }.should raise_error(Errno::ENOENT)
  end
end
