/**
    * @desc : fetch the backend details content
    * @injection : GetJSON to call the REST api
*/
app.service('CommonService', function ($rootScope, $http) {
    this.backend = "";
    this.pageTitle = "";    
    this.notificationActive = false;
    this.notificationText = "";
    this.newTasks = -1;
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

    this.scrollToTop = function () {
        jQuery(document).ready(function () {
            jQuery("html, body").animate({
                scrollTop: 0
            }, 600);
        });
    };

    this.updateRequestCount = function () {
        var serviceRef = this;
        $http.post("/Landing/GetNewRequestCount ").success(function (data) {
            serviceRef.newTasks = data.UnReadRequestsCount;
        });
    };
});


