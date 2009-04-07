# Nehe Lesson 36 Code
# modified from immediate mode to use vertex arrays for helix drawing
require 'opengl'
include Gl,Glu,Glut
include Math

def emptyTexture
	# Create Storage Space For Texture Data (128x128x4)
	data = ([0]*4*128*128).pack("f*")
	txtnumber = glGenTextures(1) # Create 1 Texture
	glBindTexture(GL_TEXTURE_2D, txtnumber[0]) # Bind The Texture 
	glTexImage2D(GL_TEXTURE_2D, 0, 4, 128, 128, 0,
		GL_RGBA, GL_FLOAT, data) # Build Texture Using Information In data
	glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR)
	glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR)
	return txtnumber[0] # Return The Texture ID
end

def init
	global_ambient = [0.2, 0.2, 0.2, 1.0] # Set Ambient Lighting To Fairly Dark Light (No Color)
	light0pos = [0.0, 5.0, 10.0, 1.0] # Set The Light Position
	light0ambient = [0.2, 0.2, 0.2, 1.0] # More Ambient Light
	light0diffuse = [0.3, 0.3, 0.3, 1.0] # Set The Diffuse Light A Bit Brighter
	light0specular = [0.8, 0.8, 0.8, 1.0] # Fairly Bright Specular Lighting

	lmodel_ambient = [0.2,0.2,0.2,1.0] # And More Ambient Light

	$angle = 0.0 # Set Starting Angle To Zero 
	
	$lasttime = 0
	
	$blurTexture = emptyTexture() # Create Our Empty Texture

	$helix_v,$helix_n = createHelix()
	glVertexPointer(3,GL_FLOAT,0,$helix_v.flatten.pack("f*"))
	glNormalPointer(GL_FLOAT,0,$helix_n.flatten.pack("f*"))

	glLoadIdentity() # Reset The Modelview Matrix

	glEnable(GL_DEPTH_TEST) # Enable Depth Testing

	glLightModelfv(GL_LIGHT_MODEL_AMBIENT,lmodel_ambient) # Set The Ambient Light Model
	
	glLightModelfv(GL_LIGHT_MODEL_AMBIENT,global_ambient) # Set The Global Ambient Light Model
	glLightfv(GL_LIGHT0, GL_POSITION, light0pos) # Set The Lights Position
	glLightfv(GL_LIGHT0, GL_AMBIENT, light0ambient)	# Set The Ambient Light
	glLightfv(GL_LIGHT0, GL_DIFFUSE, light0diffuse) # Set The Diffuse Light
	glLightfv(GL_LIGHT0, GL_SPECULAR, light0specular)	# Set Up Specular Lighting
	glEnable(GL_LIGHTING) # Enable Lighting
	glEnable(GL_LIGHT0) # Enable Light0

	glShadeModel(GL_SMOOTH) # Select Smooth Shading

	glMateriali(GL_FRONT, GL_SHININESS, 128)
	glClearColor(0.0, 0.0, 0.0, 0.5) # Set The Clear Color To Black
end

# Keyboard handler to exit when ESC is typed
keyboard = lambda do |key, x, y|
	case(key)
		when ?\e
			exit(0)
	end
	glutPostRedisplay
end

reshape = lambda do |w,h|
	glMatrixMode(GL_PROJECTION)
	glViewport(0,0,w,h)
	glLoadIdentity()
	width = 0.5
	height = 0.5 * h/w;
	glFrustum(-width,width,-height,height,1.0,2000.0)
	glMatrixMode(GL_MODELVIEW)
	glViewport(0,0,w,h)
end

def viewOrtho
	glMatrixMode(GL_PROJECTION) # Select Projection
	glPushMatrix() # Push The Matrix
	glLoadIdentity() # Reset The Matrix
	width = glutGet(GLUT_WINDOW_WIDTH)
	height = glutGet(GLUT_WINDOW_HEIGHT)
	glOrtho( 0, width , height , 0, -1, 1 ) # Select Ortho Mode (widthxheight)
	glMatrixMode(GL_MODELVIEW) # Select Modelview Matrix
	glPushMatrix() # Push The Matrix
	glLoadIdentity() # Reset The Matrix
end

def viewPerspective # Set Up A Perspective View
	glMatrixMode( GL_PROJECTION ) # Select Projection
	glPopMatrix() # Pop The Matrix
	glMatrixMode( GL_MODELVIEW ) # Select Modelview
	glPopMatrix() # Pop The Matrix
end

def normalize(v)
	len = sqrt( v[0]*v[0] + v[1]*v[1] + v[2]*v[2])
	return v if len==0
	[ v[0] / len, v[1] / len, v[2] / len ]
end

def calcNormal(v) # Calculates Normal For A Quad Using 3 Points
	# Finds The Vector Between 2 Points By Subtracting
	# The x,y,z Coordinates From One Point To Another.
	# Calculate The Vector From Point 1 To Point 0
	v1, v2, out = [], [], []
	x,y,z = 0,1,2
	
	v1[x] = v[0][x] - v[1][x] # Vector 1.x=Vertex[0].x-Vertex[1].x 
	v1[y] = v[0][y] - v[1][y] # Vector 1.y=Vertex[0].y-Vertex[1].y 
	v1[z] = v[0][z] - v[1][z]	# Vector 1.z=Vertex[0].y-Vertex[1].z 
	# Calculate The Vector From Point 2 To Point 1 
	v2[x] = v[1][x] - v[2][x]	# Vector 2.x=Vertex[0].x-Vertex[1].x 
	v2[y] = v[1][y] - v[2][y]	# Vector 2.y=Vertex[0].y-Vertex[1].y 
	v2[z] = v[1][z] - v[2][z]	# Vector 2.z=Vertex[0].z-Vertex[1].z 
	# Compute The Cross Product To Give Us A Surface Normal 
	out[x] = v1[y]*v2[z] - v1[z]*v2[y] # Cross Product For Y - Z 
	out[y] = v1[z]*v2[x] - v1[x]*v2[z] # Cross Product For X - Z 
	out[z] = v1[x]*v2[y] - v1[y]*v2[x] # Cross Product For X - Y 

	normalize(out)
end

def createHelix() # creates helix VA
	twists = 5
	r = 1.5

	helix_v = []
	helix_n = []
	
	0.step(360,20) do |phi| # 360 Degrees In Steps Of 20
		0.step(360*twists,20) do |theta| # 360 Degrees * Number Of Twists In Steps Of 20
			v= phi/180.0*PI # Calculate Angle Of First Point	(  0 ) 
			u= theta/180.0*PI # Calculate Angle Of First Point	(  0 ) 

			x= cos(u)*(2.0+cos(v))*r # Calculate x Position (1st Point) 
			y= sin(u)*(2.0+cos(v))*r	# Calculate y Position (1st Point) 
			z=((u-(2.0*PI)) + sin(v))*r	# Calculate z Position (1st Point) 

			v0 = [x,y,z]

			v= phi/180.0*PI # Calculate Angle Of Second Point	(  0 ) 
			u= (theta+20)/180.0*PI	# Calculate Angle Of Second Point	( 20 ) 

			x= cos(u)*(2.0+cos(v))*r # Calculate x Position (2nd Point)
			y= sin(u)*(2.0+cos(v))*r	# Calculate y Position (2nd Point) 
			z= ((u-(2.0*PI)) + sin(v))*r	# Calculate z Position (2nd Point) 
		
			v1 = [x,y,z]

			v= (phi+20)/180.0*PI # Calculate Angle Of Third Point	( 20 ) 
			u= (theta+20)/180.0*PI # Calculate Angle Of Third Point	( 20 ) 

			x= cos(u)*(2.0+cos(v))*r # Calculate x Position (3rd Point) 
			y= sin(u)*(2.0+cos(v))*r # Calculate y Position (3rd Point) 
			z= ((u-(2.0*PI)) + sin(v))*r	# Calculate z Position (3rd Point) 

			v2 = [x,y,z]

			v= (phi+20)/180.0*PI # Calculate Angle Of Fourth Point	( 20 ) 
			u= (theta)/180.0*PI # Calculate Angle Of Fourth Point	(  0 ) 

			x= cos(u)*(2.0+cos(v))*r # Calculate x Position (4th Point) 
			y= sin(u)*(2.0+cos(v))*r # Calculate y Position (4th Point) 
			z= ((u-(2.0*PI)) + sin(v))*r # Calculate z Position (4th Point) 
	
			v3 = [x,y,z]

			normal = calcNormal([v0,v1,v2,v3]) # Calculate The Quad Normal 
			helix_v << v0 << v1 << v2 << v3
			helix_n << normal << normal << normal << normal
		end
	end
	[helix_v,helix_n]
end

def processHelix() # Draws A Helix 
	glfMaterialColor = [0.4,0.2,0.8,1.0] # Set The Material Color
	specular = [1.0,1.0,1.0,1.0] # Sets Up Specular Lighting

	glLoadIdentity() # Reset The Modelview Matrix
	gluLookAt(0, 5, 50, 0, 0, 0, 0, 1, 0) # Eye Position (0,5,50) Center Of Scene (0,0,0), Up On Y Axis

	glPushMatrix() # Push The Modelview Matrix

	glTranslatef(0,0,-50) # Translate 50 Units Into The Screen
	glRotatef($angle/2.0,1,0,0) # Rotate By angle/2 On The X-Axis
	glRotatef($angle/3.0,0,1,0) # Rotate By angle/3 On The Y-Axis

	glMaterialfv(GL_FRONT_AND_BACK,GL_AMBIENT_AND_DIFFUSE,glfMaterialColor)
	glMaterialfv(GL_FRONT_AND_BACK,GL_SPECULAR,specular)

	glEnableClientState(GL_VERTEX_ARRAY)
	glEnableClientState(GL_NORMAL_ARRAY)
	glDrawArrays(GL_QUADS,0,$helix_v.size)
	glDisableClientState(GL_VERTEX_ARRAY)
	glDisableClientState(GL_NORMAL_ARRAY)

	glPopMatrix() # Pop The Matrix
end

def drawBlur(times,inc)
	spost = 0.0 # Starting Texture Coordinate Offset 
	alphainc = 0.9 / times # Fade Speed For Alpha Blending 
	alpha = 0.2	# Starting Alpha Value 

	width = glutGet(GLUT_WINDOW_WIDTH)
	height = glutGet(GLUT_WINDOW_HEIGHT)
	# Disable AutoTexture Coordinates 
	glDisable(GL_TEXTURE_GEN_S)
	glDisable(GL_TEXTURE_GEN_T)

	glEnable(GL_TEXTURE_2D) # Enable 2D Texture Mapping 
	glDisable(GL_DEPTH_TEST) # Disable Depth Testing 
	glBlendFunc(GL_SRC_ALPHA,GL_ONE) # Set Blending Mode 
	glEnable(GL_BLEND) # Enable Blending 
	glBindTexture(GL_TEXTURE_2D,$blurTexture) # Bind To The Blur Texture 
	viewOrtho() # Switch To An Ortho View 

	alphainc = alpha / times # alphainc=0.2 / Times To Render Blur 

	glBegin(GL_QUADS) # Begin Drawing Quads 
	0.upto(times-1) do |num| # Number Of Times To Render Blur 
		glColor4f(1.0, 1.0, 1.0, alpha) # Set The Alpha Value (Starts At 0.2) 
		glTexCoord2f(0+spost,1-spost)	# Texture Coordinate	( 0, 1 ) 
		glVertex2f(0,0)	# First Vertex		(   0,   0 ) 

		glTexCoord2f(0+spost,0+spost) # Texture Coordinate	( 0, 0 ) 
		glVertex2f(0,height) # Second Vertex	(   0, height ) 

		glTexCoord2f(1-spost,0+spost) # Texture Coordinate	( 1, 0 ) 
		glVertex2f(width,height) # Third Vertex		( width, height ) 

		glTexCoord2f(1-spost,1-spost) # Texture Coordinate	( 1, 1 ) 
		glVertex2f(width,0) # Fourth Vertex	( width,   0 ) 

		spost += inc # Gradually Increase spost (Zooming Closer To Texture Center) 
		alpha = alpha - alphainc # Gradually Decrease alpha (Gradually Fading Image Out) 
	end
	glEnd() # Done Drawing Quads 

	viewPerspective() # Switch To A Perspective View 

	glEnable(GL_DEPTH_TEST) # Enable Depth Testing 
	glDisable(GL_TEXTURE_2D) # Disable 2D Texture Mapping 
	glDisable(GL_BLEND) # Disable Blending 
	glBindTexture(GL_TEXTURE_2D,0) # Unbind The Blur Texture 
end


def renderToTexture
	glViewport(0,0,128,128); # Set Our Viewport (Match Texture Size)

	processHelix() # Render The Helix

	glBindTexture(GL_TEXTURE_2D,$blurTexture) # Bind To The Blur Texture

	# Copy Our ViewPort To The Blur Texture (From 0,0 To 128,128... No Border)
	glCopyTexImage2D(GL_TEXTURE_2D, 0, GL_LUMINANCE, 0, 0, 128, 128, 0)

	glClearColor(0.0, 0.0, 0.5, 0.5) # Set The Clear Color To Medium Blue
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT) # Clear The Screen And Depth Buffer
	width = glutGet(GLUT_WINDOW_WIDTH)
	height = glutGet(GLUT_WINDOW_HEIGHT)
	glViewport(0 , 0,width,height) # Set Viewport (0,0 to widthxheight)
end

drawGLScene = lambda do # Draw The Scene
	glClearColor(0.0, 0.0, 0.0, 0.5) # Set The Clear Color To Black
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT) # Clear Screen And Depth Buffer
	glLoadIdentity() # Reset The View
	renderToTexture() # Render To A Texture
	processHelix() # Draw Our Helix
	drawBlur(25,0.02) # Draw The Blur Effect 
	glFlush() # Flush The GL Rendering Pipeline
	glutSwapBuffers()
	sleep(0.001) # don't hog all cpu time
end

idle = lambda do
	now = glutGet(GLUT_ELAPSED_TIME)
	elapsed = now - $lasttime
	$angle += (elapsed * 0.03) # Update angle Based On The Clock 
	$lasttime = now

	glutPostRedisplay()
end

# Main
glutInit()
glutInitDisplayMode(GLUT_RGB | GLUT_DOUBLE | GLUT_ALPHA | GLUT_DEPTH)
glutInitWindowPosition(100,100)
glutInitWindowSize(640,480)
glutCreateWindow("NeHe's Lesson 36")
glutDisplayFunc(drawGLScene)
glutIdleFunc(idle)
glutReshapeFunc(reshape)
glutKeyboardFunc(keyboard)

init()

glutMainLoop()
