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
    Html2Text.html2text(HttpUtility.html_decode(HttpUtility.url_decode(html)))
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
      TestVerifier.results[tr['browser']][tr['url']] ||= []
      TestVerifier.results[tr['browser']][tr['url']] << tr
    end
    status 200
    log.debug tr.inspect
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