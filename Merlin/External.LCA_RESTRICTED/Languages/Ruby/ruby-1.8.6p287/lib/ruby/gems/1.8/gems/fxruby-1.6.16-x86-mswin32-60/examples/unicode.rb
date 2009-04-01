#!/usr/bin/env ruby

require 'fox16'
require 'jcode'

$KCODE = 'UTF8'

class UString < String
  # Show u-prefix as in Python
  def inspect; "u#{ super }" end

  # Count multibyte characters
  def length; self.scan(/./).length end

  # Reverse the string
  def reverse; self.scan(/./).reverse.join end
end

module Kernel
  def u( str )
    UString.new str.gsub(/U\+([0-9a-fA-F]{4,4})/u){["#$1".hex ].pack('U*')}
  end
end

include Fox

# Pass UTF-8 encoded Unicode strings to FXRuby.
label = u"Les enfants vont U+00E0 l'U+00E9cole.\nLa boulangU+00E8re vend-elle le pain en aoU+00FBt?"

FXApp.new("Unicode Example", "FoxTest") do |app|
  main = FXMainWindow.new(app, "Unicode Text", nil, nil, DECOR_ALL)
  FXLabel.new(main, label)
  app.create
  main.show(PLACEMENT_SCREEN)
  app.run
end
