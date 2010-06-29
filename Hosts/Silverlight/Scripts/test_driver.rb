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
  
    def run(argv)
      @completed = false
      at_exit do
        cleanup
        report_results
      end
      begin
        benchmark do
          load_config
          start_verifier
          start_chiron
          TestConfig.current.browsers.each do |browser_type|
            run_tests_in_browser(TestConfig.current.tests, browser_type)
          end
          @completed = true
        end
      rescue => e
        log.fatal e
        return 1
      end
    end
  
    def load_config
      begin
        TestConfig.load 'test.config'
      rescue LoadError
        log.fatal "No test.config found!"
        exit(1)
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
      tests.each {|htmlfile, expected_num| run_test(htmlfile, expected_num, browser_type) }
    end
  
    def run_test(htmlfile, expected_num, browser_type)
      results = TestVerifier.results[browser_type]
      uri = "#{@chiron_uri}/#{htmlfile}"
      @browser = browser_type.new
      @browser.start uri

      log.debug "Waiting for test results from #{uri}"
      
      success = false
      ITERATIONS.times do |i|
        sleep TIMEOUT
        log.debug '... waiting' if i % 20 == 0
        
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
      @browser = nil
    end
  
    def on_success(results, uri)
      #require 'repl'
      #repl binding
      results[uri].each do |r|
        puts r['output']
      end
    end
  
    def on_timeout(res, uri, num)
      Thread.exclusive do
        res[uri] ||= []
        res[uri].each {|r| r['pass'] = 'false' if !r.has_key?('pass') }
        (num - res[uri].size).times { res[uri] << {'pass' => 'false'} }
      end
      log.error "Waiting for test results timed out"
    end
  
    def successful_test_results?(results, expected_num)
      if results && results.kind_of?(Array) &&
         results.size == expected_num &&
         results.all? { |r| r.has_key?('pass') }
      then
        results.each do |r|
          log.debug "[#{['true', true].include?(r['pass']) ? "PASS" : "FAIL"}] #{results_string r['results']}"
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
      log.info '------------'
      unless @completed
        log.info "[FAIL (incomplete)]"
        exit(1)
      end
      if all_passed?
        log.info "[PASS]"
        exit(0)
      else
        log.fatal "[FAIL]"
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
        log.info '------------'
        log.info 'Test summary'
        log.info '------------'
        TestVerifier.results.each do |b, bvs|
          log.info "#{display_status browser_passed?(bvs)} #{b.new.name}"
          bvs.each do |u, uvs|
            log.info "  #{u}"
            uvs.each do |uv|
              log.info "    #{display_status uv['pass']} #{results_string(uv['results'])}"
            end
          end
        end
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
      log.debug "Elapsed time: #{@end_time - @start_time} second(s)"
    end
  end
end
  
if __FILE__ == $0
  TestDriver.run ARGV
end
