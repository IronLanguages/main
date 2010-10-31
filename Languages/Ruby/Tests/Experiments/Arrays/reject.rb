x = [1,2]
q = x.reject! { |z| if z==1 then true; else break x end }
p x,q

puts '--'


x = [1,2]
q = x.reject! { |z| 
  p z
  if z == 1 then 
     x << 3
     x << 4
     x << 5
     true
  else 
     break x; 
  end
}
p x,q

x = [1,2]
q = x.reject! { |z| 
  if z == 1 then 
     x << 3
     true
  end
}
p x,q

x = [1,2]
q = x.reject! { |z| false; break 1; }
p x,q

x = [1,2]
q = x.reject! { |z| false; }
p x,q