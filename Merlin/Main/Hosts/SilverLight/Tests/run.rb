begin
  ITERATIONS = 50
  TIMEOUT = 0.5

  # make sure dlr.js is present
  puts "making sure dlr.js is up-to-date"
  load 'gen_dlrjs.rb'

  t = Thread.new do
    options = %W(debug release)
    type = ARGV.first if ARGV.first
    type = 'debug' unless options.include?(type)
    get_path = lambda do |type|
      "#{File.dirname(__FILE__)}/../../../Bin/Silverlight\ #{type.capitalize}/Chiron.exe"
    end
    unless File.exist? get_path[type]
      puts "#{type} configuration not found, looking for more ..."
      (options - [type]).each do |t|
        if File.exist? get_path[t]
          type = t
          break
        end
        type = nil
      end
      if type.nil?
        puts "No valid build configuration found, exiting"
        exit(1)
      end
    end
    print "starting web server with #{type} configuration "
    system "\"#{get_path[type]}\" /w /d:\"#{File.dirname(__FILE__)}\" 2>&1>NUL"
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

  def check_bacon_test_results
    require 'nokogiri'
    doc = Nokogiri::HTML(@browser.html)
    
    results_html = doc.css('#silverlightDlrReplResult1 span')[-2]
    return {} if results_html.nil?
    results_str = results_html.inner_html.gsub('&nbsp;', ' ')
    
    %W(specifications requirements failures errors).inject({}) do |results, type|
      results_str.scan(/(.*?) #{type}/) do |m|
        results[type.to_sym] = m.first.split(/[ (]/).last.to_i
      end
      results
    end
  end

  def bacon_pass?(results)
    results[:failures] > 0 || results[:errors] > 0
  end

  def check_qunit_test_results
    require 'nokogiri'
    doc = Nokogiri::HTML(@browser.html)
    
    pass = doc.css('#testresult > .bad')
    all = doc.css('#testresult > .all')

    return {} if pass.nil? || all.nil?
    
    {:pass => pass.inner_html.to_i, :all => all.inner_html.to_i}
  end

  def qunit_pass?(results)
    results[:all] > results[:pass]
  end

  def get_browser_name_from_constant(constant)
    constant.to_s.split('::').last.downcase
  end

  $tests = {'index' => 'bacon', 'test_dlrjs' => 'qunit'}

  results = {}
  $tests.each do |test, test_fx|
    require 'rubygems'
    require 'watir'
    require 'firewatir'
    [FireWatir::Firefox, Watir::IE].each do |browser_type|
      @browser = browser_type.start("http://localhost:2060/#{test}.html")
      print "waiting for test results from #{test}.html in #{get_browser_name_from_constant(browser_type)} "

      test_results = nil
      ITERATIONS.times do |i|
        sleep TIMEOUT
        test_results = eval("check_#{test_fx}_test_results")
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
      results[test] ||= {}
      results[test][get_browser_name_from_constant(browser_type)] = test_results
    end
  end

rescue => e

  puts e.inspect
  exit(1)

ensure
  `taskkill /IM Chiron.exe /F`
  @browser.close if @browser
  if results
    puts results.inspect
    results.each do |test, browsers|
      browsers.each do |browser, res|
        if send("#{$tests[test]}_pass?", res)
          puts "Failed!"
          exit(1)
        end
      end
    end
  else
    puts "Failed!"
    exit(1)
  end

  puts "Success!"
  exit(0)
end
