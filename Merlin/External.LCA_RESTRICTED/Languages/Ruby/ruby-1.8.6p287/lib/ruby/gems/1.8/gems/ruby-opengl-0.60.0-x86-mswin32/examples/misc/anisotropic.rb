#
# Copyright (C) 2007 Jan Dvorak <jan.dvorak@kraxnet.cz>
#
# This program is distributed under the terms of the MIT license.
# See the included MIT-LICENSE file for the terms of this license.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
# OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
# IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
# CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
# TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
# SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#

#
# Showcase for anisotropic texture filtering
#

require 'opengl'
include Gl,Glu,Glut

# extend Array class with new function
class Array
	def rotate!
		self << self.shift
	end
end

class App
	@@filters = [[GL_NEAREST,"None"],[GL_LINEAR_MIPMAP_NEAREST,"Bilinear"],[GL_LINEAR_MIPMAP_LINEAR,"Trilinear"]]
	@@anisotropy = [1,2,4,8,16,32]
	@@color_tint = ["Off","On"]

	def checker_texture(size,divide,color_a,color_b)
		strip_a = color_a * (size/divide)
		strip_b = color_b * (size/divide)
		line_strip_a = (strip_a + strip_b) * (size/2)
		line_strip_b = (strip_b + strip_a) * (size/2)
		(line_strip_a + line_strip_b) * (divide/2)
	end

	def printGlutBitmapFont(string, font, x,y, r,g,b)
		glDisable(GL_TEXTURE_2D)
		glColor3f(r, g, b)
		glRasterPos2i(x, y)
		string.each_byte do |x|
			glutBitmapCharacter(font, x)
		end
	end

	def ortho(w,h)
		glMatrixMode(GL_PROJECTION)
		glLoadIdentity()
		gluOrtho2D(0,w,0,h)
		glScalef(1,-1,1)
		glTranslatef(0,-h,0)
	
		glMatrixMode(GL_MODELVIEW)
		glLoadIdentity()
	end

	def persp(w,h)
		glMatrixMode(GL_PROJECTION)
		glLoadIdentity
		gluPerspective(90,w.to_f/h.to_f,1,100)

		glMatrixMode(GL_MODELVIEW)
		glLoadIdentity
	end

	def reshape(w,h)
		@@w,@@h = w,h
		glViewport(0, 0, w, h)
		persp(w,h)
	end

	def initialize
		if (not Gl.is_available?("GL_EXT_texture_filter_anisotropic"))
			puts "This program needs GL_EXT_texture_filter_anisotropic extension"
			exit
		end
		@@w,@@h = glutGet(GLUT_WINDOW_WIDTH),glutGet(GLUT_WINDOW_HEIGHT)

		@t = glGenTextures(2)

		# default checkerboard texture
		glBindTexture(GL_TEXTURE_2D,@t[0])
		data = checker_texture(64,4,[1,1,1],[0,0,0])
		gluBuild2DMipmaps(GL_TEXTURE_2D,GL_RGBA,64,64,GL_RGB,GL_FLOAT,data.pack("f*"))

		# second texture with color tinted mipmaps
		glBindTexture(GL_TEXTURE_2D,@t[1])
		data = checker_texture(64,4,[1,1,1],[0,0,0])
		glTexImage2D(GL_TEXTURE_2D,0,GL_RGBA,64,64,0,GL_RGB,GL_FLOAT,data.pack("f*"))
		data = checker_texture(32,4,[1,0,0],[0,0,0])
		glTexImage2D(GL_TEXTURE_2D,1,GL_RGBA,32,32,0,GL_RGB,GL_FLOAT,data.pack("f*"))
		data = checker_texture(16,4,[0,1,0],[0,0,0])
		glTexImage2D(GL_TEXTURE_2D,2,GL_RGBA,16,16,0,GL_RGB,GL_FLOAT,data.pack("f*"))
		data = checker_texture(8,4,[0,0,1],[0,0,0])
		glTexImage2D(GL_TEXTURE_2D,3,GL_RGBA,8,8,0,GL_RGB,GL_FLOAT,data.pack("f*"))
		data = checker_texture(4,4,[1,1,0],[0,0,0])
		glTexImage2D(GL_TEXTURE_2D,4,GL_RGBA,4,4,0,GL_RGB,GL_FLOAT,data.pack("f*"))
		data = checker_texture(2,2,[1,0,1],[0,0,0])
		glTexImage2D(GL_TEXTURE_2D,5,GL_RGBA,2,2,0,GL_RGB,GL_FLOAT,data.pack("f*"))
		data = [0.5,0.5,0.5] # single pixel texture, just average it
		glTexImage2D(GL_TEXTURE_2D,6,GL_RGBA,1,1,0,GL_RGB,GL_FLOAT,data.pack("f*"))
	end

	def display_text
		ortho(@@w,@@h)
		printGlutBitmapFont("Texture Filtering ('f'): #{@@filters[0][1]}", GLUT_BITMAP_9_BY_15, 20, 20, 1.0, 1.0, 1.0)
		printGlutBitmapFont("Anisotropy factor ('a'): #{@@anisotropy[0]}x", GLUT_BITMAP_9_BY_15, 20, 40, 1.0, 1.0, 1.0)
		printGlutBitmapFont("Colored Mipmaps ('c'): #{@@color_tint[0]}", GLUT_BITMAP_9_BY_15, 20, 60, 1.0, 1.0, 1.0)
		persp(@@w,@@h)
	end

	def display_plane()
		glEnable(GL_TEXTURE_2D)
		t_repeat = 16
		# x,y,z,u,v
		quad = [[-4,-1,1,  0,t_repeat],[4,-1,1,  t_repeat,t_repeat],[4,1,-8,  t_repeat,0],[-4,1,-8,  0,0]]
		glBegin(GL_QUADS)
		quad.each do |v|
			glTexCoord2f(v[3],v[4])
			glVertex3f(v[0],v[1],v[2])
		end
		glEnd()
		glDisable(GL_TEXTURE_2D)
	end

	def display
		glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

		persp(@@w,@@h)

		# move back from scene
		glTranslatef(0,0,-2)

		# set anisotropy
		glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MAX_ANISOTROPY_EXT,@@anisotropy[0])

		# set color tint
		if (@@color_tint[0] == "On")
			glBindTexture(GL_TEXTURE_2D,@t[1])
		else			
			glBindTexture(GL_TEXTURE_2D,@t[0])
		end

		# set filters
		f = @@filters[0][0]
		glTexParameterf(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,f)

		# draw scene	
		display_plane()
		display_text()

		sleep(0.001) # microsleep to avoid consuming all CPU time
		glutSwapBuffers()
	end

	def idle
		glutPostRedisplay()
	end

	def keyboard(key,x,y)
		case (key)
		when ?f
			@@filters.rotate!
		when ?a
			max_anisotropy = glGetIntegerv(GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT)
			begin	@@anisotropy.rotate! end until @@anisotropy[0]<=max_anisotropy
		when ?c
			@@color_tint.rotate!
		when ?\e # Escape
			exit(0)
		end
		glutPostRedisplay()
	end
end

# main
glutInit()
glutInitDisplayMode(GLUT_RGBA | GLUT_DEPTH | GLUT_DOUBLE )
glutInitWindowPosition(150,50)
glutInitWindowSize(500,500)
glutCreateWindow($0)

app = App.new
glutReshapeFunc(app.method(:reshape).to_proc)
glutDisplayFunc(app.method(:display).to_proc)
glutIdleFunc(app.method(:idle).to_proc)
glutKeyboardFunc(app.method(:keyboard).to_proc)
glutMainLoop()
