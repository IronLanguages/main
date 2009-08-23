// This is the path to the DLR XAP files. Note that the path is always relative
// to the page that is using the DLR, and NOT relative to the dlr.js file.
// You can set the path directly from your page, or edit this page, and consider
// using an absolute path instead of relative.
if (!window.dlrpath)
    window.dlrpath = "../../../Bin/Silverlight Debug/";

////////////////////////////////////////////////////////////////////////////////
// HTML Utility functions
////////////////////////////////////////////////////////////////////////////////

if (typeof HTMLElement != "undefined" && 
    !HTMLElement.prototype.insertAdjacentElement) {

  HTMLElement.prototype.insertAdjacentElement = function(where, parsedNode) {
    switch (where) {
      case 'beforeBegin':
        this.parentNode.insertBefore(parsedNode, this)
        break;
      case 'afterBegin':
        this.insertBefore(parsedNode, this.firstChild);
        break;
      case 'beforeEnd':
        this.appendChild(parsedNode);
        break;
      case 'afterEnd':
        if (this.nextSibling)
            this.parentNode.insertBefore(parsedNode, this.nextSibling);
        else 
            this.parentNode.appendChild(parsedNode);
        break;
    }
  }

}

function get_lastchild(n) {
  x = n.lastChild;
  while (x.nodeType != 1) {
    x = x.previousSibling;
  }
  return x;
}

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

if (!window.dlrLoaded) {
  window.dlrLoaded = true;

  jQuery(function() {

    function InjectSilverlightTag(sibling, settings) {
      var spantag = document.createElement("span");
      if (sibling.parentElement && sibling.parentElement.tagName == "HEAD")
        document.body.appendChild(spantag);
      else 
        sibling.insertAdjacentElement('afterEnd', spantag);
      settings.source = dlrtpath;
      settings.initParams = "id=" + settings.htmlid;
      slHtml = Silverlight.buildHTML(settings);
      spantag.innerHTML = slHtml;
    };

    jQuery.fn.CheckIds = function() {
      this.each(function() {
        if (this.id == "") {
          window.idcounter++;
          this.id = "g-gen-id-" + window.idcounter;
        };
      });
    };

    jQuery.fn.InjectSilverlight = function() {
      this.each(function() {
        var settings = {};
        settings.width = this.getAttribute("width");
        settings.height = this.getAttribute("height");
        settings.htmlid = this.id;
        settings.onerror = "onSilverlightError";
        settings.id = settings.htmlid + "-sl";
        InjectSilverlightTag(this, settings);
      });
    };

    if (Silverlight.isInstalled(null)) {
      //var xamlBlocks = jQuery(".xaml").length;
      var pythonBlocks = jQuery("script[language=python]").length + jQuery("script[type=text/python]").length + jQuery("script[type=application/python]").length;
      var rubyBlocks = jQuery("script[language=ruby]").length + jQuery("script[type=text/ruby]").length + jQuery("script[type=application/ruby]").length;

      if (pythonBlocks == 0) {
        if (rubyBlocks == 0) {  // No language support
          gestaltpath += "xaml.xap";
        }
        else {  // Ruby, but no Python
          gestaltpath += "ironruby.xap";
        }
      }
      else {
        if (rubyBlocks == 0) { // Python, but no Ruby
          gestaltpath += "ironpython.xap";
        }
        else {  // both languages
          gestaltpath += "all.xap";
        }
      }

      //if (xamlBlocks > 0) {
      //  jQuery(".xaml").CheckIds();
      //  jQuery(".xaml").css("display", "none").InjectSilverlight();
      //}
      //else {
        if (rubyBlocks > 0 || pythonBlocks > 0) {
          // there are code blocks but no SL control; inject one so they can party on DOM
          var last = get_lastchild(document.body);

          var settings = {};
          settings.width = 1;
          settings.height = 1;
          settings.id = "GestaltDOMOnly-sl";
          settings.htmlid = "GestaltDOMOnly";

          InjectSilverlightTag(last, settings);
        }
      //}
    }

  })

};
