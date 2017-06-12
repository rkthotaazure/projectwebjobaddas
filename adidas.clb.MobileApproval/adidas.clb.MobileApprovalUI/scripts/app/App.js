var app = angular.module('ApplicationModule', ['ngRoute', 'ngResource', 'ui.bootstrap', 'ngDialog']);
setTimeout(function () {
    document.getElementById('splash').className = "active";
}, 1000);

    setTimeout(function () {
        document.getElementById('splash').className = "ma-close";
        setTimeout(function () {
            document.getElementById('splash').className = "";
            document.getElementById('splash').style.display = "none";
        }, 1000);
    }, 4000);

app.factory("ShareData", function () {
    return { backendId: '', detailTaskinfo: '', detailTaskId: '', userDevices: {}, userBackends: {}, aprStatus: '', reqStatus: '', backendCount: {}, ShowwaitingMessage: false, pendingtasks: {}, completedtasks: {} }
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
app.directive('myDir', function ($compile) {
    return function (scope, element, attrs) {
        scope.doSomething(element);
    };
});