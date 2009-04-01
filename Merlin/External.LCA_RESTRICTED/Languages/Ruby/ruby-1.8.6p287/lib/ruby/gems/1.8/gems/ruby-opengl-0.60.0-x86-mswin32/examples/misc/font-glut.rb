require 'opengl'
include Gl,Glu,Glut

display = Proc.new do
	glClear(GL_COLOR_BUFFER_BIT)
	GL.LoadIdentity
	
	glRasterPos2d(100,100)
	"Hello Bitmap".each_byte { |x| glutBitmapCharacter(GLUT_BITMAP_9_BY_15, x) }
	
	GL.Translate(100, 250, 0)
	GL.Scale(0.5, 0.5, 1)
	"Hello Stroke".each_byte { |x| glutStrokeCharacter(GLUT_STROKE_ROMAN, x) }
	
	glutSwapBuffers()
end

reshape = Proc.new do |w, h|
	glViewport(0, 0,  w,  h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	glOrtho(0.0, w, 0.0, h, -1.0, 1.0)
	glMatrixMode(GL_MODELVIEW)
end

keyboard = Proc.new do |key, x, y|
	case (key)
		when ?\e
			exit(0)
	end
end

#  Main Loop
#  Open window with initial window size, title bar, 
#  color index display mode, and handle input events.
#
glutInit
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB)
glutInitWindowSize(500, 500)
glutInitWindowPosition(100, 100)
glutCreateWindow($0)

glutReshapeFunc(reshape)
glutDisplayFunc(display)
glutKeyboardFunc(keyboard)
glutMainLoop
