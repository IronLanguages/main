require 'rubygems'
require 'json'
require 'sinatra/base'
require 'test_driver/logger'
require 'test_driver/browsers'

load_assembly 'System.Web'

class TestVerifier < Sinatra::Base
  include TestLogger
  include System::Web
  IronRuby.require 'test_driver/site'
  Html2Text = IronRuby.require 'test_driver/html2text'
  
  class << self
    attr_accessor :results
  end
  
  def html2text(html)
    text = Html2Text.html2text(HttpUtility.html_decode(HttpUtility.url_decode(html)))
    text.split("\n\n").join("\n")
  end
  
  def expected_num(url)
    require 'uri'
    u = URI.parse(url)
    TestConfig.current.tests[u.path[1..-1]]
  end
  
  COMPLETE = lambda do
    tr = {}
    tr['results'] = JSON.parse(params['results'])
    tr['output'] = html2text(params['output'])
    tr['pass'] = params['pass']
    tr['browser'] = Browsers::BrowserBase.get_browser(params['browser'])
    tr['url'] = params['url']
    Thread.exclusive do
      TestVerifier.results ||= {}
      TestVerifier.results[tr['browser']] ||= {}
      num_expected_results = expected_num(tr['url'])
      if num_expected_results.to_i > 0
        TestVerifier.results[tr['browser']][tr['url']] ||= []
        if TestVerifier.results[tr['browser']][tr['url']].size < num_expected_results
          TestVerifier.results[tr['browser']][tr['url']] << tr
        end
      end
    end
    status 200
    debug tr.inspect
    nil
  end

  get  '/complete', &COMPLETE
  post '/complete', &COMPLETE

  get '/clientaccesspolicy.xml' do
    content_type 'application/xml'
    <<-EOS
<?xml version="1.0" encoding="utf-8"?>
<access-policy>
  <cross-domain-access>
    <policy>
      <allow-from>      
        <domain uri="*"/>
      </allow-from>      
      <grant-to>
        <resource path="/" include-subpaths="true"/>
      </grant-to>      
    </policy>
  </cross-domain-access>
</access-policy>
EOS
  end
end