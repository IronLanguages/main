def y a
  yield *a
end

def i *a
  puts '---'
  a.each { |x| puts x.inspect }
end

y [1,2] do |x1,x2,x3,*z|  
  i x1,x2,x3,z               # 1,2,nil,[]
end

x1,x2,x3,*z = 1,2
i x1,x2,x3,z

##############

y [1,2] do |x|
  i x
end

x = 1,2
i x

##############



