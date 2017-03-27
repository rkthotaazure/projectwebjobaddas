/**
    * @desc : fetch the backend details content
    * @injection : GetJSON to call the REST api
*/
app.service('CommonService', function () {
    this.backend = "";
    this.pageTitle = "";    
    this.notificationActive = false;
    this.notificationText = "";
    this.showNotifications = function (notificationText, notificationType) {
        this.notificationActive = true;
        this.notificationText = notificationText;
    }
    this.hideNotifications = function () {
        this.notificationActive = false;
        this.notificationText = "";        
    }
});


