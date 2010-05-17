cmd = "ir -e \"exit 1\""
if system cmd
  puts "Failed exit test"
  exit 1
else
  puts "OK"
  exit 0
end
