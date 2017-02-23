var app = angular.module('ApplicationModule', ['ngRoute', 'ngResource', 'ui.bootstrap']);

app.factory("ShareData", function () {
    return { backendId: '', detailTaskinfo: '', userDevices: {}, userBackends: {}, aprStatus: '', reqStatus: '', backendCount: {}, ShowwaitingMessage:false }
});
//Showing Routing
app.config(['$routeProvider', function ($routeProvider) {
    //debugger;
    $routeProvider.when('/updateUser',
                        {
                            templateUrl: 'Home/UpdateUser'
                        })
                    .when('/createUser',
                        {
                        templateUrl: 'Home/CreateUser'
                        })
                   .when('/approvalLanding',
                        {
                            templateUrl: 'Landing/ApprovalLanding'
                        })
                    .when('/approvalDetails',
                        {
                            templateUrl: 'Landing/ApprovalDetails'
                        })
                     .when('/detailTaskInfo',
                        {
                            templateUrl: 'Landing/DetailTaskInfo'
                        })
                    .otherwise(
                            {
                              redirectTo: '/'
                            });
}]);