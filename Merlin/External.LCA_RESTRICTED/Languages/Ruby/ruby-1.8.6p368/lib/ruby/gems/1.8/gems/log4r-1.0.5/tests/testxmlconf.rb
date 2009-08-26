require "include"
require "runit/cui/testrunner"

One=<<-EOX
<log4r_config><pre_config><custom_levels> Foo </custom_levels>
</pre_config></log4r_config>
EOX
Two=<<-EOX
<log4r_config><pre_config><global level="DEBUG"/></pre_config></log4r_config>
EOX
Three=<<-EOX
<log4r_config><pre_config><custom_levels>Foo</custom_levels>
<global level="Foo"/></pre_config>
</log4r_config>
EOX

# must be run independently
class TestXmlConf < TestCase
  def test_load1
    Configurator.load_xml_string(One)
    assert_no_exception{ 
      assert(Foo == 1) 
      assert(Logger.global.level == ALL)
    }
  end
  def test_load2
    Configurator.load_xml_string(Two)
    assert_no_exception{ 
      assert(Logger.global.level == DEBUG)
    }
  end
  def test_load3
    Configurator.load_xml_string(Three)
    assert_no_exception{ 
      assert(Foo == 1) 
      assert(Logger.global.level == Foo)
    }
  end
  def test_load4
    assert_no_exception {
      Configurator['logpath'] = '.'
      Configurator.load_xml_file "xml/testconf.xml"
      a = Logger['first::second']
      a.bing "what the heck"
    }
  end
end

if __FILE__ == $0
  CUI::TestRunner.run(TestXmlConf.new("test_load#{ARGV[0]}"))
end
