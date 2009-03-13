# -*- encoding: utf-8 -*-

Gem::Specification.new do |s|
  s.name = %q{ruby-opengl}
  s.version = "0.60.0"
  s.platform = %q{i386-mswin32}

  s.required_rubygems_version = nil if s.respond_to? :required_rubygems_version=
  s.authors = ["Alain Hoang", "Jan Dvorak", "Minh Thu Vo", "James Adam"]
  s.autorequire = %q{gl}
  s.cert_chain = nil
  s.date = %q{2008-01-07}
  s.files = ["lib/gl.so", "lib/glu.so", "lib/glut.so", "lib/opengl.rb", "examples/misc", "examples/misc/anisotropic.rb", "examples/misc/fbo_test.rb", "examples/misc/font-glut.rb", "examples/misc/glfwtest.rb", "examples/misc/OGLBench.rb", "examples/misc/plane.rb", "examples/misc/readpixel.rb", "examples/misc/sdltest.rb", "examples/misc/trislam.rb", "examples/NeHe", "examples/NeHe/nehe_lesson02.rb", "examples/NeHe/nehe_lesson03.rb", "examples/NeHe/nehe_lesson04.rb", "examples/NeHe/nehe_lesson05.rb", "examples/NeHe/nehe_lesson36.rb", "examples/OrangeBook", "examples/OrangeBook/3Dlabs-License.txt", "examples/OrangeBook/brick.frag", "examples/OrangeBook/brick.rb", "examples/OrangeBook/brick.vert", "examples/OrangeBook/particle.frag", "examples/OrangeBook/particle.rb", "examples/OrangeBook/particle.vert", "examples/README", "examples/RedBook", "examples/RedBook/aapoly.rb", "examples/RedBook/aargb.rb", "examples/RedBook/accanti.rb", "examples/RedBook/accpersp.rb", "examples/RedBook/alpha.rb", "examples/RedBook/alpha3D.rb", "examples/RedBook/bezcurve.rb", "examples/RedBook/bezmesh.rb", "examples/RedBook/checker.rb", "examples/RedBook/clip.rb", "examples/RedBook/colormat.rb", "examples/RedBook/cube.rb", "examples/RedBook/depthcue.rb", "examples/RedBook/dof.rb", "examples/RedBook/double.rb", "examples/RedBook/drawf.rb", "examples/RedBook/feedback.rb", "examples/RedBook/fog.rb", "examples/RedBook/font.rb", "examples/RedBook/hello.rb", "examples/RedBook/image.rb", "examples/RedBook/jitter.rb", "examples/RedBook/lines.rb", "examples/RedBook/list.rb", "examples/RedBook/material.rb", "examples/RedBook/mipmap.rb", "examples/RedBook/model.rb", "examples/RedBook/movelight.rb", "examples/RedBook/pickdepth.rb", "examples/RedBook/planet.rb", "examples/RedBook/quadric.rb", "examples/RedBook/robot.rb", "examples/RedBook/select.rb", "examples/RedBook/smooth.rb", "examples/RedBook/stencil.rb", "examples/RedBook/stroke.rb", "examples/RedBook/surface.rb", "examples/RedBook/teaambient.rb", "examples/RedBook/teapots.rb", "examples/RedBook/tess.rb", "examples/RedBook/texbind.rb", "examples/RedBook/texgen.rb", "examples/RedBook/texturesurf.rb", "examples/RedBook/varray.rb", "examples/RedBook/wrap.rb", "doc/build_install.txt", "doc/history.txt", "doc/requirements_and_design.txt", "doc/roadmap.txt", "doc/scientific_use.txt", "doc/thanks.txt", "doc/tutorial.txt", "MIT-LICENSE", "README.txt"]
  s.homepage = %q{http://ruby-opengl.rubyforge.org}
  s.require_paths = ["lib"]
  s.required_ruby_version = Gem::Requirement.new("> 0.0.0")
  s.rubygems_version = %q{1.3.1}
  s.summary = %q{OpenGL Interface for Ruby}

  if s.respond_to? :specification_version then
    current_version = Gem::Specification::CURRENT_SPECIFICATION_VERSION
    s.specification_version = 1

    if Gem::Version.new(Gem::RubyGemsVersion) >= Gem::Version.new('1.2.0') then
    else
    end
  else
  end
end
