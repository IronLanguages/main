require 'test_driver/logger'
require 'test_driver/process'

module Browsers
  PROGRAM_FILES = ENV[ENV.has_key?('ProgramFiles(x86)') ? 'ProgramFiles(x86)' : 'ProgramFiles']
  
  def os_version
    if @windows_version.nil?
      @windows_version = mac? ? System::Environment.o_s_version.version.major : nil
    end
    @windows_version
  end

  def mac?
    if @is_mac.nil?
      @is_mac = RbConfig::CONFIG['target_os'] =~ /darwin/i != nil
    end
    @is_mac
  end
  
  # Base host class.
  # Derived class needs to have below methods implemented:
  # name - name of the host
  # path - path to the host
  class BrowserBase < ProcessWrapper
    include TestLogger
    include Browsers

    def self.get_browser(name)
      browser_constant_name = NAMES.select do |i|
        Browsers.const_get(i).new.short_name == name
      end.first
      Browsers.const_get(browser_constant_name) if browser_constant_name
    end

    def short_name
      name.split.last.downcase
    end

    def initialize
      super
      @platform = mac? ? 'mac' : 'win'
    end

    def start(url)
      info "Starting #{name}"
      __start path, url_to_args(url)
    end
  
    def stop
      info "Stopping #{name}"
      __stop
    end
    
    def path
      mac? ? mac_path : windows_path
    end
    
    def url_to_args(url)
      "#{url}"
    end
    
    def installed?
      __exist? path
    end
    
    def name
      raise "Sub-classes must provide the browser's name (should match what Silverlight's HtmlPage.BrowserInformation.Name returns)"
    end
  end

  class IE < BrowserBase
    def name
      "Microsoft Internet Explorer"
    end
    
    def windows_path
      "#{PROGRAM_FILES}/Internet Explorer/iexplore.exe"
    end
    
    def supported?
      installed? and not mac?
    end
  end
      
  class Safari < BrowserBase
    def name
      "Apple Safari"
    end

    def windows_path
      "#{PROGRAM_FILES}/Safari/Safari.exe"
    end
    
    def mac_path
      "/Applications/Firefox.app/Contents/MacOS/safari"
    end
    
    def supported?
      installed?
    end
  end

  class FireFox < BrowserBase
    def name
      "Mozilla Firefox"
    end
  
    def windows_path
      "#{PROGRAM_FILES}/Mozilla Firefox/firefox.exe"
    end
    
    def mac_path
      "/Applications/Firefox.app/Contents/MacOS/firefox #{url} &"
    end
    
    def supported?
      installed?
    end
  end
  
  class Chrome < BrowserBase
    def name
      "Google Chrome"
    end
    
    def windows_path
      "#{ENV['SystemDrive']}#{ENV['HOMEPATH']}\\AppData\\Local\\Google\\Chrome\\Application\\chrome.exe"
    end
    
    def supported?
      installed? and not mac?
    end
  end
  
  class Opera < BrowserBase
    def name
      "Opera"
    end
    
    def windows_path
      "#{PROGRAM_FILES}/Opera/opera.exe"
    end
    
    def mac_path
      "/Applications/Opera.app"
    end
    
    def supported?
      installed?
    end
  end

  NAMES = constants - ["PROGRAM_FILES", "BrowserBase"]
end
