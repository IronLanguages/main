# Copyright (C) 2002-2005  3Dlabs Inc. Ltd.
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions
# are met:
#
#    Redistributions of source code must retain the above copyright
#    notice, this list of conditions and the following disclaimer.
#
#    Redistributions in binary form must reproduce the above
#    copyright notice, this list of conditions and the following
#    disclaimer in the documentation and/or other materials provided
#    with the distribution.
#
#    Neither the name of 3Dlabs Inc. Ltd. nor the names of its
#    contributors may be used to endorse or promote products derived
#    from this software without specific prior written permission.
#
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
# "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
# LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
# FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
# COPYRIGHT HOLDERS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
# INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
# BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
# LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
# CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
# LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
# ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
# POSSIBILITY OF SUCH DAMAGE.
# ===========================================================================
#
#
# The source of the shaders itself are in plain text files. Files ending in
# .vert are vertex shaders, and files ending in .frag are fragment shaders.

require 'opengl'
include Gl,Glu,Glut

$rotl = 1 * Math::PI / 180
$last_time = 0

$fXDiff = 206
$fYDiff = 16
$fZDiff = 10
$xLastIncr = 0
$yLastIncr = 0
$fXInertia = -0.5
$fYInertia = 0
$fXInertiaOld = 0
$fYInertiaOld = 0
$fScale = 1.0
$ftime = 0
$xLast = -1
$yLast = -1
$bmModifiers = 0
$rotate = true

INERTIA_THRESHOLD = 1.0
INERTIA_FACTOR = 0.5
SCALE_FACTOR = 0.01
SCALE_INCREMENT = 0.5
TIMER_FREQUENCY_MILLIS = 20


$gleModel = [:cube, :teapot,:torus,:sphere]
$clearColor = [[0,0,0,1], [0.2,0.2,0.3,1], [0.7,0.7,0.7,1]]

def drawCube
	size = 1.0
	scale = 0.2
	delta = 0.1

	v = [
		[ size,  size,  size * scale + delta ], 
		[ size,  size, -size * scale + delta ], 
		[ size, -size, -size * scale ], 
		[ size, -size,  size * scale ],
		[-size,  size,  size * scale + delta ],
		[-size,  size, -size * scale + delta ],
		[-size, -size, -size * scale ],
		[-size, -size,  size * scale ]
	]

	cube = [
		[ [1,0,0], v[3],v[2],v[1],v[0] ], # normal, vertices
		[ [-1,0,0], v[6],v[7],v[4],v[5] ],
		[ [0,0,-1], v[2],v[6],v[5],v[1] ],
		[ [0,0,1], v[7],v[3],v[0],v[4] ],
		[ [0,1,0], v[4],v[0],v[1],v[5] ],
		[ [0,-1,0], v[6],v[2],v[3],v[7] ]
	]

	glBegin(GL_QUADS)
	cube.each do |side|
		glNormal3fv(side[0])

    glTexCoord2f(1,1)
    glVertex3fv(side[1])
    glTexCoord2f(0,1)
    glVertex3fv(side[2])
    glTexCoord2f(0,0)
    glVertex3fv(side[3])
    glTexCoord2f(1,0)
    glVertex3fv(side[4])
	end
	glEnd()
end

def nextClearColor
	glClearColor($clearColor[0][0],
							 $clearColor[0][1],
							 $clearColor[0][2],
							 $clearColor[0][3])
	$clearColor << $clearColor.shift # rotate
end


play = lambda do
	this_time = glutGet(GLUT_ELAPSED_TIME)

	$rotl+=(this_time - $last_time) * -0.001
	$last_time = this_time

	glutPostRedisplay()
end

display = lambda do
	glLoadIdentity()
	glTranslatef(0.0, 0.0, -5.0)
	
	glRotatef($fYDiff, 1,0,0)
	glRotatef($fXDiff, 0,1,0)
	glRotatef($fZDiff, 0,0,1)
	
	glScalef($fScale, $fScale, $fScale)
	
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
	
	case $gleModel[0]
		when :teapot :
			glutSolidTeapot(0.6)
		when :torus :
			glutSolidTorus(0.2, 0.6, 64, 64)
		when :sphere :
			glutSolidSphere(0.6, 64, 64)
		when :cube :
			drawCube()
	end
	glFlush()
	glutSwapBuffers()
end

key = lambda do |key,x,y|
	case(key)
	when ?b
		nextClearColor()
	when ?q, ?\e # esc
		exit(0)
	when ?t
		$gleModel << $gleModel.shift # rotate the array
	when ?\s # space
		$rotate = !$rotate

		if ($rotate==false)
			$fXInertiaOld = $fXInertia
			$fYInertiaOld = $fYInertia
		else
			$fXInertia = $fXInertiaOld
			$fYInertia = $fYInertiaOld

      # To prevent confusion, force some rotation
			if ($fXInertia == 0 && $fYInertia == 0)
				$fXInertia = -0.5
			end
		end
	when ?+
		$fScale += SCALE_INCREMENT
	when ?-
		$fScale -= SCALE_INCREMENT
	else
		puts "Keyboard commands:\n"
		puts "b - Toggle among background clear colors"
		puts "q, <esc> - Quit"
		puts "t - Toggle among models to render"
		puts "? - Help"
		puts "<home>     - reset zoom and rotation"
		puts "<space> or <click>        - stop rotation"
		puts "<+>, <-> or <ctrl + drag> - zoom model"
		puts "<arrow keys> or <drag>    - rotate model\n"
	end
end

reshape = lambda do |w,h|
	vp = 0.8
	aspect = w.to_f/h.to_f
	
	glViewport(0, 0, w, h)
	glMatrixMode(GL_PROJECTION)
	glLoadIdentity()
	
	glFrustum(-vp, vp, -vp / aspect, vp / aspect, 3, 10)
	
	glMatrixMode(GL_MODELVIEW)
	glLoadIdentity()
	glTranslatef(0.0, 0.0, -5.0)
end

motion = lambda do |x,y|
	if ($xLast != -1 || $yLast != -1)
		$xLastIncr = x - $xLast
		$yLastIncr = y - $yLast
		if ($bmModifiers & GLUT_ACTIVE_CTRL != 0)
			if ($xLast != -1)
				$fZDiff += $xLastIncr
				$fScale += $yLastIncr*SCALE_FACTOR
			end
		else
			if ($xLast != -1)
				$fXDiff += $xLastIncr
				$fYDiff += $yLastIncr
			end
		end
	end
	$xLast = x
	$yLast = y
end

mouse = lambda do |button,state,x,y|
	$bmModifiers = glutGetModifiers()
	if (button == GLUT_LEFT_BUTTON)
		if (state == GLUT_UP)
			$xLast = -1
			$yLast = -1
			if $xLastIncr > INERTIA_THRESHOLD
				$fXInertia = ($xLastIncr - INERTIA_THRESHOLD)*INERTIA_FACTOR
			end
			if -$xLastIncr > INERTIA_THRESHOLD
				$fXInertia = ($xLastIncr + INERTIA_THRESHOLD)*INERTIA_FACTOR
			end

			if $yLastIncr > INERTIA_THRESHOLD
				$fYInertia = ($yLastIncr - INERTIA_THRESHOLD)*INERTIA_FACTOR
			end
			if -$yLastIncr > INERTIA_THRESHOLD
				$fYInertia = ($yLastIncr + INERTIA_THRESHOLD)*INERTIA_FACTOR
			end
		else
			$fXInertia = 0
			$fYInertia = 0
		end
		$xLastIncr = 0
		$yLastIncr = 0
	end
end

special = lambda do |key,x,y|
	case key
	when GLUT_KEY_HOME:
		$fXDiff = 0
		$fYDiff = 35
		$fZDiff = 0
		$xLastIncr = 0
		$yLastIncr = 0
		$fXInertia = -0.5
		$fYInertia = 0
		$fScale = 1.0
	when GLUT_KEY_LEFT:
		$fXDiff -= 1
	when GLUT_KEY_RIGHT:
		$fXDiff += 1
	when GLUT_KEY_UP:
		$fYDiff -= 1
	when GLUT_KEY_DOWN:
		$fYDiff += 1
	end
end

timer = lambda do |value|
	$ftime += 0.01
	if $rotate
		$fXDiff += $fXInertia
		$fYDiff += $fYInertia
	end
	glutTimerFunc(TIMER_FREQUENCY_MILLIS , timer, 0)
end

def getUniLoc(program, name)
	loc = glGetUniformLocation(program, name)
	
	if (loc == -1)
		puts "No such uniform named #{name}"
	end
	return loc
end

def installBrickShaders(vs_fname,fs_fname)
	# Create a vertex shader object and a fragment shader object
	brickVS = glCreateShader(GL_VERTEX_SHADER)
	brickFS = glCreateShader(GL_FRAGMENT_SHADER)

	# Load source code strings into shaders
	glShaderSource(brickVS, File.read(vs_fname))
	glShaderSource(brickFS, File.read(fs_fname))

	# Compile the brick vertex shader, and print out
	#	the compiler log file.
	glCompileShader(brickVS)
  vertCompiled = glGetShaderiv(brickVS, GL_COMPILE_STATUS)
	puts "Shader InfoLog:\n#{glGetShaderInfoLog(brickVS)}\n"
	
	# Compile the brick fragment shader, and print out
	# the compiler log file.
	glCompileShader(brickFS)
  fragCompiled = glGetShaderiv(brickFS, GL_COMPILE_STATUS)
	puts "Shader InfoLog:\n#{glGetShaderInfoLog(brickFS)}\n"
	
	return false if (vertCompiled == 0 || fragCompiled == 0)
	# Create a program object and attach the two compiled shaders
	brickProg = glCreateProgram()
	glAttachShader(brickProg,brickVS)
	glAttachShader(brickProg,brickFS)
	# Link the program object and print out the info log
	glLinkProgram(brickProg)
	linked = glGetProgramiv(brickProg,GL_LINK_STATUS)
	puts "Program InfoLog:\n#{glGetProgramInfoLog(brickProg)}\n"
	return false if linked==0

	# Install program object as part of current state
	glUseProgram(brickProg)

	# Set up initial uniform values
	glUniform3f(getUniLoc(brickProg, "BrickColor"), 1.0, 0.3, 0.2)
	glUniform3f(getUniLoc(brickProg, "MortarColor"), 0.85, 0.86, 0.84)
	glUniform2f(getUniLoc(brickProg, "BrickSize"), 0.30, 0.15)
	glUniform2f(getUniLoc(brickProg, "BrickPct"), 0.90, 0.85)
	glUniform3f(getUniLoc(brickProg, "LightPosition"), 0.0, 0.0, 4.0)

	return true
end

# Main

glutInit()
glutInitDisplayMode( GLUT_RGB | GLUT_DEPTH | GLUT_DOUBLE)
glutInitWindowPosition(100,100)
glutInitWindowSize(500, 500)
glutCreateWindow( "3Dlabs Brick Shader")

glutIdleFunc(play)
glutDisplayFunc(display)
glutKeyboardFunc(key)
glutReshapeFunc(reshape)
glutMotionFunc(motion)
glutMouseFunc(mouse)
glutSpecialFunc(special)
glutTimerFunc(TIMER_FREQUENCY_MILLIS , timer, 0)
 
# Make sure that OpenGL 2.0 is supported by the driver
if Gl.is_available?(2.0)==false
	major,minor,*rest = glGetString(GL_VERSION).split(/\.| /)
	puts "GL_VERSION major=#{major} minor=#{minor}"
	puts "Support for OpenGL 2.0 is required for this demo...exiting"
	exit(1)
end

glDepthFunc(GL_LESS)
glEnable(GL_DEPTH_TEST)

nextClearColor()

key.call('?', 0, 0)
 
success = installBrickShaders("brick.vert","brick.frag")
glutMainLoop() if success == true
