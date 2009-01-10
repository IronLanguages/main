# calls with blocks -> make arguments and body lambdas?

def repeat(cond)
  yield 
  retry if not cond
end

j = 0;
repeat (j >= 10) do
 j += 1
 puts j
end 




