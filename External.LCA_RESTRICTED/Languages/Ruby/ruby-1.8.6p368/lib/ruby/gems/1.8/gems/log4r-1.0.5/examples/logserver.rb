# How to use LogServer

$: << File.join('..','src')
require 'log4r'
require 'log4r/configurator'

# XML configuration is simple enough to embed here
xml = %(
<log4r_config>
  <logserver name="server" uri="tcpromp://localhost:9999">
   <outputter>stdout</outputter>
  </logserver>
</log4r_config>
)
Log4r::Logger.new('log4r').add 'stdout'        # to see what's going on inside
Log4r::Configurator.load_xml_string xml        # load it up
sleep                                   
# now run logclient.rb on another terminal
