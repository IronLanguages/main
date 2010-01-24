class C
  def write(x)
    puts "[#{x}]"
  end
end

$stderr = C.new

END {
  raise 'e1'
}
END {
  puts 'e2'
}
END {
  raise 'e3'
}
END {
  puts 'e4'
}
END {
  raise 'e5'
}


