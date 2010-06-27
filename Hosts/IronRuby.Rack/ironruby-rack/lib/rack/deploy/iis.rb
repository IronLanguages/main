require 'System.Data'
require 'System.Xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
require 'System.Deployment, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
require 'System.DirectoryServices, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
require 'System.ServiceProcess, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'

include System
include System::DirectoryServices
include System::IO
include System::Collections
include System::ServiceProcess
include System::Security::AccessControl
include System::Security::Principal
 
module Rack

  module Deploy

    class IIS  
      ROOT = "IIS://localhost/W3SVC/1/root".to_clr_string
      
      class << self
        def deploy(options)
          check_iis
          
          options = {
            :app_name => "IronRubyRackApp",
            :app_path => File.expand_path('.')
          }.merge(options)
          options.each{|k,v| options[k] = v.to_clr_string}
          
          set_permissions options[:app_path]
          
          create_virtual_directory options[:app_name], options[:app_path]
          
          restart_iis
        end

        def remove(opts)
          delete opts[:app_name].to_clr_string, DirectoryEntry.new(ROOT)
        end

        def set_permissions app_path
          dfr = FileSystemRights.full_control
          gan = 'IIS_IUSRS'

          unless acl_exists? app_path, dfr, gan
            add_acl app_path, dfr, gan
          end
        end

        def restart_iis
          `iisreset /restart`
          puts "IIS Restarted"
        end

        private
          def check_iis
            begin
              dir_exists = DirectoryEntry.exists ROOT
            rescue => e
              raise System::Exception.clr_new("Deploying on IIS requires Administrator privileges", e)
            end
            
            raise "Please make sure IIS is installed on this machine." unless dir_exists
          end

          def create_virtual_directory(app_name, app_path)
            Directory.create_directory app_path unless Directory.exists app_path

            root = DirectoryEntry.new ROOT

            create_app get_direct_parent_app(root, app_name), 
                       app_name.substring(app_name.last_index_of('/') + 1),
                       app_path
          end

          def get_direct_parent_app current_entry, current_vpath
            while current_vpath.starts_with '/'
              current_vpath = current_vpath.substring 1
            end

            i = current_vpath.index_of '/'

            return current_entry if i < 0

            cur_name = current_vpath.substring 0, i
            cur_vpath = current_vpath.substring i + 1

            cur_entry = get_app(cur_name, current_entry) || raise(Exception.new("Please make sure parent application #{cur_name} exist under #{current_entry.parent.name}"))

            get_direct_parent_app cur_entry, cur_vpath
          end

          def create_app entry_parent, app_name, app_path
            new_app = get_app app_name, entry_parent
            unless new_app
              new_app = entry_parent.children.add app_name, "IIsWebVirtualDir"
              new_app.invoke "AppCreate", true
              new_app.commit_changes
              puts "#{app_name} has been created successfully."
            else
              puts "Found #{app_name}"
            end

            #new_app.properties.each do |i|
            #  puts "#{i.PropertyName} => , #{i.Value}
            #end
            #new_app.properties["DirBrowseFlags"].value = -2147483586 # enable directory browsing
            new_app.properties["Path"].value = app_path
            new_app.properties["AccessRead"][0] = true
            new_app.properties["AccessExecute"][0] = true
            new_app.properties["AccessWrite"][0] = true
            new_app.properties["ContentIndexed"][0] = true
            new_app.properties["AppFriendlyName"][0] = app_name
            new_app.properties["AccessScript"][0] = true

            # TODO enable windows permissions

            new_app.commit_changes

            puts "#{app_name} has been configured successfully."
          end

          def get_app name, entryparent
            entryparent.children.select{|entry| entry.name.to_lower == name.to_lower}.first
          end

          def delete name, entry_parent
            app = get_app name, entry_parent
            raise "#{name} does not exist" unless app
            app.delete_tree
            puts "#{name} has been deleted"
          end

          def acl_exists? app_path, desiredFilesystemRights, groupOrAccountName
            di = DirectoryInfo.new app_path
            ds = di.GetAccessControl
            goa = NTAccount.new groupOrAccountName
            for fr in ds.GetAccessRules(true, true, NTAccount.to_clr_type)
              if ((fr.AccessControlType.Equals(AccessControlType.Allow)) &&
                  (fr.FileSystemRights.Equals(desiredFilesystemRights)) &&
                  (fr.IdentityReference.Equals(groupOrAccountName)))
                return true
              end
            end
            false
          end

          def add_acl app_path, desiredFilesystemRights, groupOrAccountName
            di = DirectoryInfo.new app_path
            ds = di.GetAccessControl

            dsar = FileSystemAccessRule.new(
              groupOrAccountName, 
              desiredFilesystemRights,
              InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
              PropagationFlags.InheritOnly,
              AccessControlType.Allow
            )
            ds.AddAccessRule dsar

            Directory.SetAccessControl app_path, ds
            puts "Gives #{groupOrAccountName} #{desiredFilesystemRights} to #{app_path}"
          end

      end # class << self  
    end # class IIS
    
  end # module Deploy
  
end # module Rack
