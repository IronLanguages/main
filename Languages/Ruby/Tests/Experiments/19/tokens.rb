def try_eval s
  eval s
rescue Exception
  p $!
else
  puts 'ok'
end

try_eval <<EOT
foo
  .bar
EOT

try_eval <<EOT
def !@
end
EOT

