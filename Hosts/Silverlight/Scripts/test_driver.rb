require 'uri'

$:.unshift File.dirname(__FILE__)

require 'test_driver/browsers'
require 'test_driver/chiron'
require 'test_driver/config'
require 'test_driver/logger'
require 'test_driver/verifier'

class TestDriver

  TOTAL_SECONDS = 120
  TIMEOUT = 0.5
  ITERATIONS = (TOTAL_SECONDS / TIMEOUT).to_i

  class << self
    include TestLogger

    def run(options)
      info "Starting Silverlight test driver"
      info "Options: #{options.inspect}"
      @completed = false
      at_exit do
        cleanup
        report_results
      end
      begin
        benchmark do
          config = load_config(options)
          start_verifier
          start_chiron
          config.browsers.each do |browser_type|
            run_tests_in_browser(config.tests, browser_type)
          end
          @completed = true
        end
      rescue => e
        if log.debug?
          fatal e
        else
          fatal e.message
        end
        exit 1
      end
    end

    def load_config(options)
      if options
        TestConfig.load options
      else
        TestConfig.current
      end
    end

    def start_verifier
      @verifier_uri = URI.parse("http://localhost:9090")
      @verifier = Thread.new do
        TestVerifier.run! :host => @verifier_uri.host, :port => @verifier_uri.port,
          :root => File.dirname(__FILE__), :public => '.', :static => true
      end
    end

    def start_chiron
      @chiron_uri = URI.parse("http://localhost:8000")
      @chiron = ChironHelper.new @chiron_uri
      @chiron.start
    end
  
    def run_tests_in_browser(tests, browser_type)
      TestVerifier.results ||= {}
      TestVerifier.results[browser_type] ||= {}
      tests.each do |htmlfile, expected_num|
        run_test(htmlfile, expected_num, browser_type)
      end
    end
  
    def run_test(htmlfile, expected_num, browser_type)
      results = TestVerifier.results[browser_type]
      uri = "#{@chiron_uri}/#{htmlfile}"
      @browser = browser_type.new
      
      unless @browser.supported?
        warn "Skipping #{uri}, #{@browser.name} not supported."
        return
      else
      
        @browser.start uri

        debug "Waiting for test results from #{uri}"
     
        success = false
        ITERATIONS.times do |i|
          sleep TIMEOUT
          debug '... waiting' if i % 20 == 0
    
          if successful_test_results? results[uri], expected_num
            on_success results, uri
            success = true
            break
          end
        end
     
        unless success
          on_timeout results, uri, expected_num
        end
     
        @browser.stop
      end
      
      @browser = nil
    end
  
    def on_success(results, uri)
      output = "Test output:\n"
      results[uri].each do |r|
        output << '=' * 79
        output << "\n"
        output << r['output']
        output << "\n"
        output << '=' * 79
        output << "\n"
      end
      info output
    end
  
    def on_timeout(res, uri, num)
      Thread.exclusive do
        res[uri] ||= []
        res[uri].each {|r| r['pass'] = 'false' if !r.has_key?('pass') }
        (num - res[uri].size).times { res[uri] << {'pass' => 'false'} }
      end
      error "Waiting for test results timed out"
    end
  
    def successful_test_results?(results, expected_num)
      if results && results.kind_of?(Array) &&
         results.size == expected_num &&
         results.all? { |r| r.has_key?('pass') }
      then
        results.each do |r|
          debug "[#{['true', true].include?(r['pass']) ? "PASS" : "FAIL"}] #{results_string r['results']}"
        end
        return true
      end
      return false
    end
  
    def cleanup
      if @chiron
        @chiron.stop
        @chiron = nil
      end
      if @browser
        @browser.stop
        @browser = nil
      end
    end
    
    def report_results
      display_results
      output = "\n"
      output << '=' * 79
      output << "\n"
      unless @completed
        output << "[FAIL (incomplete)]"
        output << "\n"
        info output
        exit(1)
      end
      if all_passed?
        output << "[PASS]"
        output << "\n"
        info output
        exit(0)
      else
        output << "[FAIL]"
        output << "\n"
        info output
        exit(1)
      end
    end
    
    def all_passed?
      return false unless TestVerifier.results
      TestVerifier.results.all? do |b,br|
        browser_passed?(br)
      end
    end
    
    def browser_passed?(br)
      br.keys.size > 0 && br.all? do |u,ur|
        ur.all? {|r| ['true', true].include?(r['pass']) }
      end
    end
    
    def display_status(status)
      ['true', true].include?(status) ? "[PASS]" : "[FAIL]"
    end
    
    def display_results
      if TestVerifier.results
        output = ''
        output << "\n"
        output << '=' * 79
        output << "\n"
        output << 'Test summary'
        output << "\n"
        output << '=' * 79
        output << "\n"
        TestVerifier.results.each do |b, bvs|
          output << "#{display_status browser_passed?(bvs)} #{b.new.name}"
          output << "\n"
          bvs.each do |u, uvs|
            output << "  #{u}"
            output << "\n"
            uvs.each do |uv|
              output << "    #{display_status uv['pass']} #{results_string(uv['results'])}"
              output << "\n"
            end
          end
        end
        info output
      end
    end
    
    def results_string(results)
      return "(No results)" if results.nil? || results.keys.empty?
      results.inject([]){|l,(k,v)| l << "#{v} #{k}"}.join(', ')
    end
    
    def benchmark
      @start_time = Time.now
      yield
      @end_time = Time.now
      info "Elapsed time: #{@end_time - @start_time} second(s)"
    end
  end
end
  
if __FILE__ == $0
  def parse_args(argv)
    require 'optparse'
    
    options = {
      :log_level => "INFO",
      :browsers  => %W(explorer firefox)
    }
    
    opts = OptionParser.new do |opts|
      opts.banner = "Usage: ir.exe test_driver.rb [options]"
      
      opts.separator ""
      opts.separator "Specific options:"
      opts.separator ""
      
      opts.on("-t", "--tests HTML_FILES", Array,
              "Test HTML files to run.",
              "Use \"<html_filename>:<num>\" to specify how many test results will be returned.\n\n",
              "  \"--tests tests.html:2,tests2.html:1\"\n\n") do |tests|
        options[:tests] = tests.inject({}) { |hash, i|
          html_file, num = i.split(':')
          hash[html_file] = (num || 1).to_i
          hash
        }
      end
      
      opts.on("-b", "--browsers [BROWSERS]", Array,
              "Browser(s) to run tests on:",
              "  #{Browsers::NAMES.map{|n| Browsers.const_get(n).new.short_name}.join(', ')}.\n\n",
              "  \"--browsers chrome,firefox\"\n\n",
              "Defaults to \"#{options[:browsers].join(',')}\"\n\n") do |browsers|
        options[:browsers] = browsers
      end
      
      opts.on("-l [LEVEL]", "--log-level [LEVEL]", Logger::Severity.constants,
              "Select a log level:",
              "  #{Logger::Severity.constants.join(', ')}\n\n",
              "  \"--log-level DEBUG\"\n\n",
              "Defaults to \"INFO\"\n\n") do |level|
        options[:log_level] = level
      end
      
      opts.on_tail("-h", "--help", "Show this message") do
        puts opts
        exit
      end
    end
    
    opts.parse!(argv)
    options
  end

  TestDriver.run parse_args(ARGV)
end
