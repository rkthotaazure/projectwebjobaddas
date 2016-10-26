    var myContrlApp = angular.module('ApprovalApp', ['ngRoute', 'ngResource',' ui.bootstrap']);

      myContrlApp.config(['$routeProvider', function ($routeProvider) {
        $routeProvider.when('/', {
            templateUrl: '/LandingPage.html',
            controller: 'LandingController'            
        })
                   
    }]);




