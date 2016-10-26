var myContrlApp = angular.module('ApprovalApp',[]);

myContrlApp.service('PersonalizationSvc', function (CommonService) {
    this.GetLoggedInUser = function (serviceHost) {
        var url = serviceHost + "personalization/users/userID";
        return CommonSvc.getJson(url, null);
    }

    this.CreateNewUser = function (serviceHost, fullname, email, selectedapplicationarray, CommonService) {
            var dataobj = {
            FullName: fullname,
            Email: email,
            SelectedApplications: selectedapplicationarray
        }
        var url = serviceHost + "personalization/users/userID";
        return CommonSvc.postJson(url, obj);

    }
});

myContrlApp.service('CommonSvc', function ($http) {
    this.getJson = function (url) {
        return $http({
            method: 'GET',
            url: _spPageContextInfo.webAbsoluteUrl + url,
            headers: { "Accept": "application/json;odata=verbose" }
        }).success(function (data, status, headers, config) {
            //return data.d.results;  
        }).error(function (data, status, headers, config) {
            //return status;
        });
    };
    this.postJson = function (url, dataObject) {
        return $http({
            method: 'POST',
            url: _spPageContextInfo.webAbsoluteUrl + url,
            headers: { "Accept": "application/json;odata=verbose" }
        }).success(function (data, status, headers, config) {
            //return data.d.results;  
        }).error(function (data, status, headers, config) {
            //return status;
        });
    };
    
});