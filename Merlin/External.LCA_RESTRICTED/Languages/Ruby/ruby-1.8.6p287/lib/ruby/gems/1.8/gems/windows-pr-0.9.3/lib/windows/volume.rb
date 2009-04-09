require 'windows/api'

module Windows
   module Volume
      API.auto_namespace = 'Windows::Volume'
      API.auto_constant  = true
      API.auto_method    = true
      API.auto_unicode   = true

      DRIVE_UNKNOWN     = 0
      DRIVE_NO_ROOT_DIR = 1
      DRIVE_REMOVABLE   = 2
      DRIVE_FIXED       = 3
      DRIVE_REMOTE      = 4
      DRIVE_CDROM       = 5
      DRIVE_RAMDISK     = 6
      
      API.new('FindFirstVolume', 'PL', 'L')
      API.new('FindFirstVolumeMountPoint', 'PPL', 'L')
      API.new('FindNextVolume', 'LPL', 'B')
      API.new('FindNextVolumeMountPoint', 'LPL', 'B')
      API.new('FindVolumeClose', 'L', 'B')
      API.new('FindVolumeMountPointClose', 'L', 'B')
      API.new('GetDriveType', 'P', 'I')
      API.new('GetLogicalDrives', 'V', 'L')
      API.new('GetLogicalDriveStrings', 'LP', 'L')
      API.new('GetVolumeInformation', 'PPLPPPPL', 'B')      
      API.new('GetVolumeNameForVolumeMountPoint', 'PPL', 'B')
      API.new('GetVolumePathName', 'PPL', 'B')
      API.new('SetVolumeLabel', 'PP', 'B')
      API.new('SetVolumeMountPoint', 'PP', 'B')

      # Windows XP or later
      begin
         API.new('GetVolumePathNamesForVolumeName', 'PPLL', 'B')
      rescue Windows::API::Error
         # Do nothing - not supported on current platform.  It's up to you to
         # check for the existence of the constant in your code.
      end
   end
end
