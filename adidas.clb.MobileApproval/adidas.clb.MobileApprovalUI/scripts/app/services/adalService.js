var myContrlApp = angular.module('MyApp');

myContrlApp.factory('adalService', ['adalAuthenticationService', function (adalService) {
   
    return adalService;
}]);