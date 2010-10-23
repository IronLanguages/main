require "win32ole"

# This is really just a rubified copy of IronPython's cominterop_util.py
module ComHelper
  include Microsoft::Win32

  def app_installed?(appname, binary = appname)
    app = nil

    # check for any of the Office versions known to work with the tests
    [10.0, 11.0, 12.0, 14.0].each do |version|
      app ||= Registry.local_machine.open_sub_key("Software\\Microsoft\\Office\\#{version}\\#{appname}\\InstallRoot")
    end

    return nil unless app

    File.exists?(app.get_value("Path") + binary + ".exe")
  end
  private :app_installed?
  module_function :app_installed?
  
  def srv_registered?(progid)
    !Registry.classes_root.get_sub_key_names.grep(/#{progid}/i).empty?
  end
  module_function :srv_registered?

  def excel_installed?
    app_installed?("excel")
  end
  module_function :excel_installed?

  def word_installed?
    app_installed?("word", "winword")
  end
  module_function :word_installed?

  def create_app(prog_id_or_cls_id)
    prog_id_or_cls_id = prog_id_or_cls_id.to_s if prog_id_or_cls_id.is_a? System::Guid
    WIN32OLE.new prog_id_or_cls_id
  end
  private :create_app
  module_function :create_app
  
  def create_excel_app
    create_app("Excel.Application")
  end
  module_function :create_excel_app

  def create_word_app
    create_app("Word.Application")
  end
  module_function :create_word_app
  
  class EventTracker
    attr_reader :counter
    def initialize
      @counter = 0
    end

    def handler(obj, event)
      @counter += 1
    end
  end
end
