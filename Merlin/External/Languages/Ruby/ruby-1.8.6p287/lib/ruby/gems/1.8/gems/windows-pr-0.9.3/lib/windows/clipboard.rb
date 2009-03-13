require 'windows/api'

module Windows
   module Clipboard
      API.auto_namespace = 'Windows::Clipboard'
      API.auto_constant  = true
      API.auto_method    = true
      API.auto_unicode   = true

      CF_TEXT         = 1
      CF_BITMAP       = 2
      CF_METAFILEPICT = 3
      CF_SYLK         = 4
      CF_DIF          = 5
      CF_TIFF         = 6
      CF_OEMTEXT      = 7
      CF_DIB          = 8
      CF_PALETTE      = 9
      CF_PENDATA      = 10
      CF_RIFF         = 11
      CF_WAVE         = 12
      CF_UNICODETEXT  = 13
      CF_ENHMETAFILE  = 14

      API.new('OpenClipboard', 'L', 'B', 'user32')
      API.new('CloseClipboard', 'V', 'B', 'user32')
      API.new('GetClipboardData', 'I', 'P', 'user32')
      API.new('EmptyClipboard', 'V', 'B', 'user32')
      API.new('SetClipboardData', 'IL', 'L', 'user32')
      API.new('CountClipboardFormats', 'V', 'I', 'user32')
      API.new('IsClipboardFormatAvailable', 'I', 'B', 'user32')
      API.new('GetClipboardFormatName', 'IPI', 'I', 'user32')
      API.new('EnumClipboardFormats', 'I', 'I', 'user32')
      API.new('RegisterClipboardFormat', 'S', 'I', 'user32')
   end
end
