require 'Win32API'

buffer = '\0' * 1024
ENV["FooBar"] = 'hello world'
len = Win32API.new('kernel32', 'GetEnvironmentVariable', 'P _ P _ L', 'L').call('FooBar', buffer, 1024)
p buffer[0...len]

ENV["FooBar"] = 'xxx'
p Win32API.new('kernel32', 'GetEnvironmentVariable', 'P _ P _ L', 'X').call('FooBar', buffer, 1024)
p buffer[0..3]

['GetVersionExA', 'GetVersionEx'].each { | name| 
  buffer = [ 148, 0, 0, 0, 0 ].pack('V5') + "\0" * 128
  Win32API.new('kernel32.dll', name, 'P', 'L').call(buffer)
  p buffer.unpack('V5')
}

puts '---'

handle = Win32API.new( "kernel32", "GetStdHandle", ['L'], 'L' ).call(STD_OUTPUT_HANDLE)

w = Win32API.new("kernel32", "WriteFile", 'LPLPP', 'L' )

# 0 is marshalled as nil if the argument type is P
buffer = "Something to write -> "
bytesWritten = "\0" * 8
p w.call(handle, buffer, buffer.size, bytesWritten, 0)
p bytesWritten.unpack('L') # 22

# nil is not marshalled as 0 if the argument type is integer
buffer = "Something to write -> "
bytesWritten = "\0" * 8
p w.call(handle, buffer, nil, bytesWritten, 0) rescue p $!
p bytesWritten.unpack('L') # 22

# to_str conversion
class C
  def initialize
    @data = "some string to write => "
  end
 
  def to_str
    @data
  end
  
  def to_int
    @data.size
  end
end
c = C.new

bytesWritten = "\0" * 8
p w.call(handle, c, c, bytesWritten, 0)
p bytesWritten.unpack('L') # 22
