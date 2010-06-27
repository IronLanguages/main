begin
  start_time = Time.now
  puts "Start time: #{start_time}"

  ITERATIONS = 10
  TIMEOUT = 0.5

  begin
    load 'test.config'
  rescue LoadError
    $stderr.puts
    $stderr.puts "[ERROR] no test.config found!"
    $stderr.puts "        Create a test.config file in the current directory"
    $stderr.puts "        this script is running out of, using this format:"
    $stderr.puts
    $stderr.puts "        $tests_dir = File.dirname(__FILE__) # directory where test files are located"
    $stderr.puts "        $tests = {"
    $stderr.puts "          'test1.html' => 'bacon', # test html file which runs bacon tests"
    $stderr.puts "          'test2.html' => 'qunit', # test html file which runs qunit tests"
    $stderr.puts "          # etc ..."
    $stderr.puts "        }"
    exit(1)
  end

  require 'uri'
  $uri = URI.parse("http://localhost:8000")

  def start_webserver
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
    chiron = "\"#{get_path[type]}\" /d:#{$tests_dir} /w:#{$uri.port}"
    require 'rubygems'
    require 'win32/process'
    require 'tempfile'
    f = File.open('./chiron.log', 'w')
    $p = Process.create 'app_name' => chiron, 'startup_info' => {'stdout' => f}
  end

  def is_webserver_running?
    # make sure process is running
    require 'rubygems'
    require 'win32/process'
    return false unless $p
    begin
      pid = Process.kill(0, $p.process_id).first
      return false if $p.process_id != pid
    rescue Process::Error
      return false
    end

    # and make sure a request works
    require 'net/http'
    begin
      req = Net::HTTP.start($uri.host, $uri.port){|http| http.get('/')}
      return false if req.code.to_i != 200
    rescue Errno::ECONNREFUSED
      return false
    end

    return true
  end

  def stop_webserver
    print "Stopping web server "
    if $p
      Process.kill(1, $p.process_id)
      Process.waitpid($p.process_id)
      puts "[DONE]"
    else
      puts "[WARNING] webserver not running"
    end
  end

  ITERATIONS.times do |i|
    start_webserver
    sleep 5
    if is_webserver_running?
      puts '[SUCCESS]'
      break
    elsif i == (ITERATIONS - 1)
      puts '[FAIL]'
      $stderr.puts '[FAIL] tried on many ports, aborting'
      exit(1)
    else
      puts '[TIMEOUT]'
      puts 'Trying on another port'
      $uri.port += 1
    end
  end

  def check_rspecstyle_test_results
    require 'nokogiri'
    begin
      doc = Nokogiri::HTML(@browser.html)
    rescue
      return {}
    end
 
    results_html = doc.css('#silverlightDlrReplResult1 span')[-2]
    return {} if results_html.nil?
    results_str = results_html.inner_html.gsub('&nbsp;', ' ')

    %W(tests assertions skips specifications requirements failures errors).inject({}) do |results, type|
      results_str.scan(/(.*?) #{type}/) do |m|
        results[type.to_sym] = m.first.split(/[ (]/).last.to_i
      end
      results
    end
  end

  def rspecstyle_pass?(results)
    results.keys.size > 0 &&
    results.has_key?(:failures) && results.has_key?(:errors) &&
    results[:failures] && results[:errors] &&
    results[:failures] == 0 && results[:errors] == 0
  end

  def check_qunit_test_results
    require 'nokogiri'
    begin
      doc = Nokogiri::HTML(@browser.html)
    rescue
      return {}
    end

    pass = doc.css('#testresult > .bad')
    all = doc.css('#testresult > .all')

    return {} if pass.nil? || all.nil?

    {:pass => pass.inner_html.to_i, :all => all.inner_html.to_i}
  end

  def qunit_pass?(results)
    results.keys.size > 0 &&
    results.has_key?(:all) && results.has_key?(:pass) &&
    results[:all] && results[:pass] &&
    results[:all] == results[:pass]
  end

  def check_minitest_test_results
    check_rspecstyle_test_results
  end

  def minitest_pass?(results)
    rspecstyle_pass? results
  end

  def check_bacon_test_results
    check_rspecstyle_test_results
  end

  def bacon_pass?(results)
    rspecstyle_pass? results
  end

  def get_browser_name_from_constant(constant)
    constant.to_s.split('::').last.downcase
  end

  print "Loading dependencies "

  results = {}

  require 'rubygems'
  print '.'
  require 'active_support'
  print '.'
  require 'watir'
  puts '[DONE]'

  [
    FireWatir::Firefox, 
    Watir::IE
  ].each do |browser_type|
 
    puts
    browser_name = get_browser_name_from_constant(browser_type)
    print "Opening #{browser_name} "
    @browser = browser_type.new
    puts '[DONE]'
    puts '-' * (8 + browser_name.size + 7)

    $tests.each do |test, test_fx|

      print "> Running #{test} "

      @browser.goto "#{$uri}/#{test}"

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

rescue Exception => e

  $stderr.puts
  $stderr.puts
  $stderr.puts "Exception raised"
  $stderr.puts "----------------"
  $stderr.puts e.inspect
  $stderr.puts e.backtrace
  exit(1)

ensure
  puts
  stop_webserver

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
    puts "Fail!"
    exit(1)
  else
    puts
    puts "Pass!"
  end
end
