module Fox
  class FXQuatd < FXVec4d
    # Return an initialized FXQuatd instance.
    def initialize; end
    
    #
    # Construct an FXQuatd instance from a rotation axis and angle.
    #
    # ==== Parameters:
    #
    # +axis+::		the rotation axis [FXVec3d]
    # +angle+::		the rotation angle (in radians) [Float]
    #
    def initialize(axis, phi=0.0); end
    
    #
    # Construct an FXQuatd from components.
    #
    # ==== Parameters:
    #
    # +x+::		x [Float]
    # +y+::		y [Float]
    # +z+::		z [Float]
    # +width+::		w [Float]
    #
    def initialize(x, y, z, w); end
    
    # Construct an FXQuatd from an array of Floats.
    def initialize(floats); end

    #
    # Construct from Euler angles yaw (z), pitch (y) and roll (x).
    #
    # ==== Parameters:
    #
    # +roll+::		roll angle in radians [Float]
    # +pitch+::		pitch angle in radians [Float]
    # +yaw+::		yaw angle in radians [Float]
    #
    def initialize(roll, pitch, yaw); end
    
    # Construct quaternion from axes; _ex_, _ey_ and _ez_ are all FXVec3d
    # instances.
    def initialize(ex, ey, ez); end

    # Adjust quaternion length; returns a reference to self.
    def adjust!; end
    
    #
    # Set quaternion from rotation axis and angle.
    #
    # ==== Parameters:
    #
    # +axis+::		the rotation axis [FXVec3d]
    # +angle+::		the rotation angle (in radians) [Float]
    #
    def setAxisAngle(axis, phi=0.0); end
    
    #
    # Return the rotation axis and angle for this quaternion, i.e.
    #
    #     axis, angle = aQuaternion.getAxisAngle()
    #
    # where _axis_ is an FXVec3d instance and _angle_ is the angle
    # of rotation in radians.
    #
    def getAxisAngle(); end
    
    #
    # Set quaternion from yaw (z), pitch (y) and roll (x).
    #
    # ==== Parameters:
    #
    # +roll+::		roll angle in radians [Float]
    # +pitch+::		pitch angle in radians [Float]
    # +yaw+::		yaw angle in radians [Float]
    #
    def setRollPitchYaw(roll, pitch, yaw); end
    
    #
    # Obtain roll, pitch and yaw angles (in radians) from quaternion, e.g.
    #
    #     roll, pitch, yaw = aQuaternion.getRollPitchYaw()
    #
    def getRollPitchYaw(); end
    
    # Set quaternion from axes; _ex_, _ey_ and _ez_ are all FXVec3d instances.
    def setAxes(ex, ey, ez); end

    # Get quaternion axes; returns a 3-element array of FXVec3d instances.
    def getAxes(); end

    # Obtain local x axis (an FXVec3d instance).
    def getXAxis(); end

    # Obtain local y axis (an FXVec3d instance).
    def getYAxis(); end

    # Obtain local z axis (an FXVec3d instance).
    def getZAxis(); end

    #
    # Return the exponentiation of this quaternion (a new FXQuatd instance).
    #
    def exp; end
    
    #
    # Return the logarithm of this quaternion (a new FXQuatd instance).
    #
    def log; end
    
    #
    # Return the inverse of this quaternion (a new FXQuatd instance).
    #
    def invert; end
    
    #
    # Invert unit quaternion (returns a new FXQuatd instance).
    #
    def unitinvert; end
    
    #
    # Return the conjugate of this quaternion (a new FXQuatd instance).
    #
    def conj; end
    
    #
    # Return the product of this quaternion and _other_ (another FXQuatd instance).
    #
    def *(other); end
    
    #
    # Compute the rotation of a vector _vec_ by this quaternion; returns the
    # rotated vector (a new FXVec3d instance).
    #
    # ==== Parameters:
    #
    # +vec+::		the vector to be rotated [FXVec3d]
    #
    def *(vec); end
    
    #
    # Construct a quaternion from arc a->b on unit sphere and return a reference
    # to self.
    #
    # ==== Parameters:
    #
    # +a+::	[FXVec3d]
    # +b+::	[FXVec3d]
    #
    def arc!(a, b); end
    
    #
    # Spherical lerp and return a reference to self.
    #
    # ==== Parameters:
    #
    # +u+::	[FXQuatd]
    # +v+::	[FXQuatd]
    # +f+:: [Float]
    #
    def lerp!(u, v, f); end
  end
end
