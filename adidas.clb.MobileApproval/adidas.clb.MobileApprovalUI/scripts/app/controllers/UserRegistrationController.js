var myContrlApp = angular.module('ApprovalApp', []);
myContrlApp.controller('UserRegistrationController', function ($scope) {
    $scope.applications = [
         { name: 'CAR', selected: false },
         { name: 'Store', selected: false }
    ]
    //Cancel the New registration
    $scope.CancelMe = function () {
        try {
            $location.path('/');
            $scope.$apply();
        }
        catch (e) {
            // Error logging
        }
    }
    //Submit the new registration details
    $scope.SubmitData = function () {
        try {
            var selectedapplications = [];
            var userFullname = document.getElementById('Fullname').value;
            var userEmail = document.getElementById('Email').value;
            //PersonalizationSvc.CreateNewUser(userFullname, userEmail, selectedapplications).success(function (res) {
       // });
    }
     catch (e) {
         // Error logging
    }
}
   
});