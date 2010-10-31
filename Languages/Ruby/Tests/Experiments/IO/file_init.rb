class C
  def respond_to? name
    puts name
    false
  end
end

class File2 < File
  def initialize
    super(C.new) rescue p $!
  end
end

puts 'uninitialized'
f2 = File2.new
f2.path rescue p $!
f2.chmod(1) rescue p $!

puts 'w/o path'
f3 = File.new(File.new("x.txt", "w").to_i)
p f3.path
f3.chmod(1) rescue p $!
p f3.atime rescue p $!
p f3.ctime rescue p $!
p f3.mtime rescue p $!

puts 'closed'
f3.close
p f3.path rescue p $!
p f3.chmod(1) rescue p $!
p f3.atime rescue p $!
p f3.ctime rescue p $!
p f3.mtime rescue p $!

puts 'FileTest'
File.blockdev?(C.new) rescue p $!
FileTest.blockdev?(C.new) rescue p $!