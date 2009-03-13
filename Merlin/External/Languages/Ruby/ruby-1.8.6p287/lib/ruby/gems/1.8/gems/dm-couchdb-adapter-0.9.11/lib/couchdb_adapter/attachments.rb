require 'base64'
require 'net/http'
require 'rubygems'

gem 'mime-types', '~>1.15'
require 'mime/types'

module DataMapper
  module CouchResource
    module Attachments

      def self.included(mod)
        mod.class_eval do

          def add_attachment(file, options = {})
            assert_attachments_property

            filename = File.basename(file.path)

            content_type = options[:content_type] || begin
              mime_types = MIME::Types.of(filename)
              mime_types.empty? ? 'application/octet-stream' : mime_types.first.content_type
            end

            name = options[:name] || filename
            data = file.read

            if new_record? || !model.properties.has_property?(:rev)
              self.attachments ||= {}
              self.attachments[name] = {
                'content_type' => content_type,
                'data'         => Base64.encode64(data).chomp,
              }
            else
              adapter = repository.adapter
              http = Net::HTTP.new(adapter.uri.host, adapter.uri.port)
              uri = Addressable::URI.encode_component("#{attachment_path(name)}?rev=#{self.rev}")
              headers = {
                'Content-Length' => data.size.to_s,
                'Content-Type'   => content_type,
              }
              http.put(uri, data, headers)
              self.reload
            end

          end

          def delete_attachment(name)
            assert_attachments_property

            attachment = self.attachments[name] if self.attachments

            unless attachment
              return false
            end

            response = unless new_record?
              adapter = repository.adapter
              http = Net::HTTP.new(adapter.uri.host, adapter.uri.port)
              uri = Addressable::URI.encode_component("#{attachment_path(name)}?rev=#{self.rev}")
              http.delete(uri, 'Content-Type' => attachment['content_type'])
            end

            if response && !response.kind_of?(Net::HTTPSuccess)
              false
            else
              self.attachments.delete(name)
              self.attachments = nil if self.attachments.empty?
              true
            end
          end

          # TODO: cache data on model? (don't want to make resource dirty though...)
          def get_attachment(name)
            assert_attachments_property

            attachment = self.attachments[name] if self.attachments

            unless self.id && attachment
              nil
            else
              adapter = repository.adapter
              http = Net::HTTP.new(adapter.uri.host, adapter.uri.port)
              uri = Addressable::URI.encode_component(attachment_path(name))
              response, data = http.get(uri, 'Content-Type' => attachment['content_type'])

              unless response.kind_of?(Net::HTTPSuccess)
                nil
              else
                data
              end
            end

          end

          private

          def attachment_path(name)
            if new_record?
              nil
            else
              "/#{repository.adapter.escaped_db_name}/#{self.id}/#{name}"
            end
          end

          def assert_attachments_property
            property = model.properties[:attachments]

            unless property &&
              property.type == DataMapper::Types::JsonObject &&
              property.field == '_attachments'
              raise ArgumentError, "Attachments require   property :attachments, JsonObject, :field => '_attachments'"
            end
          end

        end

      end
    end
  end
end
