###################################
#
# rscutil.rb
# Programmed by nyasu <nyasu@osk.3web.ne.jp>
# Copyright 2000-2005  Nishikawa,Yasuhiro
#
# More information at http://vruby.sourceforge.net/index.html
#
#
###################################

require 'swin'


=begin
= rscutil.rb
Utilities for resources

== Base64dumper
The module to use binary data with base64
=== Class Method
--- loadString(str)
    Load a object from base64 encoded string.
=== Method
--- dumpString
    Dumps an object into base64 encoded string.
=end

module Base64dumper
  def self.loadString(str)
    Marshal.load(str.unpack("m")[0])
  end
  
  def dumpString
    [Marshal.dump(self)].pack("m")
  end
end



module SWin
  class Bitmap
    include Base64dumper
    def self.loadString(str) Base64dumper.loadString(str) end
  end
end

#####

module SWin
  class Font
    STRIKEOUT=4
    UNDERLINE=2
    ITALIC=1
  end
end

## ####--------------------

# FontStruct for handling fonts and text attributes.

=begin
== FontStruct
Subclass of Struct for representing text attributes.

=== Attributes
--- fontface
--- height
--- style
--- weight
--- width
--- escapement
--- orientation
--- pitch_family
--- charset
--- point
--- color

=== Class Method
--- new2(args)
    Use new2 instead of new.

=== Methods
--- params
    Returns parameters that can be used for parameter of 
    SWin::LWFactory.newfont()
--- spec
    Returns text attributes for parameter of SWin::CommonDialog.chooseFont
--- bold?
    Returns whether the attributes means bold style
--- bold=(flag)
    Sets or resets bold style.
--- italic?
    Returns whether the attributes means italic style
--- italic=(flag)
    Sets or resets italic style.
--- underlined?
    Returns whether the attributes means underline style
--- underlined=(flag)
    Sets or resets underline style.
--- striked?
    Returns whether the attributes means strike-out style
--- striked=(flag)
    Sets or resets strike-out style.
=end

FontStruct = Struct.new("FontStruct",
           :fontface,:height,:style,:weight,:width,:escapement,
           :orientation,:pitch_family,:charset,
           :point,:color)

class FontStruct
  private_class_method :new

  def self.new2(*args)
    new(*args.flatten)
  end

  def params
    to_a[0,9]
  end

  def spec
    a=self.to_a
    [ a[0,9],a[9],a[10] ]
  end

  def bold=(flag)
    self.weight = if flag then 600 else 300 end
  end
  def bold?
    (@weight>400)
  end
  
  def italic=(flag)
    if flag then
      self.style |= 1  # 1=SWINFONT_ITALIC
    else
      self.style &= 0xfffffffe
    end
  end
  def italic?
    (self.style&1)>0
  end

  def underlined=(flag)
    if flag then
      self.style |= 2  # 2=SWINFONT_ULINE
    else
      self.style &= 0xfffffffd
    end
  end
  def underlined?
    (self.style&2)>0
  end

  def striked=(flag)
    if flag then
      self.style |= 4  # 4=SWINFONT_STRIKE
    else
      self.style &= 0xfffffffb
    end
  end
  def striked?
    (self.style&4)>0
  end
end

