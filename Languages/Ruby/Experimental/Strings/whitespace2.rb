require 'mscorlib'
0xffff.times { |i| 
  c = System::Convert.ToChar(i)
  if System::Char.IsWhiteSpace(c) 
    puts System::Text::Encoding.UTF8.GetBytes("foo".to_clr_string)
  end
}


