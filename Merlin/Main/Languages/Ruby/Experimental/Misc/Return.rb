def p(&p)
    p
end

def y
    yield
end

def goo
    $x = p {
      puts 'B'
      return
    }
end

def zoo
   y &$x
end

goo
zoo