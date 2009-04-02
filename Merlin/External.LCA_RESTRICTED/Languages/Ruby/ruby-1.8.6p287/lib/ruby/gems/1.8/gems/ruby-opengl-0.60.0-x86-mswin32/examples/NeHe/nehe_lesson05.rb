#!/usr/bin/env ruby -rubygems
# Name:  nehe_lesson05.rb
# Purpose: An implementation of NeHe's OpenGL Lesson #05
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
$window = ""
# Angle for the triangle  (Global)
$pyramid_angle = 0
# Angle for the quadrilateral (Global)
$cube_angle = 0

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

    # Rotate the pyramid on the Y-axis
    glRotatef($pyramid_angle, 0.0, 1.0, 0.0)
    # Draw a pyramid
    glBegin(GL_POLYGON)
        # Draw front side of pyramid
        glColor3f(1.0, 0.0, 0.0)
        glVertex3f( 0.0,  1.0, 0.0)
        glColor3f(0.0, 1.0, 0.0)
        glVertex3f(-1.0, -1.0, 1.0)
        glColor3f(0.0, 0.0, 1.0)
        glVertex3f(1.0, -1.0, 1.0)
        # Draw right side of pyramid
        glColor3f(1.0, 0.0, 0.0)
        glVertex3f( 0.0,  1.0, 0.0)
        glColor3f(0.0, 0.0, 1.0)
        glVertex3f( 1.0, -1.0, 1.0)
        glColor3f(0.0, 1.0, 0.0)
        glVertex3f(1.0, -1.0, -1.0)
        # Draw back side of pyramid
        glColor3f(1.0, 0.0, 0.0)
        glVertex3f( 0.0,  1.0, 0.0)
        glColor3f(0.0, 0.0, 1.0)
        glVertex3f(1.0, -1.0, -1.0)
        glColor3f(0.0, 1.0, 0.0)
        glVertex3f(-1.0, -1.0, -1.0)
        # Draw left side of pyramid
        glColor3f(1.0, 0.0, 0.0)
        glVertex3f( 0.0,  1.0, 0.0)
        glColor3f(0.0, 0.0, 1.0)
        glVertex3f(-1.0, -1.0, -1.0)
        glColor3f(0.0, 1.0, 0.0)
        glVertex3f(-1.0, -1.0, 1.0)
    glEnd

    glLoadIdentity
    # Move right 3 units
    glTranslatef(1.5, 0.0, -7.0)

    # Draw a cube
    # Rotate the cube on the X, Y and Z axis
    glRotatef($cube_angle, 1.0, 1.0, 1.0)
    # Set it to a blue color one time only
    glBegin(GL_QUADS)
        # Draw the top side in green
        glColor3f(0.0, 1.0, 0.0)
        glVertex3f( 1.0,  1.0, -1.0)
        glVertex3f(-1.0,  1.0, -1.0)
        glVertex3f(-1.0,  1.0,  1.0)
        glVertex3f( 1.0,  1.0,  1.0)
        # Draw the bottom side in orange
        glColor3f(1.0, 0.5, 0.0)
        glVertex3f( 1.0, -1.0,  1.0)
        glVertex3f(-1.0, -1.0,  1.0)
        glVertex3f(-1.0, -1.0, -1.0)
        glVertex3f( 1.0, -1.0, -1.0)
        # Draw the front side in red
        glColor3f(1.0, 0.0, 0.0)
        glVertex3f( 1.0,  1.0,  1.0)
        glVertex3f(-1.0,  1.0,  1.0)
        glVertex3f(-1.0, -1.0,  1.0)
        glVertex3f( 1.0, -1.0,  1.0)
        # Draw the back side in yellow
        glColor3f(1.0, 1.0, 0.0)
        glVertex3f( 1.0, -1.0, -1.0)
        glVertex3f(-1.0, -1.0, -1.0)
        glVertex3f(-1.0,  1.0, -1.0)
        glVertex3f( 1.0,  1.0, -1.0)
        # Draw the left side in blue
        glColor3f(0.0, 0.0, 1.0)
        glVertex3f(-1.0,  1.0,  1.0)
        glVertex3f(-1.0,  1.0, -1.0)
        glVertex3f(-1.0, -1.0, -1.0)
        glVertex3f(-1.0, -1.0,  1.0)
        # Draw the right side in violet
        glColor3f(1.0, 0.0, 1.0)
        glVertex3f( 1.0,  1.0, -1.0)
        glVertex3f( 1.0,  1.0,  1.0)
        glVertex3f( 1.0, -1.0,  1.0)
        glVertex3f( 1.0, -1.0, -1.0)
    glEnd

    $pyramid_angle += 0.2
    $cube_angle -= 0.15
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
          glutDestroyWindow($window)
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
$window = glutCreateWindow("NeHe Lesson 05 - ruby-opengl version");
glutDisplayFunc(method(:draw_gl_scene).to_proc);
glutReshapeFunc(method(:reshape).to_proc);
glutIdleFunc(method(:idle).to_proc);
glutKeyboardFunc(keyboard);
init_gl_window(640, 480)
glutMainLoop();
