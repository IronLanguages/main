module Fox
  #
  # The registry maintains a database of persistent settings for an application.
  # The settings database is organized in two groups of three layers each.  The
  # system-wide settings group contains settings information pertaining to all
  # users on a system.  The per-user settings group contains settings affecting
  # that user only.
  # Each settings group contains a desktop layer, which comprises the settings
  # which affect all FOX programs, a vendor layer which holds settings that
  # affect all applications from that vendor (e.g. a application-suite), and
  # an application layer which holds settings only for a single application.
  # The vendor-key and application-key determine which files these layers come
  # from, while the "Desktop" key is used for all FOX applications.
  # Settings in the system-wide group are overwritten by the per-user group,
  # and settings from the "Desktop" layer are overwritten by the vendor-layer;
  # vendor-layer settings are overwritten by the application-layer settings.
  # Only the per-user, per-application settings ever gets written; the layers
  # in the system-group only get written during installation and configuration
  # of the application.
  # The registry is read when FXApp::init() is called, and written back to the
  # system when FXApp::exit() is called.
  #
  class FXRegistry < FXSettings

    # Application key [String]
    attr_reader	:appKey
    
    # Vendor key [String]
    attr_reader	:vendorKey
    
    # Use file-based registry instead of Windows Registry [Boolean]
    attr_writer	:asciiMode

    #
    # Construct registry object; _appKey_ and _vendorKey_ must be string constants.
    # Regular applications SHOULD set a vendor key!
    #
    def initialize(appKey="", vendorKey="") ; end
    
    #
    # Read registry.
    #
    def read; end
    
    #
    # Write registry.
    #
    def write; end

    #
    # Return +true+ if we're using a file-based registry mechanism instead of the Windows Registry
    # (only relevant on Windows systems).
    #
    def asciiMode?; end
  end
end
