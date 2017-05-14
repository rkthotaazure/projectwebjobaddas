/**
    * @desc : fetch the backend details content
    * @injection : GetJSON to call the REST api
*/
app.service('CommonService', function ($rootScope) {
    this.backend = "";
    this.pageTitle = "";    
    this.notificationActive = false;
    this.notificationText = "";
    this.showNotifications = function (notificationText, notificationType) {
        this.notificationActive = true;
        this.notificationText = notificationText;
        this.hideNotificationAfter();
    };
    this.hideNotifications = function () {
        this.notificationActive = false;
        this.notificationText = "";
    };
    this.hideNotificationAfter = function () {
        var serviceRef = this;
        setTimeout(function () {
            serviceRef.hideNotifications();
            $rootScope.$digest();
        }, 10000);
    };
});


