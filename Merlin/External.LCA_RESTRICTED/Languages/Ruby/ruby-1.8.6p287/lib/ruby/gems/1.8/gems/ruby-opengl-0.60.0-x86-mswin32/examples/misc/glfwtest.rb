require 'opengl'
require 'glfw'

#init
Glfw.glfwOpenWindow( 500,500, 0,0,0,0, 32,0, Glfw::GLFW_WINDOW )

# main loop
while true
	if( Glfw.glfwGetWindowParam( Glfw::GLFW_OPENED ) == false ||
		  Glfw.glfwGetKey(Glfw::GLFW_KEY_ESC) == Glfw::GLFW_PRESS )
		break
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

  Glfw.glfwSwapBuffers()

	sleep 0.01 # to avoid consuming all CPU power
end
