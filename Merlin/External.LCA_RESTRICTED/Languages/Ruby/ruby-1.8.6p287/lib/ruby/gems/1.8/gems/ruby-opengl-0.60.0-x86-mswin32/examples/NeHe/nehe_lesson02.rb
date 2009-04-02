#!/usr/bin/env ruby -rubygems
# Name:  nehe_lesson02.rb
# Purpose: An implementation of NeHe's OpenGL Lesson #02
#          using ruby-opengl (http://nehe.gamedev.net/)
# 

require "gl"
require "glu"
require "glut"
require "mathn"

# Add GL and GLUT namespaces in to make porting easier
include Gl
include Glu
include Glut

# Placeholder for the window object
window = ""

def init_gl_window(width = 640, height = 480)
    # Background color to black
    glClearColor(0.0, 0.0, 0.0, 0)
    # Enables clearing of depth buffer
    glClearDepth(1.0)
    # Set type of depth test
    glDepthFunc(GL_LEQUAL)
    # Enable depth testing
    glEnable(GL_DEPTH_TEST)
    # Enable smooth color shading
    glShadeModel(GL_SMOOTH)

    glMatrixMode(GL_PROJECTION)
    glLoadIdentity
    # Calculate aspect ratio of the window
    gluPerspective(45.0, width / height, 0.1, 100.0)

    glMatrixMode(GL_MODELVIEW)

    draw_gl_scene
end

#reshape = Proc.new do |width, height|
def reshape(width, height)
    height = 1 if height == 0

    # Reset current viewpoint and perspective transformation
    glViewport(0, 0, width, height)

    glMatrixMode(GL_PROJECTION)
    glLoadIdentity

    gluPerspective(45.0, width / height, 0.1, 100.0)
end

#draw_gl_scene = Proc.new do
def draw_gl_scene
    # Clear the screen and depth buffer
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)

    # Reset the view
    glMatrixMode(GL_MODELVIEW)
    glLoadIdentity

    # Move left 1.5 units and into the screen 6.0 units
    glTranslatef(-1.5, 0.0, -6.0)

    # Draw a triangle
    glBegin(GL_POLYGON)
        glVertex3f( 0.0,  1.0, 0.0)
        glVertex3f( 1.0, -1.0, 0.0)
        glVertex3f(-1.0, -1.0, 0.0)
    glEnd

    # Move right 3 units
    glTranslatef(3.0, 0.0, 0.0)

    # Draw a rectangle
    glBegin(GL_QUADS)
        glVertex3f(-1.0,  1.0, 0.0)
        glVertex3f( 1.0,  1.0, 0.0)
        glVertex3f( 1.0, -1.0, 0.0)
        glVertex3f(-1.0, -1.0, 0.0)
    glEnd

    # Swap buffers for display 
    glutSwapBuffers
end

# The idle function to handle 
def idle
    glutPostRedisplay
end

# Keyboard handler to exit when ESC is typed
keyboard = lambda do |key, x, y|
    case(key)
      when ?\e
          glutDestroyWindow(window)
          exit(0)
      end
      glutPostRedisplay
end


# Initliaze our GLUT code
glutInit;
# Setup a double buffer, RGBA color, alpha components and depth buffer
glutInitDisplayMode(GLUT_RGB | GLUT_DOUBLE | GLUT_ALPHA | GLUT_DEPTH);
glutInitWindowSize(640, 480);
glutInitWindowPosition(0, 0);
window = glutCreateWindow("NeHe Lesson 02 - ruby-opengl version");
glutDisplayFunc(method(:draw_gl_scene).to_proc);
glutReshapeFunc(method(:reshape).to_proc);
glutIdleFunc(method(:idle).to_proc);
glutKeyboardFunc(keyboard);
init_gl_window(640, 480)
glutMainLoop();
