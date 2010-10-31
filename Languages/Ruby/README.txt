redist-libs includes Ruby 1.9.2 standard library as distributed with MRI with the following changes:

i386-mingw32 directory removed
- *.so files need to be reimplemented in IronRuby.Libraries.dll
- rbconfig.rb is replaced by the one in Languages\Ruby\Libs

complex18.rb and rational18.rb are taken from MRI 1.8.6 
- this should be replaced by IronRuby's own implementation of Complex and Rational builtins in future 

gem_prelude.rb taked from Ruby 1.9.2 source distribution and adapted
- should be replaced by an implementation in IronRuby.Libraries.dll
