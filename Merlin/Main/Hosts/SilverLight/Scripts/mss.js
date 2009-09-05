///////////////////////////////////////////////////////////////////////////////
// When the page is loaded, place a single hidden Silverlight control on the 
// page, which loads the DLR assemblies, and then pulls DLR code out of the
// HTML page.
///////////////////////////////////////////////////////////////////////////////

if(!window.DLR)
  window.DLR = {}

if(!DLR.__loaded) {

  Object.merge = function(dest, src) {
    var temp = {}
    for(var prop in dest) {
      temp[prop] = dest[prop]
    }
    for(var prop in src) {
      temp[prop] = src[prop]
    }
    return temp
  }

  DLR.parseSettings = function(defaults, settings) {
    // the the full settings dictionary
    var raw_settings = Object.merge(defaults, settings);

    // pull out the DLR-specific configuration options
    var dlr_keys = ['debug', 'console', 'start', 'exceptionDetail', 'reportErrors'];
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

  DLR.__startup = function() {
    DLR.__loaded = true;
    if(DLR.autoAdd && Silverlight.isInstalled(null)) {
      DLR.createObject(DLR.settings);
    }
  }

  DLR.__defaultObjectId = "silverlightDLRObject"

  DLR.__objectCount = 0

  DLR.createObject = function(settings) {
    settings = DLR.parseSettings(
      DLR.defaultSettings, 
      typeof(settings) == 'undefined' ? {} : settings
    )
    var spantag = document.createElement("span");
    document.body.appendChild(spantag);
    if(settings.id == DLR.defaultSettings.id && DLR.__objectCount > 0) {
      settings.id = DLR.__defaultObjectId + DLR.__objectCount
    }
    slHtml = Silverlight.buildHTML(settings);
    spantag.innerHTML = slHtml;
    DLR.__objectCount++;
  }

  DLR.defaultSettings = {
    autoAdd: true,
    width: 1,
    height: 1,
    onerror: 'Silverlight.default_error_handler',
    reportErrors: 'errorLocation',
    source: 'dlr.xap',
    id: 'silverlightDlrObject_DOMOnly'
  }

  DLR.settings = DLR.parseSettings(
      DLR.defaultSettings, 
      !DLR.settings ? {} : DLR.settings
  );

  // autoAdd is a special DLR-specific settings; put it on the DLR object
  DLR.autoAdd = DLR.settings.autoAdd;
  delete DLR.settings.autoAdd;

  //if(!window.__pageloaded) {

    if(window.addEventListener) {
      window.addEventListener('load', DLR.__startup, false);
    } else {
      window.attachEvent('onload', DLR.__startup);
    }

  // if the page has already loaded, depend on a "pageloaded" 
  // variable to detect that ... 
  //} else {
  //  DLR.startup();
  //}
};
