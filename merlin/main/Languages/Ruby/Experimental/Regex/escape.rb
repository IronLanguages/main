(0..255).each { |o|
  c = o.chr
  
  if Regexp.escape(c).length > 1 then     
     p c
  end
}

