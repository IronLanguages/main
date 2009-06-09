ruby-opengl
===========

[ruby-opengl][] consists of Ruby extension modules that are bindings for
the [OpenGL][], GLU, and [GLUT][] libraries. It is intended to be a
replacement for -- and uses the code from -- [Yoshi's ruby-opengl][].

It's licensed under the MIT license. Please see the included MIT-LICENSE
file for the terms of the license.


[ruby-opengl]: http://rubyforge.org/projects/ruby-opengl/
[OpenGL]: http://www.opengl.org/
[GLUT]: http://www.opengl.org/documentation/specs/glut/spec3/spec3.html


[Yoshi's ruby-opengl]: http://www2.giganet.net/~yoshi/


Current release: 0.60.0 (December 29, 2007)
-----------------
Changes in this release:
* Automatic error checking for GL/GLU calls, enabled by default (see doc/tutorial)
* Added support for many more OpenGL extensions
* Support for Ruby 1.9.0+ (requires mkrf 0.2.3)
* Ton of bugfixes.<br><br>
<b>API Changes:</b>
* Boolean functions/parameters was changed to ruby true/false instead of GL\_TRUE / GL\_FALSE, which remains for compatibility
* glGet\* functions now returns x instead of [x] when returning only one value
* Functions operating on packed strings (glTexture, gl\*Pointer etc.) now also accepts ruby arrays directly
* Matrix handling functions now also accepts instances of Matrix class, or any class that can be converted to array
* glUniform\*v and glUniformmatrix\*v now does not require 'count' parameter, they will calculate it from length of passed array
* glCallLists needs type specifier (previously was forced to GL_BYTE)
* On ruby 1.9, glut keyboard callback returns char ("x") instead of integer so using 'if key == ?x' works on both 1.8 and 1.9

Previous release: 0.50.0 (October 23, 2007)
-----------------
Changes in this release:

* GLU and GLUT cleanup, bugfixes, some missing functions added - version 3.7
  of GLUT API is now requirement (previously 3.0)
* We added support for number of OpenGL extensions
* Some new examples and code cleanup
* Support for OpenGL 2.1 (that includes pixelpack/unpack buffer)
* Some code refactoring to remove duplicity
* Documentation update (still no API doc though)
* Lots of bugfixes.
