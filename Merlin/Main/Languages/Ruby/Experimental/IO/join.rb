p File.join([])
p File.join([], [])
p File.join([[], []])
p File.join("a", []) 

puts '---'

p File.join("")
p File.join("", "")
p File.join(["", ""])
p File.join("a", "") 

puts '---'

p File.join("", "")  
p File.join("bin")  
p File.join("bin", "")  
p File.join("bin/")     
p File.join("bin/", "") 
p File.join("bin", "/") 
p File.join("bin/", "/")
    
p File.join("x", nil) rescue p $!
p File.join()
p File.join("x")
p File.join("x", "y")
p File.join("x", ["y", [[[["z"]]]]])

a = ["y", "z"]
a[1] = ['a', ['b', a]]
p File.join("x", a)

class C
  def respond_to? name
    puts "? #{name}"
    true
  end

  def to_ary
    ['a','b']
  end
  
  def to_str
    'hello'
  end
end

p File.join([C.new])
p File.join("a/", "b")
p File.join("a/", "/b")
p File.join("a/", "\\b")
p File.join("a\\", "b")
p File.join("a\\", "/b")
p File.join("a\\", "\\b")
p File.join("a///", "///b")
p File.join("a/\\/", "/\\\\\\\/b")
p File.join("//", "/\\/", "/\\\\\\\/b")
p File.join("", "", "/\\\\\\\/b")
p File.join("a/\\\\/", "b")

puts '----'

p File.join("usr",   "", "bin") 
p File.join("usr/",  "", "bin") 
p File.join("usr",   "", "/bin")
p File.join("usr/",  "", "/bin")
    
p File.join("", "b")
p File.join([""], "b")
p File.join([[""],[""]], "b")
p File.join("a", "b")

