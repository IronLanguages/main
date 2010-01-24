require File.dirname(__FILE__) + '/../../../spec_helper'

describe "File::Stat#inspect" do

  before :each do
    @file = tmp('i_exist')
    touch(@file) { |f| f.write "rubinius" }
  end

  after :each do
    rm_r @file
  end
  
  it "produces a nicely formatted description of a File::Stat object" do
    st = File.stat(@file)  
    st.inspect.should =~ /
      \#<File::Stat \s
      dev=0x#{st.dev.to_s(16)}, \s
      ino=#{st.ino}, \s
      mode=#{sprintf("%07d", st.mode.to_s(8).to_i)}, \s
      nlink=#{st.nlink}, \s
      uid=#{st.uid}, \s
      gid=#{st.gid}, \s
      rdev=0x#{st.rdev.to_s(16)}, \s
      size=#{st.size}, \s
      blksize=(#{st.blksize}|nil), \s
      blocks=(#{st.blocks}|nil), \s
      atime=#{Regexp.escape(st.atime.to_s)}, \s
      mtime=#{Regexp.escape(st.mtime.to_s)}, \s
      ctime=#{Regexp.escape(st.ctime.to_s)}>
      /xm
  end


end
