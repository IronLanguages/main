class C
  def [](*args)
    puts 'read'
    p args
    0
  end
  
  def []=(*args)
    puts 'write'
    p args
    nil
  end
end

def t
  puts 't'
  C.new
end

def k
  puts 'k'
  0
end

def l
  puts 'l'
  0
end

t[k,l] += 1
