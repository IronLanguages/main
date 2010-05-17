///////////////////////////////////////////////////////////////////////////////
// When the page is loaded, place a single hidden Silverlight control on the 
// page, which loads the DLR assemblies, and then pulls DLR code out of the
// HTML page.
///////////////////////////////////////////////////////////////////////////////

if(!window.DLR) {
  window.DLR = {}
}

if(typeof DLR.autoAdd == 'undefined') {
  DLR.autoAdd = true
}

if(!DLR.path) {
  //DLR.path = null
  DLR.path = "/dlr-build"
}

if (typeof HTMLElement != "undefined" && !HTMLElement.prototype.insertAdjacentElement) {
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
        else this.parentNode.appendChild(parsedNode);
        break;
    }
  }
}

if(!DLR.__loaded) {

  Object.merge = function(dest, src) {
    var _temp = {}
    for(var prop in dest) {
      _temp[prop] = dest[prop]
    }
    for(var prop in src) {
      _temp[prop] = src[prop]
    }
    return _temp
  }

  /*
   * DLR.parseSettings(defaults, settings)
   *
   * @param defaults: an object representing the default settings.
   * @param settings: overrides any default settings.
   * @returns An object ready to be given to DLR.__createSilverlightObject:
   *          the DLR-specific options are moved into initParams.
   */
  DLR.parseSettings = function(defaults, settings) {
    // the the full settings dictionary
    var raw_settings = Object.merge(defaults, settings);

    // pull out the DLR-specific configuration options
    var dlr_keys = ['debug', 'console', 'start', 'exceptionDetail', 'reportErrors', 'xamlid'];
    var dlr_options = {};
    for(d in dlr_keys) {
      key = dlr_keys[d]
      if(raw_settings[key]) {
        dlr_options[key] = raw_settings[key];
        delete raw_settings[key];
      }
    }
 
    // generate settings.initParams from the rest of the DLR-specific options
    var initParams = "";
    for(opt in dlr_options) {
      initParams += (opt + "=" + dlr_options[opt] + ",")
    }
    initParams = initParams.substring(0, initParams.length - 1);
    raw_settings['initParams'] = initParams;

    return raw_settings;
  }

  /*
   * DLR.__startup()
   *
   * Creates a Silverlight control (using the current DLR.settings) if the DLR
   * hasn't loaded yet, autoAdd is true, and Silverlight is installed.
   * Also adds a silverlight control for each XAML script-tag
   */
  DLR.__startup = function() {
    if(!DLR.__loaded && DLR.autoAdd && Silverlight.isInstalled(null)) {
      DLR.createSilverlightObject({});
      DLR.__loaded = true;
    }

    elements = document.getElementsByTagName("script");
    for(var i = 0; i < elements.length; i++) {
      var element = elements[i];
      if(element.type == 'application/xml+xaml') { // && !element.defer) {
        settings = {
          width: element.getAttribute('width'),
          height: element.getAttribute('height')
        };
        if(element.id == '')
          element.id = DLR.__defaultXAMLId + DLR.__objectCount;
        settings.xamlid = element.id;
        DLR.createSilverlightObject(settings);
      }
    }
  }

  DLR.__defaultXAMLId = "silverlightDLRXAML"

  DLR.__defaultObjectId = "silverlightDLRObject"

  DLR.__objectCount = 0

  /*
   * DLR.createSilverlightObject(settings)
   *
   * @param settings: pre-parsed settings to be passed as 2nd argument to 
   *                  DLR.parseSettings
   *
   * Parses settings and passes them to DLR.__createSilverlightObject.
   */
  DLR.createSilverlightObject = function(settings) {
    settings = typeof(settings) == 'undefined' ? {} : settings;

    var xamlid = settings.xamlid;
    DLR.__createSilverlightObject(
      xamlid,
      DLR.parseSettings(DLR.getSettings(), settings)
    );
  }

  /*
   * DLR.__createSilverlightObject(settings)
   *
   * @param settings: Already-parsed settings to create the Silverlight control
   *                  with.
   *
   * If settings.xamlid points to a valid DOM element, the Silverlight control
   * is added as a sibling afer. Otherwise it's appended to the body.
   */
  DLR.__createSilverlightObject = function(xamlid, settings) {
    settings = typeof(settings) == 'undefined' ? {} : settings;

    var spantag = document.createElement("span");
    var sibling = null;

    if(xamlid)
      sibling = document.getElementById(xamlid);
    if(sibling && !sibling.parentElement && sibling.parentNode)
      sibling.parentElement = sibling.parentNode;
    
    if(!sibling || !sibling.parentElement || sibling.parentElement.tagName == "HEAD")
      document.body.appendChild(spantag);
    else
      sibling.insertAdjacentElement('afterEnd', spantag);

    if(settings.id == DLR.defaultSettings().id && DLR.__objectCount > 0)
      settings.id = DLR.__defaultObjectId + DLR.__objectCount

    slHtml = Silverlight.buildHTML(settings);
    spantag.innerHTML = slHtml;
    DLR.__objectCount++;
  }

  DLR.getSettings = function() {
    return Object.merge(DLR.defaultSettings(), DLR.settings);
  }

  DLR.defaultSettings = function() {
    return {
      width: 1,
      height: 1,
      onError: 'Silverlight.default_error_handler',
      reportErrors: 'errorLocation',
      source: DLR.path != null ? DLR.path + '/dlr.xap' : 'dlr.xap',
      id: 'silverlightDlrObject_DOMOnly',
      enableHtmlAccess: "true"
    };
  }

  if(!DLR.settings)
    DLR.settings = {}

  /*
   * __onDownloadCompleteToPoll is only used by the XMLHttpRequest implementation
   * that uses polling to detect the download succeeded, rather than using the 
   * HTMLBridge to hook the XMLHttpRequest.onreadystatechanged event. The only
   * benefit to this is that a cross-domain XAP file does not require the
   * ExternalCallersFromCrossDomain="ScriptableOnly" setting to exist in the
   * AppManifest.xaml.
   */

  DLR.__onDownloadCompleteToPoll = function(file) {
    return function() {
      if (this.readyState == 4 && this.status == 200) {
        obj = DLR.__persistFile(file, this.responseText, this.status);
      } else if (this.readyState == 4 && this.status != 200) {
        obj = DLR.__persistFile(file, null, this.status);
      }
    }
  }

  DLR.__persistFile = function(file, content, status) {
    obj = document.getElementById(file);
    if (obj == null) {
      obj = document.createElement("div");
      obj.id = file;
    }
    obj.setAttribute("style", "display: none");
    obj.status = status;
    obj.scriptContent = content;
    document.body.appendChild(obj);
    return obj;
  }

  if(window.addEventListener) {
    window.addEventListener('load', DLR.__startup, false);
  } else {
    window.attachEvent('onload', DLR.__startup);
  }
};
