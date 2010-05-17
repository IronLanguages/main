File.delete("x.txt")
File.open("x.txt", File::CREAT | File::WRONLY ) { |f| f.write('hello') }

puts 'WRONLY | APPEND'

File.open("x.txt", File::WRONLY | File::APPEND) { |f| 
  f.pos = 1  # doesn't work MRI silently ignores
  p f.pos
  f.write('z') 
  p f.pos
}

puts 'RDONLY | APPEND'

# APPEND ignored
File.open("x.txt", File::RDONLY | File::APPEND) { |f| 
  p f.pos
  p f.read(1)
  p f.pos
  f.pos = 4
  p f.read(2)
  p f.pos
}

puts 'RDWR | APPEND'

File.open("x.txt", File::RDWR | File::APPEND) { |f| 
  p f.pos
  p f.read(1)
  p f.pos
  p f.write('q')
  p f.pos  
  f.pos = 1
  p f.write('q')
  p f.pos  
}

puts 'CREAT'

# exists -> nop
File.open("x.txt", File::CREAT) { |f|
  p f.read
}

# doesn't exist -> create
File.open("z.txt", File::CREAT) { |f|
  # cannot delete opened file
  File.delete("z.txt") rescue p $!
}

File.delete("z.txt")

# no CREAT -> error
File.open("z.txt", File::APPEND) rescue p $!
File.open("z.txt", File::WRONLY) rescue p $!
File.open("z.txt", File::EXCL) rescue p $!
File.open("z.txt", File::TRUNC) rescue p $!

puts 'EXCL'

# no CREAT -> nop
File.open("x.txt", File::EXCL) { |f|
  p f.read
}

# CREAT -> raise error if exists
File.open("x.txt", File::CREAT | File::EXCL)  rescue p $!
File.open("z.txt", File::CREAT | File::EXCL) { }
File.delete("z.txt");

puts 'TRUNC'
File.open("x.txt", File::WRONLY | File::TRUNC) { |f| f.write('x'); p f.pos }
File.open("x.txt", File::RDWR | File::TRUNC) { |f| p f.read }
File.open("x.txt", File::RDONLY | File::TRUNC) rescue p $!
File.open("x.txt", File::APPEND | File::TRUNC) rescue p $!
File.open("x.txt", File::TRUNC) rescue p $!

