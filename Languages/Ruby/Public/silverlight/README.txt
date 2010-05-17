= IronRuby in Silverlight


== ATTENTION!

See http://ironruby.net/browser/ for the most up-to-date information 
on using Ruby in the browser with Silverlight. For example, this HTML
page hosts Silverlight to run Ruby code, and requires no extra 
dependencies:

<html>
  <head>
    <script type="text/javascript" 
            src="http://gestalt.ironruby.net/dlr-latest.js"></script>
  </head>
  <body>
    <div id="message">Loading ...</div>
    <script type="text/ruby">
      document.message.innerHTML = "Hello from Ruby!"
      document.message.onclick {|obj, args| obj.innerHTML = "Clicked!"}
    </script>
  </body>
</html>

Keep in mind the information, binaries, and examples in this package
will still help you build Silverlight applications with IronRuby, but
using HTML script-tags is the preferred model going forward; the
ways described here will be deprecated in a future release.


== Generating an application

script/sl ruby MyApp

Will create "MyApp" in the current directory. 


== Run an app

script/chr /b:MyApp\index.html

This will launch a browser pointed at the application.

For more information on the "chr" script, run "script/chr" for help.

== Running Samples

script/chr /b

Will open a browser pointed at the "/silverlight" directory. Navigate to the 
any Silverlight sample in the "/silverlight/samples" directory 
(index.html file).


== Package

  /bin              IronRuby and IronPython binaries for Silverlight
  /samples          Samples for IronRuby and IronPython in Silverlight
  /script           "chr" and "sl" script
  README.txt        This file

== License

Read the License.* files at the root of this release.
