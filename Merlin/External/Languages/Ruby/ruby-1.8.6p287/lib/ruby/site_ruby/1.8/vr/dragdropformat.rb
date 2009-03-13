###################################
#
# dragdropformat.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2002-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
###################################

VR_DIR="vr/" unless defined?(::VR_DIR)
require VR_DIR+'sysmod'
require 'Win32API'

module ClipboardFormat
  RegisterClipboardFormat = 
      Win32API.new("user32","RegisterClipboardFormat","P","I")

  CF_URL = RegisterClipboardFormat.call("UniformResourceLocator")

end

class DragDropObject
=begin
== DragDropObject
A capsule for OLE Drag and Drop
This is just a base class. Don't use this directly.

=== Methods
--- handle
    Returns a global heap handle containing the file pathes.
--- free_handle
    Tries to free the handle.
    Please check you really need to free it, because sometimes your OS will 
    free it automatically at the appropriate timing.
=end

  def initialize
    @__binarydata=""
  end
  def handle
    @handle=GMEM::AllocStr(0x2000,@__binarydata)
  end
  def free_handle
    GMEM::Free(@handle)
    @handle=0
  end

  def objectformat
    self.class::FormatId
  end

end

class DragDropText < DragDropObject
=begin
== DragDropText
   This class deals the structure for drag-and-drop files.

=== Class Methods
--- set(files)
    Creates object and sets the file pathes in ((|files|)) to the object.
--- get(handle)
    Create object and sets the file pathes containing in ((|handle|)).
    The ((|handle|)) is a global heap handle.
=== Methods
--- text
    Returns the text.
=end

  FormatName="CF_TEXT"
  FormatId=ClipboardFormat::CF_TEXT

  def initialize(text,handle=0)
    @__binarydata = @__texts = text.to_str.dup
    @handle=handle
  end

  def self.set(texts)
    self.new(texts)
  end

  def self.get(handle)
    self.new(GMEM::Get(handle),handle)
  end

  def text
    @__texts.dup
  end
end

class DragDropFiles < DragDropObject
=begin
== DragDropFiles
   This class deals the structure for drag-and-drop files.

=== Class Methods
--- set(files)
    Creates object and sets the file pathes in ((|files|)) to the object.
--- get(handle)
    Create object and sets the file pathes containing in ((|handle|)).
    The ((|handle|)) is a global heap handle.
=== Methods
--- files
    Returns the file pathes.
=end

  FormatName = "CF_HDROP"
  FormatId = ClipboardFormat::CF_HDROP

  DragQueryFile   = Win32API.new("shell32","DragQueryFile", ["I","I","P","I"],"I")

  def initialize(files,handle=0)
    @__files = files.dup
    @handle=handle
    @__binarydata="\0"*0x14+@__files.join("\0")+"\0"
    @__binarydata[0]="\x14"
  end

  def self.set(files)
    self.new(files)
  end

  def self.get(handle)
    __fnbuffer = "                "*16 #256bytes
    n=DragQueryFile.call(handle,-1,__fnbuffer,__fnbuffer.size)
    r=[]
    0.upto(n-1) do |i|
      s=DragQueryFile.call(handle,i,__fnbuffer,__fnbuffer.size)
      r.push __fnbuffer[0,s]
    end
    self.new(r,handle)
  end

  def files
    @__files.dup
  end
end


module DragDropRubyObjectFactory
=begin
== DragDropRubyObjectFactory
This class' feature is to register new clipboard format into your system for 
drag&drop ruby object.

=== Class Methods
--- declare_dndclass(classname,registername)
    Registers new format and create new wrapper class for ruby.
    The format is registered as ((|registername|)) and
    the wrapper class is declared as DragDropRubyObjectFactory::((|classname|)).
    This methods returns the wrapper class.
=end


  class DragDropRubyObject < DragDropObject
=begin
== DragDropRubyObjectFactory::DragDropRubyObject
This is a base class of wrapper classes created by 
DragDropRubyObjectFactory.declare_dndclass()

=== Class Methods
--- new(binstr,handle=0)
    No need to use this. (maybe)
--- set(obj)
    Returns new wrapper object of ((|obj|)) for drag&drop.
--- get(handle)
    Returns wrapper object using global heap of ((|handle|)).
=== Method
--- object
    returns the wrapped object.
=end

    include ClipboardFormat
    FormatName = "DumpedRubyObjectForDnD"
    FormatId = RegisterClipboardFormat.call(FormatName)

    def initialize(bin,handle=0)
      @objectformat = ClipboardFormat::CF_TEXT # only for base class
      @__binarydata = bin
      @handle=handle
    end

    def self.set(obj)
      self.new(Marshal.dump(obj))
    end

    def self.get(handle)
      bin =  GMEM::Get(handle)
      self.new(bin,handle)
    end

    def object
      Marshal.load(@__binarydata)
    end
  end

  def self.declare_dndclass(classname,registername)
    str = <<"EEOOFF"
        class #{classname} < DragDropRubyObject
          FormatName = '#{registername}'
          FormatId = RegisterClipboardFormat.call(FormatName)
        end
EEOOFF
    eval(str)
    eval("#{classname} ")
  end

end
