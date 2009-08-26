CAMPING_EXTRAS_DIR = File.expand_path(File.dirname(__FILE__))

module Generators
class HTMLGenerator
    def generate_html
      @files_and_classes = {
        'allfiles'     => gen_into_index(@files),
        'allclasses'   => gen_into_index(@classes),
        "initial_page" => main_url,
        'realtitle'    => CGI.escapeHTML(@options.title),
        'charset'      => @options.charset
      }

      # the individual descriptions for files and classes
      gen_into(@files)
      gen_into(@classes)
      gen_main_index
      
      # this method is defined in the template file
      write_extra_pages if defined? write_extra_pages
    end

    def gen_into(list)
      hsh = @files_and_classes.dup
      list.each do |item|
        if item.document_self
          op_file = item.path
          hsh['root'] = item.path.split("/").map { ".." }[1..-1].join("/")
          item.instance_variable_set("@values", hsh)
          File.makedirs(File.dirname(op_file))
          File.open(op_file, "w") { |file| item.write_on(file) }
        end
      end
    end

    def gen_into_index(list)
      res = []
      list.each do |item|
        hsh = item.value_hash
        hsh['href'] = item.path
        hsh['name'] = item.index_name
        res << hsh
      end
      res
    end

    def gen_main_index
      template = TemplatePage.new(RDoc::Page::INDEX)
      File.open("index.html", "w") do |f|
        values = @files_and_classes.dup
        if @options.inline_source
          values['inline_source'] = true
        end
        template.write_html_on(f, values)
      end
      ['Camping.gif', 'permalink.gif'].each do |img|
          ipath = File.join(CAMPING_EXTRAS_DIR, img)
          File.copy(ipath, img)
      end
    end
end
end


module RDoc
module Page
######################################################################
#
# The following is used for the -1 option
#

FONTS = "verdana,arial,'Bitstream Vera Sans',helvetica,sans-serif"

STYLE = %{
    body, th, td {
        font: normal 14px verdana,arial,'Bitstream Vera Sans',helvetica,sans-serif;
        line-height: 160%;
        padding: 0; margin: 0;
        margin-bottom: 30px;
        /* background-color: #402; */
        background-color: #694;
    }
    h1, h2, h3, h4 {
        font-family: Utopia, Georgia, serif;
        font-weight: bold;
        letter-spacing: -0.018em;
    }
    h1 { font-size: 24px; margin: .15em 1em 0 0 }
    h2 { font-size: 24px }
    h3 { font-size: 19px }
    h4 { font-size: 17px; font-weight: normal; }
    h4.ruled { border-bottom: solid 1px #CC9; }
    h2.ruled { padding-top: 35px; border-top: solid 1px #AA5; }

    /* Link styles */
    :link, :visited {
        color: #00b;
    }
    :link:hover, :visited:hover {
        background-color: #eee;
        color: #B22;
    }
    #fullpage {
        width: 720px;
        margin: 0 auto;
    }
    .page_shade, .page {
        padding: 0px 5px 5px 0px;
        background-color: #fcfcf9;
        border: solid 1px #983;
    }
    .page {
        margin-left: -5px;
        margin-top: -5px;
        padding: 20px 35px;
    }
    .page .header {
        float: right;
        color: #777;
        font-size: 10px;
    }
    .page h1, .page h2, .page h3 {
        clear: both;
        text-align: center;
    }
    #pager {
        padding: 10px 4px;
        color: white;
        font-size: 11px;
    }
    #pager :link, #pager :visited {
        color: #bfb;
        padding: 0px 5px;
    }
    #pager :link:hover, #pager :visited:hover {
        background-color: #262;
        color: white;
    }
    #logo { float: left; }
    #menu { background-color: #dfa; padding: 4px 12px; margin: 0; }
    #menu h3 { padding: 0; margin: 0; }
    #menu #links { float: right; }
    pre { font-weight: bold; color: #730; }
    tt { color: #703; font-size: 12pt; }
    .dyn-source { background-color: #775915; padding: 4px 8px; margin: 0; display: none; }
    .dyn-source pre  { color: #DDDDDD; font-size: 8pt; }
    .source-link     { text-align: right; font-size: 8pt; }
    .ruby-comment    { color: green; font-style: italic }
    .ruby-constant   { color: #CCDDFF; font-weight: bold; }
    .ruby-identifier { color: #CCCCCC;  }
    .ruby-ivar       { color: #BBCCFF; }
    .ruby-keyword    { color: #EEEEFF; font-weight: bold }
    .ruby-node       { color: #FFFFFF; }
    .ruby-operator   { color: #CCCCCC;  }
    .ruby-regexp     { color: #DDFFDD; }
    .ruby-value      { color: #FFAAAA; font-style: italic }
    .kw { color: #DDDDFF; font-weight: bold }
    .cmt { color: #CCFFCC; font-style: italic }
    .str { color: #EECCCC; font-style: italic }
    .re  { color: #EECCCC; }
}

CONTENTS_XML = %{
IF:description
%description%
ENDIF:description

IF:requires
<h4>Requires:</h4>
<ul>
START:requires
IF:aref
<li><a href="%aref%">%name%</a></li>
ENDIF:aref
IFNOT:aref
<li>%name%</li>
ENDIF:aref 
END:requires
</ul>
ENDIF:requires

IF:attributes
<h4>Attributes</h4>
<table>
START:attributes
<tr><td>%name%</td><td>%rw%</td><td>%a_desc%</td></tr>
END:attributes
</table>
ENDIF:attributes

IF:includes
<h4>Includes</h4>
<ul>
START:includes
IF:aref
<li><a href="%aref%">%name%</a></li>
ENDIF:aref
IFNOT:aref
<li>%name%</li>
ENDIF:aref 
END:includes
</ul>
ENDIF:includes

START:sections
IF:method_list
<h2 class="ruled">Methods</h2>
START:method_list
IF:methods
START:methods
<h4 class="ruled">%type% %category% method: 
IF:callseq
<strong><a name="%aref%">%callseq%</a></strong> <a href="#%aref%"><img src="%root%/permalink.gif" border="0" title="Permalink to %callseq%" /></a>
ENDIF:callseq
IFNOT:callseq
<strong><a name="%aref%">%name%%params%</a></strong> <a href="#%aref%"><img src="%root%/permalink.gif" border="0" title="Permalink to %type% %category% method: %name%" /></a></h4>
ENDIF:callseq

IF:m_desc
%m_desc%
ENDIF:m_desc

IF:sourcecode
<div class="sourcecode">
  <p class="source-link">[ <a href="javascript:toggleSource('%aref%_source')" id="l_%aref%_source">show source</a> ]</p>
  <div id="%aref%_source" class="dyn-source">
<pre>
%sourcecode%
</pre>
  </div>
</div>
ENDIF:sourcecode
END:methods
ENDIF:methods
END:method_list
ENDIF:method_list
END:sections
}

############################################################################


BODY = %{
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN">
<html>
<head>
  <title>
IF:title
  %realtitle% &raquo; %title%
ENDIF:title
IFNOT:title
  %realtitle%
ENDIF:title
  </title>
  <meta http-equiv="Content-Type" content="text/html; charset=%charset%" />
  <link rel="stylesheet" href="%style_url%" type="text/css" media="screen" />
    <script language="JavaScript" type="text/javascript">
    // <![CDATA[

    function toggleSource( id )
    {
    var elem
    var link

    if( document.getElementById )
    {
    elem = document.getElementById( id )
    link = document.getElementById( "l_" + id )
    }
    else if ( document.all )
    {
    elem = eval( "document.all." + id )
    link = eval( "document.all.l_" + id )
    }
    else
    return false;

    if( elem.style.display == "block" )
    {
    elem.style.display = "none"
    link.innerHTML = "show source"
    }
    else
    {
    elem.style.display = "block"
    link.innerHTML = "hide source"
    }
    }

    function openCode( url )
    {
    window.open( url, "SOURCE_CODE", "width=400,height=400,scrollbars=yes" )
    }
    // ]]>
    </script>
</head>
<body>
<div id="menu">
<div id="links">
    <a href="http://redhanded.hobix.com/bits/campingAMicroframework.html">backstory</a> |
    <a href="http://code.whytheluckystiff.net/camping/">wiki</a> |
    <a href="http://code.whytheluckystiff.net/camping/newticket">bugs</a> |
    <a href="http://code.whytheluckystiff.net/svn/camping/">svn</a>
</div>
<h3 class="title">%title%</h3>
</div>
<div id="fullpage">
<div id="logo"><img src="%root%/Camping.gif" /></div>
<div id="pager">
<strong>Files:</strong>
START:allfiles
<a href="%root%/%href%" value="%title%">%name%</a>
END:allfiles
IF:allclasses
|
<strong>classes:</strong>
START:allclasses
<a href="%root%/%href%" title="%title%">%name%</a>
END:allclasses
ENDIF:allclasses
</ul>
</div>

    !INCLUDE!

</div>
</body>
</html>
}

###############################################################################

FILE_PAGE = <<_FILE_PAGE_
<div id="%full_path%" class="page_shade">
<div class="page">
<div class="header">
  <div class="path">%full_path% / %dtm_modified%</div>
</div>
#{CONTENTS_XML}
</div>
</div>
_FILE_PAGE_

###################################################################

CLASS_PAGE = %{
<div id="%full_name%" class="page_shade">
<div class="page">
IF:parent
<h3>%classmod% %full_name% &lt; HREF:par_url:parent:</h3>
ENDIF:parent
IFNOT:parent
<h3>%classmod% %full_name%</h3>
ENDIF:parent

IF:infiles
(in files
START:infiles
HREF:full_path_url:full_path:
END:infiles
)
ENDIF:infiles
} + CONTENTS_XML + %{
</div>
</div>
}

###################################################################

METHOD_LIST = %{
IF:includes
<div class="tablesubsubtitle">Included modules</div><br>
<div class="name-list">
START:includes
    <span class="method-name">HREF:aref:name:</span>
END:includes
</div>
ENDIF:includes

IF:method_list
START:method_list
IF:methods
<table cellpadding=5 width="100%">
<tr><td class="tablesubtitle">%type% %category% methods</td></tr>
</table>
START:methods
<table width="100%" cellspacing = 0 cellpadding=5 border=0>
<tr><td class="methodtitle">
<a name="%aref%">
IF:callseq
<b>%callseq%</b>
ENDIF:callseq
IFNOT:callseq
 <b>%name%</b>%params%
ENDIF:callseq
IF:codeurl
<a href="%codeurl%" target="source" class="srclink">src</a>
ENDIF:codeurl
</a></td></tr>
</table>
IF:m_desc
<div class="description">
%m_desc%
</div>
ENDIF:m_desc
IF:aka
<div class="aka">
This method is also aliased as
START:aka
<a href="%aref%">%name%</a>
END:aka
</div>
ENDIF:aka
IF:sourcecode
<div class="sourcecode">
  <p class="source-link">[ <a href="javascript:toggleSource('%aref%_source')" id="l_%aref%_source">show source</a> ]</p>
  <div id="%aref%_source" class="dyn-source">
<pre>
%sourcecode%
</pre>
  </div>
</div>
ENDIF:sourcecode
END:methods
ENDIF:methods
END:method_list
ENDIF:method_list
}


########################## Index ################################

FR_INDEX_BODY = %{
!INCLUDE!
}

FILE_INDEX = %{
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=%charset%">
<style>
<!--
  body {
background-color: #ddddff;
     font-family: #{FONTS}; 
       font-size: 11px; 
      font-style: normal;
     line-height: 14px; 
           color: #000040;
  }
div.banner {
  background: #0000aa;
  color:      white;
  padding: 1;
  margin: 0;
  font-size: 90%;
  font-weight: bold;
  line-height: 1.1;
  text-align: center;
  width: 100%;
}
  
-->
</style>
<base target="docwin">
</head>
<body>
<div class="banner">%list_title%</div>
START:entries
<a href="%href%">%name%</a><br>
END:entries
</body></html>
}

CLASS_INDEX = FILE_INDEX
METHOD_INDEX = FILE_INDEX

INDEX = %{
<HTML>
<HEAD>
<META HTTP-EQUIV="refresh" content="0;URL=%initial_page%">
<TITLE>%realtitle%</TITLE>
</HEAD>
<BODY>
Click <a href="%initial_page%">here</a> to open the Camping docs.
</BODY>
</HTML>
}

end
end
