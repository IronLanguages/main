# This is really just a rubified copy of IronPython's cominterop_util.py
module ComHelper
  include Microsoft::Win32

  def app_installed?(appname, binary = appname)
    app = nil

    app = Registry.local_machine.open_sub_key("Software\\Microsoft\\Office\\12.0\\#{appname}\\InstallRoot")
    app ||= Registry.local_machine.open_sub_key("Software\\Microsoft\\Office\\11.0\\#{appname}\\InstallRoot")
    app ||= Registry.local_machine.open_sub_key("Software\\Microsoft\\Office\\14.0\\#{appname}\\InstallRoot")

    return nil unless app

    File.exists?(app.get_value("Path") + binary + ".exe")
  end
  private :app_installed?
  module_function :app_installed?

  def excel_installed?
    app_installed?("excel")
  end
  module_function :excel_installed?

  def word_installed?
    app_installed?("word", "winword")
  end
  module_function :word_installed?

  def create_app(prog_id_or_cls_id)
    if prog_id_or_cls_id.is_a? System::Guid
      type = System::Type.GetTypeFromCLSID(prog_id_or_cls_id)
    elsif guid? prog_id_or_cls_id
      type = System::Type.GetTypeFromCLSID(System::Guid.new(prog_id_or_cls_id))
    else
      type = System::Type.GetTypeFromProgID(prog_id_or_cls_id)
    end
    raise "Unknown PROGID '#{prog_id_or_cls_id}'" if type.nil?
    System::Activator.CreateInstance(type)
  end
  private :create_app
  module_function :create_app
  
  def guid?(str)
    /[{]?[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}[}]?|[0-9A-F]{32}/ =~ str
  end
  private :guid?
  module_function :guid?

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
