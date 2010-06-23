ft = File.read(File.dirname(__FILE__) + '/out')
disable = {}
ft.scan(/(...)\) Error:\ntest_spec \{(.*?)\} (\d\d\d) \[(.*?)\]\(\2\):\n(.*?)\n/) do |data|
  count, context, number, specify, error = *data
  disable[context] ||= {}
  disable[context][error] ||= []
  disable[context][error] << specify
end

ft.scan(/(...)\) Failure:\ntest_spec \{(.*?)\} (\d\d\d) \[(.*?)\]\(\2\)/) do |data|
  count, context, number, specify = *data
  disable[context] ||= {}
  disable[context][nil] ||= []
  disable[context][nil] << specify
end

disable.each do |context, errors|
  puts "disable_spec '#{context}',"
  errors.each do |error, specifies|
    puts "  ##{error[0..100]}#{'...' if error.size > 100}" if error
    specifies.each do |specify|
      puts "  \"#{specify}\"#{',' if specifies.last != specify || errors.keys.last != error}"
    end
  end
  puts
end