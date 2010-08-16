def foo *args
  bar(x = yield(1,2,3) do
    
  end)
end

def bar *x
end

foo {}

???