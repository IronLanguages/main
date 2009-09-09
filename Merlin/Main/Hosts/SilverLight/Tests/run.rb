begin
  start_time = Time.now
  puts "Start time: #{start_time}"

  ITERATIONS = 50
  TIMEOUT = 0.5

  # make sure dlr.js is present
  puts
  puts "Updating dlr.js"
  puts "---------------"
  load 'gen_dlrjs.rb'

  t = Thread.new do
    puts
    options = %W(debug release)
    type = ARGV.first if ARGV.first
    type = 'debug' unless options.include?(type)
    get_path = lambda do |type|
      "#{File.dirname(__FILE__)}/../../../Bin/Silverlight\ #{type.capitalize}/Chiron.exe"
    end
    unless File.exist? get_path[type]
      print "\"#{type}\" configuration not found, looking for more "
      (options - [type]).each do |t|
        if File.exist? get_path[t]
          type = t
          puts "[FOUND \"#{type}\"]"
          break
        end
        type = nil
      end
      if type.nil?
        $stderr.puts "[ERROR]"
        $stderr.puts "[ERROR] no valid build configuration found!"
        $stderr.puts "        Run \"bsd\" from a Dev.bat command prompt"
        exit(1)
      end
    end
    print "Starting web server with \"#{type}\" configuration "
    system "\"#{get_path[type]}\" /p:\"lib\" /w /d:\"#{File.dirname(__FILE__)}\" 2>&1>NUL"
  end

  ITERATIONS.times do |i|
    sleep TIMEOUT
    if `tasklist` =~ /Chiron/
      puts "[DONE]"
      break
    end
    if i == (ITERATIONS - 1)
      puts '[TIMEOUT]'
      $stderr.puts "[ERROR] web server startup timed out"
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
    results.has_key?(:failures) && results.has_key?(:errors) && 
    results[:failures] == 0 && results[:errors] == 0
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
    results[:all] == results[:pass]
  end

  def get_browser_name_from_constant(constant)
    constant.to_s.split('::').last.downcase
  end

  print "Loading dependencies "

  $tests = {'index' => 'bacon', 'test_dlrjs' => 'qunit', 'test_script-tags' => 'bacon'}

  results = {}

  require 'rubygems'
  print '.'
  require 'watir'
  print '.'
  require 'firewatir'
  puts '[DONE]'

  [FireWatir::Firefox, Watir::IE].each do |browser_type|
  
    puts
    browser_name = get_browser_name_from_constant(browser_type)
    print "Opening #{browser_name} "
    @browser = browser_type.new
    puts '[DONE]'
    puts '-' * (8 + browser_name.size + 7)

    $tests.each do |test, test_fx|

      print "> Running #{test}.html "

      @browser.goto("http://localhost:2060/#{test}.html")

      test_results = nil
      ITERATIONS.times do |i|
        sleep TIMEOUT
        test_results = eval("check_#{test_fx}_test_results")
        unless test_results.keys.empty?
          puts '[DONE]'
          break
        end
        if i == (ITERATIONS - 1)
          puts '[TIMEOUT]'
          @browser.close
          $stderr.puts "[ERROR]"
          $stderr.puts "[ERROR] waiting for results timed out"
        end
        print '.'
      end
      results[test] ||= {}
      results[test][get_browser_name_from_constant(browser_type)] = test_results
    end

    puts '-' * (8 + browser_name.size + 7)
    print "Closing #{browser_name} "
    @browser.close
    @browser = nil
    puts '[DONE]'
  end

rescue => e
  
  $stderr.puts
  $stderr.puts
  $stderr.puts "Exception raised"
  $stderr.puts "----------------"
  $stderr.puts e.inspect
  $stderr.puts e.backtrace
  exit(1)

ensure
  puts
  print "Stopping web server "
  `taskkill /IM Chiron.exe /F`
  puts "[DONE]"

  if @browser
    print "Closing browser "
    @browser.close
    @browser = nil
    puts '[DONE]'
  end

  def pass?(results)
    if results
      puts
      puts "Raw results"
      puts "-----------"
      puts results.inspect
      puts "-----------"
      results.each do |test, browsers|
        browsers.each do |browser, res|
          return false unless send("#{$tests[test]}_pass?", res)
        end
      end
    else
      return false
    end
    return true
  end

  puts
  puts "-" * 40
  end_time = Time.now
  puts "End time: #{end_time}"
  puts "Elapsed time: #{end_time - start_time} second(s)"
  puts "-" * 40

  unless pass?(results)
    puts
    puts "Failed!"
    exit(1)
  else
    puts
    puts "Success!"
  end
end
