var ADIDAS = ADIDAS || {};
//jQuery.noConflict();
// General purpose namespace method
// this will allow us to create namespace a bit easier
ADIDAS.CreateNameSpace = function (namespace) {
    var nsparts = namespace.split(".");
    var parent = ADIDAS;

    // we want to be able to include or exclude the root namespace 
    // So we strip it if it's in the namespace
    if (nsparts[0] === "ADIDAS") {
        nsparts = nsparts.slice(1);
    }
    // loop through the parts and create
    // a nested namespace if necessary
    for (var i = 0; i < nsparts.length; i++) {
        var partname = nsparts[i];
        // check if the current parent already has 
        // the namespace declared, if not create it
        if (typeof parent[partname] === "undefined") {
            parent[partname] = {};
        }
        // get a reference to the deepest element 
        // in the hierarchy so far
        parent = parent[partname];
    }
    // the parent is now completely constructed 
    // with empty namespaces and can be used.
    return parent;
};
/*---------------------------------------- START : Constants --------------------------------------------*/

//Create NameSpace for common methods
ADIDAS.CreateNameSpace("ADIDAS.GM.CONSTANTS");
ADIDAS.GM.CONSTANTS = {
    // Maintains constants in the application
    CONFIG: function (name) {
        var constants = {
            //API Url
            "WebAPIUrl": "http://mobileapproval.azurewebsites.net",
            "MobileIconKeyword": "[ Mobile ]",
            "MobileIconClass": "glyphicon-phone",
            "BackendIconKeyword": "[ Backend ]",
            "BackendIconClass": "glyphicon-blackboard"
         };
        return constants[name];
    },
};