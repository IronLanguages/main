# ogl_bench v1.0 - Copyright 2007 - Graphcomp
# Bob Free bfree@graphcomp.com
# http://graphcomp.com/opengl

# This program is freely distributable without licensing fees 
# and is provided without guarantee or warrantee expressed or 
# implied. This program is -not- in the public domain.
#
# Conversion to ruby by Jan Dvorak <jan.dvorak@kraxnet.cz>

# Set up standard libs
require 'opengl'
include Gl,Glut

# Set up constants
PROGRAM = "OpenGL Benchmark - Ruby Bindings"
CYCLES = 1000

# Setup OpenGL Extensions
# (Not needed)

# Set up types
class Bench
	attr_accessor :start,:secs
	def initialize
		@start = 0
		@secs = 0
	end
end

# Set up globals
$appBench = Bench.new
$frameBench = Bench.new
$textureBench = Bench.new
$teapotBench = Bench.new
$now = 0
$frames = 0

$idWindow = 0

$windowWidth = 512
$windowHeight = 512

$textureWidth = 128
$textureHeight = 128

$rotTeapotX = 0
$rotTeapotY = 0

$idTexture = 0
$idFrameBuffer = 0
$idRenderBuffer = 0
$idVertexProg = 0
$idFragProg = 0

$incY = 0.5
$rotY = 0.0

# Start benchmark
def startBench(pBench)
	pBench.start = Time.now
#	pBench.secs = 0.0
end

# Accumulate benchmark
def endBench(pBench)
	$now = Time.now
	pBench.secs += $now - pBench.start
end

# Print benchmark
def printBench
  if ($frames==0 || $appBench.secs==0 || $frameBench.secs==0 ||
    $textureBench.secs==0 || $teapotBench.secs==0)
    puts "No measurable time has elapsed"
    return
  end
  
  puts "FBO Texture Rendering FPS: #{$frames / $textureBench.secs}"
  puts "Teapot Shader FPS: #{$frames / $teapotBench.secs}"

  overhead = $frameBench.secs - ($textureBench.secs + $teapotBench.secs)
  puts "Frame overhead secs/frame: #{overhead / $frames}"

  overhead = $appBench.secs - $frameBench.secs
  puts "OS/GLUT overhead secs/frame: #{overhead / $frames}"

  puts "Overall FPS: #{$frames / $appBench.secs}"
	puts ""
end

# Error handling
def error(errTitle, errMsg)
  puts "#{errTitle}: #{errMsg}"
  exit(0)
end

# Check OpenGL Version
def checkVersion
  version = glGetString(GL_VERSION)
  vendor = glGetString(GL_VENDOR)
  renderer = glGetString(GL_RENDERER)
  exts = glGetString(GL_EXTENSIONS)

	puts PROGRAM
	puts ""
	puts "OpenGL: #{version}"
	puts "Vendor: #{vendor}"
	puts "Renderer: #{renderer}"
	puts ""

	if Gl.is_available?("GL_EXT_framebuffer_object") == false
    error("Extension not available","EXT_framebuffer_object")
	end
end

# Load Extension Procs
def initExtensions
	# (Not needed)
end

# Initialize Vertex/Fragment Programs
def initProgs
  # NOP Vertex shader
	$vertexProg = <<-END_OF_PROGRAM
!!ARBvp1.0
TEMP vertexClip;
DP4 vertexClip.x, state.matrix.mvp.row[0], vertex.position;
DP4 vertexClip.y, state.matrix.mvp.row[1], vertex.position;
DP4 vertexClip.z, state.matrix.mvp.row[2], vertex.position;
DP4 vertexClip.w, state.matrix.mvp.row[3], vertex.position;
MOV result.position, vertexClip;
MOV result.color, vertex.color;
MOV result.texcoord[0], vertex.texcoord;
MOV result.texcoord[1], vertex.normal;
END
	END_OF_PROGRAM

  # Black Light Fragment shader
	$fragProg = <<-END_OF_PROGRAM
!!ARBfp1.0
TEMP decal,color;
TEX decal, fragment.texcoord[0], texture[0], 2D;
MUL result.color, decal, fragment.texcoord[1];
END
	END_OF_PROGRAM

  $idVertexProg,$idFragProg = glGenProgramsARB(2)

  glBindProgramARB(GL_VERTEX_PROGRAM_ARB, $idVertexProg)
  glProgramStringARB(GL_VERTEX_PROGRAM_ARB, GL_PROGRAM_FORMAT_ASCII_ARB, $vertexProg)

  glBindProgramARB(GL_FRAGMENT_PROGRAM_ARB, $idFragProg)
  glProgramStringARB(GL_FRAGMENT_PROGRAM_ARB, GL_PROGRAM_FORMAT_ASCII_ARB, $fragProg)
end

# Terminate Vertex/Fragment Programs
def termProgs
  glBindProgramARB(GL_VERTEX_PROGRAM_ARB, 0)
  glBindProgramARB(GL_FRAGMENT_PROGRAM_ARB, 0)

  glDeleteProgramsARB([$idVertexProg,$idFragProg])
end

# FBO Status handler
def statusFBO
  stat = glCheckFramebufferStatusEXT(GL_FRAMEBUFFER_EXT)
  return if (stat==0 || stat == GL_FRAMEBUFFER_COMPLETE_EXT)
  printf("FBO status: %04X\n", stat)
  exit(0)
end

# Initialize Framebuffers
def initFBO
  $idTexture = glGenTextures(1)[0]
  $idFrameBuffer = glGenFramebuffersEXT(1)[0]
  $idRenderBuffer = glGenRenderbuffersEXT(1)[0]

  glBindFramebufferEXT(GL_FRAMEBUFFER_EXT, $idFrameBuffer)
  glBindTexture(GL_TEXTURE_2D, $idTexture)

  glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, $textureWidth, $textureHeight,
		0, GL_RGBA, GL_UNSIGNED_BYTE, nil)

  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR)
  glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR)

  glFramebufferTexture2DEXT(GL_FRAMEBUFFER_EXT, GL_COLOR_ATTACHMENT0_EXT,
    GL_TEXTURE_2D, $idTexture, 0)

  glBindRenderbufferEXT(GL_RENDERBUFFER_EXT, $idRenderBuffer)

  glRenderbufferStorageEXT(GL_RENDERBUFFER_EXT, GL_DEPTH_COMPONENT24,
		$textureWidth, $textureHeight)

  glFramebufferRenderbufferEXT(GL_FRAMEBUFFER_EXT, GL_DEPTH_ATTACHMENT_EXT,
      GL_RENDERBUFFER_EXT, $idRenderBuffer)

  statusFBO()
end

# FBO texture renderer
def renderFBO
  glBindFramebufferEXT(GL_FRAMEBUFFER_EXT, $idFrameBuffer)

  glLoadIdentity()
  glTranslated(-0.75, -0.85, -2.5)

  glRotated($rotTeapotX, 1.0, 0.0, 0.0)
  $rotTeapotX += 0.5

  glRotated($rotTeapotY, 0.0, 1.0, 0.0)
  $rotTeapotY += 1.0

  glClearColor(0, 0, 0, 0)
  glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
  glColor3d(1.0, 1.0, 1.0)
 
  glutWireTeapot(0.125)

  glBindFramebufferEXT(GL_FRAMEBUFFER_EXT, 0)
end

# Terminate FBO objects
def termFBO
  glBindRenderbufferEXT(GL_RENDERBUFFER_EXT, 0)
  glBindFramebufferEXT(GL_FRAMEBUFFER_EXT, 0)
  glBindTexture(GL_TEXTURE_2D, 0)

  glDeleteRenderbuffersEXT($idRenderBuffer)
  glDeleteFramebuffersEXT($idFrameBuffer)
  glDeleteTextures($idTexture)
end

# Resize Window
def resizeScene(width,height)
  if (height==0)
		height = 1
  end
  
  glViewport(0, 0, width, height)

  glMatrixMode(GL_PROJECTION)
  glLoadIdentity()
  gluPerspective(45.0,width.to_f/height.to_f,0.1,100.0)

  glMatrixMode(GL_MODELVIEW)

  $windowWidth  = width
  $windowHeight = height
end

# Initialize OpenGL Environment
def init
  checkVersion
  initExtensions

  resizeScene($windowWidth, $windowHeight)

  initFBO
  initProgs
end

# Terminate OpenGL Environment
def term
  # Display benchmark
  endBench($appBench)
  printBench()

  # Disable app
  glutHideWindow()
  glutKeyboardFunc(nil)
  glutSpecialFunc(nil)
  glutIdleFunc(nil)
  glutReshapeFunc(nil)

  # Release Framebuffers
  termProgs()
  termFBO()

  # Now we can destroy window
  glutDestroyWindow($idWindow)
  exit(0)
end

# Frame handler
display = lambda do
  # Run benchmark CYCLES times
	$frames += 1
  term() if ($frames > CYCLES)
  startBench($frameBench)

  # Render animated texture
  startBench($textureBench)
  renderFBO()
  endBench($textureBench)
  
  # Set up ModelView
  glMatrixMode(GL_MODELVIEW)
  glLoadIdentity()
  glTranslatef(0.0,0.0,-5.0)
  glRotated(0.0,1.0,0.0,0.0)
  glRotated($rotY,0.0,1.0,0.0)
  $rotY += $incY

  # Set attributes
  glEnable(GL_TEXTURE_2D)
  glEnable(GL_DEPTH_TEST)
  glTexEnvi(GL_TEXTURE_ENV,GL_TEXTURE_ENV_MODE,GL_DECAL)

  # Clear render buffer and set teapot color
  glClearColor(0.2, 0.2, 0.2, 1.0)
  glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
  glColor3d(0.9, 0.45, 0.0)

  # Render the teapot using our shader
  startBench($teapotBench)
  glEnable(GL_VERTEX_PROGRAM_ARB)
  glEnable(GL_FRAGMENT_PROGRAM_ARB)
 
  glutSolidTeapot(1.0)

  glDisable(GL_FRAGMENT_PROGRAM_ARB)
  glDisable(GL_VERTEX_PROGRAM_ARB)
  endBench($teapotBench)

  # Double-buffer and done
  glutSwapBuffers()
  endBench($frameBench)
end

# Keyboard handler
keyPressed = lambda do |key,x,y|
  term()
end



# Main app

glutInit()
glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGBA | GLUT_DEPTH | GLUT_ALPHA)
glutInitWindowSize($windowWidth,$windowHeight)

$idWindow = glutCreateWindow(PROGRAM)

glutDisplayFunc(display)
glutIdleFunc(display)
#glutReshapeFunc(resizeScene)
glutKeyboardFunc(keyPressed)

init()
startBench($appBench)
glutMainLoop()
exit(0)

