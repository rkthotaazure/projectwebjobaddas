var myContrlApp = angular.module('ApprovalApp');

myContrlApp.service('PersonalizationSvc', function (CommonSvc) {
    this.GetLoggedInUser = function (serviceHost) {
        var url = serviceHost + "personalization/users/userID";
        return CommonSvc.AsynchronousGetCall(url, null);

    }

    this.CreateNewUser = function (serviceHost, fullname, email, selectedapplicationarray) {

        var obj = {
            FullName: fullname,
            Email: email,
            SelectedApplications: selectedapplicationarray
        }
        var url = serviceHost + "personalization/users/userID";
        return CommonSvc.Asynchronous

    }
});
