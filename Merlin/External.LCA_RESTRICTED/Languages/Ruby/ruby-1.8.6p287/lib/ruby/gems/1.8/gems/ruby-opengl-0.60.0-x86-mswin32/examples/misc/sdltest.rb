require 'opengl'
require 'sdl'

#init
SDL.init(SDL::INIT_VIDEO)
SDL.setGLAttr(SDL::GL_DOUBLEBUFFER,1)
SDL.setVideoMode(512,512,32,SDL::OPENGL)

# main loop
while true
	while event = SDL::Event2.poll
		case event
		when SDL::Event2::KeyDown, SDL::Event2::Quit
			exit
		end
	end

	Gl.glClear( Gl::GL_COLOR_BUFFER_BIT | Gl::GL_DEPTH_BUFFER_BIT )

	Gl.glBegin( Gl::GL_POLYGON )
	Gl.glColor3f( 1.0, 0.0, 0.0 )
	Gl.glVertex2f( -0.5, -0.5 )
	Gl.glColor3f( 0.0, 1.0, 0.0 )
	Gl.glVertex2f( -0.5,  0.5 )
	Gl.glColor3f( 0.0, 0.0, 1.0 )
	Gl.glVertex2f(  0.5,  0.5 )
	Gl.glColor3f( 1.0, 0.0, 1.0 )
	Gl.glVertex2f(  0.5, -0.5 )
	Gl.glEnd

	SDL.GLSwapBuffers()

	sleep 0.01 # to avoid consuming all CPU power
end
