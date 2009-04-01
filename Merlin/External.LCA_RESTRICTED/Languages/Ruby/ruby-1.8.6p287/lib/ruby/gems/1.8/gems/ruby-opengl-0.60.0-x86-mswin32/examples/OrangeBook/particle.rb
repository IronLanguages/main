#
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
require 'opengl'
include Gl,Glu,Glut

$particleTime = 0

# Movement variables
$fXDiff = 206
$fYDiff = 16
$fZDiff = 10
$xLastIncr = 0
$yLastIncr = 0
$fXInertia = -0.5
$fYInertia = 0
$fXInertiaOld
$fYInertiaOld
$fScale = 0.25
$ftime = 0
$xLast = -1
$yLast = -1
$bmModifiers = 0
$rotate = 1

$programObject = 0
$vertexShaderObject = 0
$fragmentShaderObject = 0

# Rotation defines
INERTIA_THRESHOLD = 1.0
INERTIA_FACTOR = 0.5
SCALE_FACTOR = 0.01
SCALE_INCREMENT = 0.5
TIMER_FREQUENCY_MILLIS = 20

VELOCITY_ARRAY = 3
START_TIME_ARRAY = 4

$clearColor = [[0,0,0,1], [0.2,0.2,0.3,1], [0.7,0.7,0.7,1]]

$arrayWidth = 0, $arrayHeight = 0
$verts = []
$colors = []
$velocities = []
$startTimes = []

def nextClearColor
	glClearColor($clearColor[0][0],
							 $clearColor[0][1],
							 $clearColor[0][2],
							 $clearColor[0][3])
	$clearColor << $clearColor.shift # rotate
end

def getUniLoc(program, name)
	loc = glGetUniformLocation(program, name)
	
	if (loc == -1)
		puts "No such uniform named #{name}"
	end
	return loc
end

def updateAnim
	location = getUniLoc($programObject,"Time")

	$particleTime += 0.002
	if $particleTime > 15.0
		$particleTime = 0.0
	end

	glUniform1f(location,$particleTime)
end

def drawPoints
	glPointSize(2.0)
	glVertexPointer(3, GL_FLOAT, 0, $verts)
	glColorPointer(3, GL_FLOAT, 0, $colors)
	glVertexAttribPointer(VELOCITY_ARRAY,  3, GL_FLOAT, GL_FALSE, 0, $velocities)
	glVertexAttribPointer(START_TIME_ARRAY, 1, GL_FLOAT, GL_FALSE, 0, $startTimes)

	glEnableClientState(GL_VERTEX_ARRAY)
	glEnableClientState(GL_COLOR_ARRAY)
	glEnableVertexAttribArray(VELOCITY_ARRAY)
	glEnableVertexAttribArray(START_TIME_ARRAY)

	glDrawArrays(GL_POINTS, 0, $arrayWidth * $arrayHeight)

	glDisableClientState(GL_VERTEX_ARRAY)
	glDisableClientState(GL_COLOR_ARRAY)
	glDisableVertexAttribArray(VELOCITY_ARRAY)
	glDisableVertexAttribArray(START_TIME_ARRAY)
end

def createPoints(w,h)
	$verts  = []
	$colors = []
	$velocities = []
	$startTimes = []

	i = 0.5 / w - 0.5
	while (i<0.5)
		j = 0.5 / h - 0.5
		while (j<0.5)
			$verts << i
			$verts << 0.0
			$verts << j
		
			$colors << rand() * 0.5 + 0.5
			$colors << rand() * 0.5 + 0.5
			$colors << rand() * 0.5 + 0.5

			$velocities << rand() + 3.0
			$velocities << rand() * 10.0
			$velocities << rand() + 3.0

			$startTimes << rand() * 10.0

			j += 1.0/h
		end
		i += 1.0/w
	end
	# convert from ruby Array to memory representation of float data that
	# will be passed as array pointers to GL
	$verts = $verts.pack("f*")
	$colors = $colors.pack("f*")
	$velocities = $velocities.pack("f*")
	$startTimes = $startTimes.pack("f*")

	$arrayWidth = w
	$arrayHeight = h
end

play = lambda do
	thisTime = glutGet(GLUT_ELAPSED_TIME)
	updateAnim()
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
	
	drawPoints()
	
	glFlush()
	glutSwapBuffers()
end

key = lambda do |key,x,y|
	$particleTime = 0

	case(key)
	when ?b
		nextClearColor()
	when ?q, ?\e # esc
		exit(0)
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
		puts "? - Help"
		puts "<home>     - reset zoom and rotation"
		puts "<space> or <click>        - stop rotation"
		puts "<+>, <-> or <ctrl + drag> - zoom model"
		puts "<arrow keys> or <drag>    - rotate model\n"
	end
end

reshape = lambda do |w,h|
    vp = 0.8
		aspect = w/h

    glViewport(0, 0, w, h)
    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()

    glFrustum(-vp, vp, -vp / aspect, vp / aspect, 3, 10.0)

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
		$fXDiff = 206
		$fYDiff = 16
		$fZDiff = 10
		$xLastIncr = 0
		$yLastIncr = 0
		$fXInertia = -0.5
		$fYInertia = 0
		$fScale = 0.25
		$particleTime = 0
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

def installParticleShaders(vs_name,fs_name)
	# Create a vertex shader object and a fragment shader object
	$vertexShaderObject = glCreateShader(GL_VERTEX_SHADER)
	$fragmentShaderObject = glCreateShader(GL_FRAGMENT_SHADER)

	# Load source code strings into shaders
	glShaderSource($vertexShaderObject, File.read(vs_name))
	glShaderSource($fragmentShaderObject, File.read(fs_name))

	# Compile the particle vertex shader, and print out
	# the compiler log file.
	glCompileShader($vertexShaderObject)
  vertCompiled = glGetShaderiv($vertexShaderObject, GL_COMPILE_STATUS)
	puts "Shader InfoLog:\n#{glGetShaderInfoLog($vertexShaderObject)}\n"

	# Compile the particle fragment shader, and print out
	# the compiler log file.
	glCompileShader($fragmentShaderObject)
  fragCompiled = glGetShaderiv($fragmentShaderObject, GL_COMPILE_STATUS)
	puts "Shader InfoLog:\n#{glGetShaderInfoLog($fragmentShaderObject)}\n"

	return false if (vertCompiled == 0 || fragCompiled == 0)

	# Create a program object and attach the two compiled shaders
	$programObject = glCreateProgram()
	glAttachShader($programObject, $vertexShaderObject)
	glAttachShader($programObject, $fragmentShaderObject)

	#Bind generic attribute indices to attribute variable names
	glBindAttribLocation($programObject, VELOCITY_ARRAY, "Velocity")
	glBindAttribLocation($programObject, START_TIME_ARRAY, "StartTime");

	# Link the program object and print out the info log
	glLinkProgram($programObject)
	linked = glGetProgramiv($programObject, GL_LINK_STATUS)
	puts "Program InfoLog:\n#{glGetProgramInfoLog($programObject)}\n"

	return false if linked==0

	# Install program object as part of current state
	glUseProgram($programObject)

	# Set up initial uniform values
	glUniform4f(getUniLoc($programObject, "Background"), 0.0, 0.0, 0.0, 1.0)
	glUniform1f(getUniLoc($programObject, "Time"), -5.0)

	return true
end

# Main
glutInit()
glutInitDisplayMode( GLUT_RGB | GLUT_DEPTH | GLUT_DOUBLE)
glutInitWindowSize(500, 500)
glutInitWindowPosition(100,100)
window = glutCreateWindow("3Dlabs OpenGL Shading Language Particle System Demo")
    
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

createPoints(100, 100)

glDepthFunc(GL_LESS)
glEnable(GL_DEPTH_TEST)
nextClearColor()

key.call('?', 0, 0)

success = installParticleShaders("particle.vert", "particle.frag")
if (success)
	glutMainLoop()
end
