F = false
T = true
x = X = '!'

B = [F,F,T,x,T,x,T,x,x,F,F,T,x]
E = [x,x,x,T,x,T,x,F,T,x,x,x,T]
       
def b
  r = B[$j]
  puts "#{$j}: b -> #{r.inspect}"
  
  $j += 1
  
  $continue = !r.nil?  
  r == X ? raise : r  
end

def e
  r = E[$j]
  puts "#{$j}: e -> #{r.inspect}"
  
  $j += 1
  
  $continue = !r.nil?  
  r == X ? raise : r  
end

$j = 0
$continue = true
while $continue
  if b..e 
    puts "#{$j}: TRUE" 
  else
    puts "#{$j}: FALSE" 
  end
end