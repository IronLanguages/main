require 'windows/api'

module Windows
   module GDI
      module Bitmap
         API.auto_namespace = 'Windows::GDI::Bitmap'
         API.auto_constant  = true
         API.auto_method    = true
         API.auto_unicode   = true

         DIB_RGB_COLORS = 0
         DIB_PAL_COLORS = 1

         # Raster operations
         SRCCOPY      = 0x00CC0020
         SRCPAINT     = 0x00EE0086
         SRCAND       = 0x008800C6
         SRCINVERT    = 0x00660046
         SRCERASE     = 0x00440328
         NOTSRCCOPY   = 0x00330008
         NOTSRCERASE  = 0x001100A6
         MERGECOPY    = 0x00C000CA
         MERGEPAINT   = 0x00BB0226
         PATCOPY      = 0x00F00021
         PATPAINT     = 0x00FB0A09
         PATINVERT    = 0x005A0049
         DSTINVERT    = 0x00550009
         BLACKNESS    = 0x00000042
         WHITENESS    = 0x00FF0062
         
         API.new('BitBlt', 'LIIIILIIL', 'B', 'gdi32')
         API.new('CreateCompatibleBitmap', 'LII', 'L', 'gdi32')
         API.new('GetDIBits', 'LLIIPPI', 'I', 'gdi32')
      end
   end
end
