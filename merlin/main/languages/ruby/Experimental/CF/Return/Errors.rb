def foo
  $p = Proc.new { return }
end

def y
  yield
end

foo
y &$p
