def foo
  $x = $x + 1 
  
  x = $x
  if $d
    y { puts x }
  end
end

def y &p
  p[]
  $p = p
end

$x = 0
$d = true

foo
$p[]

$d = false
foo
$p[]




