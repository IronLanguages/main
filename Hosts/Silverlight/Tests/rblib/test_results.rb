include System
include System::Windows::Browser

$: << 'lib'
require 'system-json'

class TestResults
  URI = "http://localhost:9090/complete" unless defined? URI
  BROWSERS = %W(explorer firefox chrome safari) unless defined? BROWSERS

  class << self
    def broadcast(results, passed, output = '')
      results = to_hash(results)
      uri = Uri.new URI
      request = System::Net::WebClient.new
      request.upload_string_async uri, to_post_data({
        'url' => url,
        'results' => results.to_json,
        'output' => output,
        'browser' => browser,
        'pass' => passed
      })
    end
    
    def url
      @url ||= HtmlPage.document.document_uri.to_s
    end
    
    def browser
      @browser ||= begin
        index = case HtmlPage.BrowserInformation.user_agent
          when /MSIE/:    0
          when /Firefox/: 1 
          when /Chrome/:  2
          when /Safari/:  3
          when /Opera/:   4
        end
        return nil if index.nil?
        BROWSERS[index]
      end
    end
    
    def to_post_data(hash)
      hash.inject([]){|r,(k,v)| r << [HttpUtility.url_encode(k.to_s),HttpUtility.url_encode(v.to_s)].join('=')}.join('&')
    end
    
    def to_hash(dict)
      unless dict.kind_of?(Hash)
        hash = {}
        dict.keys.each do |i|
          hash[i.to_s] = dict[i]
        end
        return hash
      end
      dict
    end
  end  
end
