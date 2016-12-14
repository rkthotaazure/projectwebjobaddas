app.controller('HomeController', function ($scope, $http, $location) {
    $scope.init = function () {
        $scope.userResults ="" ;
        $http.get("/Home/CheckUserExisits").success(function (data) {
            console.log(data.userResults);
            if(data.userResults!=null)
            { 
                $location.path('/updateUser');
            }
            else {
                $location.path('/createUser');
            }
        }).error(function (data, status) {
            console.log(data);
        });
    };
});

//AngularJS controller to load backend details and post submitted data to Home Controller
app.controller('CreateUserController', function ($scope, $http, $location) {
    // Bind the backend deatils in CreateNewUser page load
    $scope.init = function () {
        $scope.frequency = [{
            id: 1,
            name: 'hours'
        }, {
            id: 2,
            name: 'days'
        }];
        $http.get("/Home/GetBackends").success(function (data) {
            //console.log(data);
            $scope.backends = [];
            angular.forEach(data, function (backendItems) {
                $scope.backends.push(backendItems.backend);
            });
            console.log($scope.backends)
     }).error(function (data, status) {
            console.log(data);
    });
    };
    //Add New row in My Backends section
    $scope.addNewBackend = function (backends) {
        $scope.backends.push({
            'BackendID': "",
            'DefaultUpdateFrequency': "",
        });
    };
    //Add New row in My Devices section
    $scope.devices = [
        {
            'DeviceName': "",
            'DeviceBrand': "",
        }];
    $scope.addNewdevices = function (devices) {
        $scope.devices.push({
            'DeviceName': "",
            'DeviceBrand': "",
        });
    };
    //Remove row in My Backends section
    $scope.removeBackend = function (backend) {
        for (var i = 0; i < $scope.backends.length; i++) {
            if ($scope.backends[i] === backend) {
                $scope.backends.splice(i, 1);
                break;
            }
        }
    }
    //Remove row in My Devices section
    $scope.removedevices = function (device) {
        for (var i = 0; i < $scope.devices.length; i++) {
            if ($scope.devices[i] === device) {
                $scope.devices.splice(i, 1);
                break;
            }
        }
    }
    //Post Submited datat to HomeController CreateNewuser action
    $scope.SendData = function () {
        var userbackends = [];
        var updatefrq = "";
        
        angular.forEach($scope.backends, function (backend) {
         updatefrq = $scope.item;
         userbackends.push({ backend: backend });
        });
        
        var userdevices = [];
        //var devices = [];
        //userdevices.device = devices;
        var currentdevice={
            'DeviceName': $scope.DeviceName,
            'DeviceBrand': $scope.DeviceOS,
        }
        userdevices.push({ device: currentdevice });
        angular.forEach($scope.devices, function (deviceitems) {
            userdevices.push({ device: deviceitems });
        });
        var request = {
            FirstName: $scope.FirstName,
            LastName: $scope.LastName,
            Fullname: $scope.Fullname,
            Email: $scope.Email,
            Domain: $scope.Domain,
            DeviceName: $scope.DeviceName,
            DeviceOS:$scope.DeviceOS,
            userbackends: userbackends,
            userdevices: userdevices
        }
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        //console.log(UserDetails);
        console.log(request);
        //console.log(userdevices);
        $http.post("/Home/CreateNew", (request), config)
            .success(function (data, status, headers, config) {
                ////console.log(data);
                ////if (data != '') {
                //    $scope.msg = data;
                //    $scope.FirstName = '';
                //    $scope.LastName = '';
                //    $scope.Email = '';
                //    $scope.Domain = '';
                //    $scope.DeviceName = '';
                //    $scope.Fullname = '';
                //    $scope.msg = "Successfully saved";
                $location.path('/approvalLanding');
                    
                //}
            })
            .error(function (data, status, header, config) {
                console.log(data);
            });
    };
});
//AngularJS controller to load user details and post submitted data to Home Controller
app.controller('UpdateUserController', function ($scope, $http, $location, ShareData) {
    //$scope.userDetails = {FirstName:'',LastName:''}
  
   
    // Bind the User deatils in UpdateUser page load
    $scope.init = function () {
        $http.get("/Home/GetUserInfo").success(function (data) {
            //console.log(data);
            //$scope.userDetails = data;
            $scope.FirstName = data.FirstName;
            $scope.LastName = data.LastName;
            $scope.Fullname = data.Fullname;
            $scope.Domain = data.Domain;
            $scope.Email = data.Email;
            $scope.DeviceName = data.DeviceName;
            $scope.DeviceOS = data.DeviceOS;
            $scope.backends = [];
            $scope.devices = [];
            $scope.frequency = [{
                  id: 1,  
                  name: 'hours'  
              }, {
                  id: 2,  
                  name: 'days'  
              }];
            angular.forEach(data.userbackends, function (backendItems) {
                $scope.backends.push(backendItems.backend);
            });
            angular.forEach(data.userdevices, function (deviceItems) {
                    $scope.devices.push(deviceItems.device);
            });
            
        }).error(function (data, status) {
            console.log(data);
        });
    };
    //Add New row in My Backends section
    $scope.addNewBackend = function (backends) {
        $scope.backends.push({
            'BackendID': "",
            'DefaultUpdateFrequency': "",
        });
    };
    //Add New row in My Devices section
    $scope.addNewdevices = function (devices) {
        $scope.devices.push({
            'DeviceName': "",
            'DeviceBrand': "",
        });
    };
    //Remove row in My Backends section
    $scope.removeBackend = function (backend) {
        for (var i = 0; i < $scope.backends.length; i++) {
            if ($scope.backends[i] === backend) {
                $scope.backends.splice(i, 1);
                break;
            }
        }
    }
    //Remove row in My Devices section
    $scope.removedevices = function (device) {
        for (var i = 0; i < $scope.devices.length; i++) {
            if ($scope.devices[i] === device) {
                $scope.devices.splice(i, 1);
                break;
            }
        }
    }
    //Post Submited datat to HomeController UpdateUser action
    $scope.SendData = function () {
        var userbackends = [];
        angular.forEach($scope.backends, function (backend) {
            userbackends.push({ backend: backend});
        });
        var userdevices = [];
        angular.forEach($scope.devices, function (device) {
            userdevices.push({ device: device });
        });
        var request = {
            FirstName: $scope.FirstName,
            LastName: $scope.LastName,
            Fullname:$scope.Fullname,
            Email: $scope.Email,
            Domain: $scope.Domain,
            DeviceName: $scope.DeviceName,
            DeviceOS: $scope.DeviceOS,
            userbackends: userbackends,
            userdevices: userdevices
        }
        ShareData.userDevices = userdevices;
        ShareData.userBackends = userbackends;

        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
         console.log(request);
        $http.post("/Home/UpdateUser", JSON.stringify(request), config)
            .success(function (data, status, headers, config) {
                //console.log(data);
                //$scope.msg = "Successfully saved";
                $location.path('/approvalLanding');
               
            }).error(function (data, status, header, config) {
                console.log(data);
            });
    };
    $scope.GotoLandingpage = function () {
        $location.path('/approvalLanding');
    };
    
});

//AngularJS controller to get backend approval details
app.controller('ApprovalLandingController', function ($scope, $http, $location, ShareData) {
    // Bind the User Backend deatils in approavl landing page load
    $scope.init = function () {
        $scope.backends = [];
        $scope.openRequest = [];
        $scope.openApproval = [];
        var userDevices = ShareData.userDevices;
        var userbackends = ShareData.userBackends;
        $scope.userBackendid = [];
        angular.forEach(userbackends, function (BackendItems) {
        $scope.userBackendid.push(BackendItems.backend.BackendID)
        });
        var depthInfo={
            overview:'true',
            genericInfo:'true',
            approvers:'true'
        }
        var filtersinfo=
            {
                backends: $scope.userBackendid,
                reqStatus:'',
                apprStatus:'',
                isChanged:'false',
                onlyChangedReq:'false'
            }
        var params={
            forceUpdate:'false',
            filters:filtersinfo,
            depth: depthInfo
        }
        var requestsync = {
            client: userDevices[0],
            parameters:params
        }
        console.log(requestsync);
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        $http.post("/Landing/GetBackendApprovalrequestcount", requestsync, config).success(function (data) {
            console.log(data);
            angular.forEach(data, function (BackendItems) {
                $scope.openRequest.push({ OpenRequests: BackendItems.backend.OpenRequests });
                $scope.openApproval.push({ OpenApprovals: BackendItems.backend.OpenApprovals });
                $scope.backends.push(BackendItems.backend);
            });
            $scope.pending = $scope.sum($scope.openApproval, 'OpenApprovals');
            //console.log($scope.Pending);
            var openRequests = $scope.sum($scope.openRequest, 'OpenRequests');
            $scope.completed = (openRequests - $scope.pending);
            //$(".progress-bar-success").css("width", ($scope.Completed / openRequests)*100 + "%");
            //$(".progress-bar-danger").css("width", ($scope.Pending / openRequests) * 100 + "%");
        }).error(function (data, status) {
            console.log(data);
        });
    };
    $scope.SyncUpdate = function () {
        window.location.reload();
    };
    $scope.redirectToDetailsPage = function (backendid) {
        ShareData.backendId = backendid;
        $location.path('/approvalDetails');
    };
    $scope.sum = function (items, prop) {
        return items.reduce(function (a, b) {
            return a + b[prop];
        }, 0);
    };
});
//AngularJS controller to get Individual backend approval details
app.controller('ApprovalDetailsController', function ($scope, $http, $location, ShareData) {
    $scope.init = function () {
        var backendid = ShareData.backendId;
        var userDevices = ShareData.userDevices;
        $scope.backend = ShareData.backendId;
        $scope.approvalTasks = [];
        var depthInfo = {
            overview: 'true',
            genericInfo: 'true',
            approvers: 'true'
        }
        var filtersinfo =
            {
                backends: $scope.backend,
                reqStatus: '',
                apprStatus: '',
                isChanged: 'false',
                onlyChangedReq: 'false'
            }
        var params = {
            forceUpdate: 'false',
            filters: filtersinfo,
            depth: depthInfo
        }
        var requestsync = {
            client: userDevices[0],
            parameters: params
        }
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        console.log(requestsync);
        $http.post("/Landing/GetApprovalDetails", requestsync, config).success(function (data) {
            console.log(data);
             angular.forEach(data, function (approvalItems) {
                 $scope.approvalTasks.push(approvalItems.request);
             });
         }).error(function (data, status) {
             console.log(data);
         });
    };
    $scope.showDetails = function (requestId) {
        ShareData.detailTaskinfo = requestId;
        $location.path('/detailTaskInfo');
    };
    $scope.requestStatus = function (status,requestId) {
        var userDevices = ShareData.userDevices;

        var approvalDecision={
                DecisionDate:'15/12/2016',
                Comment:"Approved",
                Status:status
        }
        var approvalDetails = {
            ApprovalRequesID: requestId,
            ApprovalDecision:approvalDecision,
            DeviceID: ShareData.userDevices.DeviceID
        }
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        console.log(approvalDetails);
        $http.post("/Landing/SendApprovalstatus", approvalDetails, config).success(function (data) {
            console.log(data);
            
        }).error(function (data, status) {
            console.log(data);
        });
    };
    $scope.redirect = function () {
        $location.path('/approvalLanding');
    };
});
app.controller('ApprovalDetailstaskController', function ($scope, $http, $location, ShareData) {
    $scope.init = function () {
        var requestId = ShareData.detailTaskinfo;
        var userDevices = ShareData.userDevices;
        var depthInfo = {
            overview: 'true',
            genericInfo: 'true',
            approvers: 'true'
        }
        var filtersinfo =
            {
                backends: $scope.backend,
                reqStatus: '',
                apprStatus: '',
                isChanged: 'false',
                onlyChangedReq: 'false'
            }
        var params = {
            forceUpdate: 'false',
            filters: filtersinfo,
            depth: depthInfo
        }
        var requestsync = {
            client: userDevices[0],
            parameters: params
        }
        var requestDetails = {
            syncRequest: requestsync,
            requestID: requestId
        }
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        console.log(requestDetails);
        $scope.fields = [];
        $http.post("/Landing/GetRequestDetails", requestDetails, config).success(function (data) {
            console.log(data);
            angular.forEach(data, function (approvalItems) {
                $scope.id = approvalItems.request.id;
                $scope.title = approvalItems.request.title;
                $scope.status = approvalItems.request.status;
                $scope.created = approvalItems.request.created;
                $scope.userID = approvalItems.request.requester.userID;
                $scope.name = approvalItems.request.requester.name;
                angular.forEach(approvalItems.request.fields, function (overviewFields)
                {
                    $scope.fields.push(overviewFields);
                });
            });
        }).error(function (data, status) {
            console.log(data);
        });
    };

    $scope.requestStatus = function (status, requestId) {
        var userDevices = ShareData.userDevices;

        var approvalDecision = {
            DecisionDate: '15/12/2016',
            Comment: "Approved",
            Status: status
        }
        var approvalDetails = {
            ApprovalRequesID: requestId,
            ApprovalDecision: approvalDecision,
            DeviceID: ShareData.userDevices.DeviceID
        }
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        console.log(approvalDetails);
        $http.post("/Landing/SendApprovalstatus", approvalDetails, config).success(function (data) {
            console.log(data);

        }).error(function (data, status) {
            console.log(data);
        });
    };
    $scope.redirect = function () {
        $location.path('/approvalDetails');
    };
});
app.filter("dateFilter", function () {
    return function (item) {
        if (item != null) {
            return new Date(parseInt(item.substr(6)));
        }
        return "";
    };
});