puts 'mode b'

File.open("Files/cr_lf_eolns.txt", "rb") { |f|
  6.times { p f.getc() }
}

puts
puts 'mode t'

File.open("Files/cr_lf_eolns.txt", "r") { |f|
  6.times { p f.getc() }
}