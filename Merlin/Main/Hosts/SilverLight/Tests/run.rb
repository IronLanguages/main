begin
  require 'rubygems'
  require 'watir'
  require 'firewatir'

  ITERATIONS = 50
  TIMEOUT = 0.5

  # make sure dlr.js is present
  puts "making sure dlr.js is up-to-date"
  dlrjspath = File.dirname(__FILE__) + '/../Scripts/dlr.js'
  gendlrjspath = File.dirname(__FILE__) + '/../Scripts/generate_dlrjs.rb'
  FileUtils.rm dlrjspath if File.exist?(dlrjspath)
  load gendlrjspath
  mydlrjspath = File.dirname(__FILE__) + '/dlr.js'
  require 'fileutils'
  FileUtils.rm mydlrjspath if File.exist?(mydlrjspath)
  FileUtils.cp dlrjspath, mydlrjspath

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

  results.each do |browser, res|
    if res[:failures] > 0 || res[:errors] > 0
      puts "Failed!"
      exit(1)
    end
  end

  puts "Success!"
  exit(0)
end
