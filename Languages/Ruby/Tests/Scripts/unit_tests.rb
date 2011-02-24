def banner(msg, size=70, char="=")
  border = char * size
  puts border, msg, border
end

flags = ["", "/partial", "/noadaptive", "/partial /noadaptive", "/sync0", "/sync1", "/debug", "/partial /debug"]

if ENV['DLR_VM'] && ENV['DLR_VM'].include?("mono")
   exclude = "/exclude"
   # critical (https://bugzilla.novell.com/show_bug.cgi?id=6698080):
   exclude += " InstanceVariables3 Scenario_RubyExceptions12 Scenario_RubyExceptions12A Scenario_RubyExceptions14 Scenario_RubyExceptions15 Scenario_RubyExceptions16 Scenario_RubyRescueStatement1 ExceptionArg1 ExceptionArg2 RescueSplat1 RescueSplat2 RescueSplat3 RescueSplat4 RescueSplat5 Backtrace7"
   
   # http://bugzilla.novell.com/show_bug.cgi?id=669813
   exclude += " RegexTransform1"
   
   # http://bugzilla.novell.com/show_bug.cgi?id=643371
   exclude += " Scenario_RubyExceptions7 Backtrace2 Backtrace3 Backtrace4 Backtrace6"
   
   # other failures:
   exclude += " MutableString_GetHashCode MutableString_ValidEncoding1 Interpreter3 Encoding_Host2 ClrExtensionMethods0 Dlr_RubyObjects"

   flags = ["/verbose"]
end

ENV['DLR_BIN'] ||= File.join(ENV['DLR_ROOT'], "bin/Debug")

flags.each do |flag|  
  exe_path = File.join(ENV['DLR_BIN'], "IronRuby.Tests.exe")

  cmd = "#{ENV['DLR_VM']} \"#{exe_path}\" #{flag} #{exclude}"
  banner cmd
  exit 1 unless system cmd
end

puts "PASSED"
