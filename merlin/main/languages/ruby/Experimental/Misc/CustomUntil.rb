def do_until(cond)
  if cond then return end
  yield
  retry
end

i = 0
do_until(i > 4) do
  puts i
  i = i + 1
end