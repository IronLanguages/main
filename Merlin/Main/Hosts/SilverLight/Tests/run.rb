begin
  require 'rubygems'
  require 'watir'
  require 'firewatir'

  ITERATIONS = 50
  TIMEOUT = 0.5

  t = Thread.new do
    print 'starting web server '
    system '"..\..\..\Bin\Silverlight Release\Chiron.exe" /w 2>&1>NUL'
  end

  ITERATIONS.times do |i|
    sleep TIMEOUT
    if `tasklist` =~ /Chiron/
      puts
      break
    end
    if i == (ITERATIONS - 1)
      puts 'timeout!'
      $stderr.puts "webserver startup timed out"
      exit(1)
    end
    print '.'
  end

  def check_test_results
    types = %W(specifications requirements failures errors)
    results = {}
    @browser.html.scan(/>(.*?)&nbsp;#{types[0]}&nbsp;\((.*?)&nbsp;#{types[1]}\),&nbsp;(.*?)&nbsp;#{types[2]},&nbsp;(.*?)&nbsp;#{types[3]}/) do |m|
      m.each_with_index do |n, i|
        n = n.split('>').last
        results[types[i].to_sym] = n.to_i
      end
    end
    results
  end

  def get_browser_name_from_constant(constant)
    constant.to_s.split('::').last.downcase
  end

  results = [FireWatir::Firefox, Watir::IE].inject({}) do |results, browser_type|
    @browser = browser_type.start("http://localhost:2060/index.html")
    print "waiting for test result from #{get_browser_name_from_constant(browser_type)} "

    test_results = nil
    ITERATIONS.times do |i|
      sleep TIMEOUT
      test_results = check_test_results
      unless test_results.keys.empty?
        puts
        @browser.close
        break
      end
      if i == (ITERATIONS - 1)
        puts 'timeout!'
        @browser.close
        $stderr.puts "waiting for results timed out"
      end
      print '.'
    end
    results[get_browser_name_from_constant(browser_type)] = test_results
    results
  end


ensure
  `taskkill /IM Chiron.exe /F`
  @browser.close if @browser
  puts results.inspect if results
end
