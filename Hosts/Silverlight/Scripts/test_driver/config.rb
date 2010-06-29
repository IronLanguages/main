require 'test_driver/logger'

class TestConfig

  BUILD_CONFIGS = [
    "Silverlight3Debug", "Silverlight3Release", 
    "Silverlight4Debug", "Silverlight4Release"
  ]

  attr_reader :tests, :tests_dir

  attr_accessor :browsers

  # "tests_dir" is the directory where tests are located
  # "tests" is either an Array<String>, or a Hash<String> => Array<String>
  # The Strings represent HTML files, and the Hash is if one HTML file embeds
  # other HTML files actually running tests (using a <frame>, for example).
  def initialize(tests_dir, tests)
    @tests_dir = tests_dir
    @tests = tests
    @browsers = []
  end

  def browsers=(val)
    @browsers = val.map{|i| Browsers::BrowserBase.get_browser(i)}
  end

  class << self
    include TestLogger

    def current
      @current
    end

    def browsers
      @current.browsers
    end

    def test_config(opts)
      @current = TestConfig.new(Dir.pwd, opts)
    end

    def load(path)
      path = File.join Dir.pwd, path
      raise LoadError unless File.exist?(path)
      self.instance_eval File.read(path), File.expand_path(path)
    end
    
    def build_config=(type)
      @build_config = type
    end
    
    def build_config
      unless @build_config
        log.debug "Looking for build configurations"
        BUILD_CONFIGS.each do |b|
          bc = get_build_config(b)
          if File.exist?(File.join(bc[1], "Microsoft.Scripting.Silverlight.dll"))
            log.debug "FOUND \"#{b}\""
            @build_config, @build_path = bc
            break
          end
        end
      end
      unless @build_config
        log.fatal "No Silverlight build found! Build any of the Silverlight build configurations in Dlr.sln and retry."
        exit(1)
      end
      [@build_config, @build_path]
    end
    
    private
    
      def get_build_config(type)
        return [type, File.expand_path(File.join(RbConfig::CONFIG['bindir'], '..', type))]
      end
  end
end