def banner(msg, size=70, char="=")
  border = char * size
  puts border, msg, border
end

flags = ["", "/partial", "/noadaptive", "/partial /noadaptive", "/sync0", "/sync1", "/debug", "/partial /debug"]

if ENV['DLR_VM'] && ENV['DLR_VM'].include?("mono")
   exclude = "/exclude"
   # critical:
   exclude += " ClrGenerics1 ClrToString1 ClrTypeVariance1 Backtrace7"
   # failures:
   exclude += " Interpreter3 MutableString_Reverse Encoding_Host2 Dir1 RegexEncoding2 RegexEncoding2 ClrGenerics3 Scenario_RubyExceptions7 Backtrace1 Backtrace2 Backtrace3 Backtrace4 Backtrace6 Inspect1 RubyHosting5 ClrVisibility1"
end

ENV['DLR_BIN'] ||= File.join(ENV['DLR_ROOT'], "bin/Debug")

flags.each do |flag|  
  exe_path = File.join(ENV['DLR_BIN'], "IronRuby.Tests.exe")

  cmd = "#{ENV['DLR_VM']} \"#{exe_path}\" #{flag} #{exclude}"
  banner cmd
  exit 1 unless system cmd
end

puts "PASSED"
