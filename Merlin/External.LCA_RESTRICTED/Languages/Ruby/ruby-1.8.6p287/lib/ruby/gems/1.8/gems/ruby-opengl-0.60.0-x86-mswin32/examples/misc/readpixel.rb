require 'opengl'
include Gl,Glu,Glut

begin
	require "RMagick"
rescue Exception
	print "This sample needs RMagick Module.\n"
	exit
end

WIDTH = 500
HEIGHT = 500

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT)
	
	glBegin(GL_LINES)
	glVertex(0.5, 0.5)
	glVertex(-0.5, -0.5)
	glEnd
	
	glFlush()
	
	pixels = glReadPixels(0, 0, WIDTH, HEIGHT, GL_RGBA, GL_UNSIGNED_SHORT)
	
	image = Magick::Image.new(WIDTH, HEIGHT)
	image.import_pixels(0, 0, WIDTH, HEIGHT, "RGBA", pixels,Magick::ShortPixel)
	image.flip!
	image.write("opengl_window.gif")
end

reshape = Proc.new do |w, h|
	glViewport(0, 0,  w,  h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	if (w <= h) 
		gluOrtho2D(-1.0, 1.0, -h.to_f/w.to_f, h.to_f/w.to_f)
	else 
		gluOrtho2D(w.to_f/h.to_f, w.to_f/h.to_f, -1.0, 1.0)
	end
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
end


keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
		exit(0);
	end
end

#  Main Loop
#  Open window with initial window size, title bar, 
#  color index display mode, and handle input events.
glutInit
glutInitDisplayMode(GLUT_SINGLE | GLUT_RGB | GLUT_ALPHA)
glutInitWindowSize(WIDTH, HEIGHT)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)
glutReshapeFunc(reshape)
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop

