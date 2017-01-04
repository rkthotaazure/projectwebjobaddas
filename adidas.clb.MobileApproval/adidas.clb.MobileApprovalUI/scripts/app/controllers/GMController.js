//AngularJS controller to check user is eisits or not
app.controller('HomeController', function ($scope, $http, $location) {
    //On page load check user is exisits or not 
    $scope.init = function () {
        $scope.userResults = "";
        //Getting user exisits or not from Personalization API redirect to create/update page
        $http.get("/Home/CheckUserExisits").success(function (data) {
            //console.log(data.userResults);
            if(data.userResults!=null)
            {
                //Redirect to create page
                $location.path('/updateUser');
            }
            else {
                //Redirect to update page
                $location.path('/createUser');
            }
        }).error(function (data, status) {
            console.log(data);
        });
    };
});

//AngularJS controller to load backend details and post submitted data to Home Controller
app.controller('CreateUserController', function ($scope, $http, $location) {
    // Bind the backend deatils in CreateNewUser on page load
    $scope.init = function () {
        //Getting backends from Personalization API 
        $http.get("/Home/GetBackends").success(function (data) {
            console.log(data);
            $scope.backends = [];
            $scope.devices = [];
            //Iterate backends details getting from Personalization API and bind the backends object
            angular.forEach(data, function (backendItems) {
                $scope.backends.push(backendItems.backend);
            });
            //console.log($scope.backends)
     }).error(function (data, status) {
            console.log(data);
    });
    };
    //Add New row in My Backends section on click
    $scope.addNewBackend = function (backends) {
        $scope.backends.push({
            'BackendID': "",
            'DefaultUpdateFrequency': "",
        });
    };
    //Add New row in My Devices section by default while page load
    //$scope.devices = [
    //    {
    //        'DeviceName': "",
    //        'DeviceBrand': "",
    //    }];
    //Add New row in My Devices section on click
    $scope.addNewdevices = function () {
        $scope.devices.push({
            'DeviceName': "",
            'DeviceBrand': "",
        });
    };
    //Remove row in My Backends section on click
    $scope.removeBackend = function (backend) {
        for (var i = 0; i < $scope.backends.length; i++) {
            if ($scope.backends[i] === backend) {
                $scope.backends.splice(i, 1);
                break;
            }
        }
    }
    //Remove row in My Devices section on click
    $scope.removedevices = function (device) {
        for (var i = 0; i < $scope.devices.length; i++) {
            if ($scope.devices[i] === device) {
                $scope.devices.splice(i, 1);
                break;
            }
        }
    }
    //Send Submited data to HomeController CreateNewuser action and redirect to Approval Landing page
    $scope.SendData = function () {
        var userbackends = [];
        var updatefrq = "";
        //Iterate backends and push to userbackends variable
        angular.forEach($scope.backends, function (backend) {
         updatefrq = $scope.item;
         userbackends.push({ backend: backend });
        });
        var userdevices = [];
        var currentdevice={
            'DeviceName': $scope.DeviceName,
            'DeviceBrand': $scope.DeviceOS,
        }
        userdevices.push({ device: currentdevice });
        //Iterate userdevices and push to userdevices variable
        angular.forEach($scope.devices, function (deviceitems) {
            userdevices.push({ device: deviceitems });
        });
      //Create the request object
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
        
        //post Submited data to HomeController CreateNew action and redirect to Approval Landing page
        $http.post("/Home/CreateNew", (request), config)
            .success(function (data, status, headers, config) {
                $location.path('/approvalLanding');
            })
            .error(function (data, status, header, config) {
                console.log(data);
            });
    };
});
//AngularJS controller to load user details and post submitted data to Home Controller
app.controller('UpdateUserController', function ($scope, $http, $location, ShareData) {
    // Bind the User deatils in UpdateUser page load
    $scope.init = function () {
        $http.get("/Home/GetUserInfo").success(function (data) {
            console.log(data);
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
            //Iterate backends details and bind to backends scope
            angular.forEach(data.userbackends, function (backendItems) {
                $scope.backends.push(backendItems.backend);
            });
            //Iterate Devices details and bind to devices scope
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
        //Iterate backends and push to userbackends variable
        angular.forEach($scope.backends, function (backend) {
            userbackends.push({ backend: backend});
        });
        var userdevices = [];
        //Iterate devices and push to userdevices variable
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
        //Storing in ShareData factory and retrive where ever we need details
        ShareData.userDevices = userdevices;
        ShareData.userBackends = userbackends;

        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        console.log(request);
        //Post request variable to update user method
        $http.post("/Home/UpdateUser", JSON.stringify(request), config)
            .success(function (data, status, headers, config) {
                //console.log(data);
                //$scope.msg = "Successfully saved";
                //Redirect to approval landing page
                $location.path('/approvalLanding');
               
            }).error(function (data, status, header, config) {
                console.log(data);
            });
    };
    //On click cancel button go to landing page 
    $scope.GotoLandingpage = function () {
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
        //Post request variable to update user method
        $http.post("/Home/UpdateUser", JSON.stringify(request), config)
            .success(function (data, status, headers, config) {
                //console.log(data);
                //$scope.msg = "Successfully saved";
                $location.path('/approvalLanding');
               
            }).error(function (data, status, header, config) {
                console.log(data);
            });
    };
    
});

//AngularJS controller to get backend approval details
app.controller('ApprovalLandingController', function ($scope, $http, $location, ShareData) {
    // Bind the User Backend deatils in approavl landing page load
    $scope.init = function () {
        $scope.requestStatus = [
            { id: 1, name: 'Started' },
            { id: 2, name: 'Completed' },
            { id: 3, name: 'Cancelled' },
            { id: 4, name: 'Unknown' }
        ];
        $scope.approvalStatus = [
           { id: 1, name: 'Waiting' },
           { id: 2, name: 'OnHold' },
           { id: 3, name: 'Approved' },
           { id: 4, name: 'Rejected' },
           { id: 4, name: 'Removed' }
        ];
        $scope.rqStatus = $scope.requestStatus[0];
        $scope.apStatus = $scope.approvalStatus[0];
        ShareData.aprStatus = $scope.apStatus.name;
        ShareData.reqStatus = $scope.rqStatus;
        $scope.backends = [];
        $scope.openRequest = [];
        $scope.openApproval = [];
        $scope.userBackendid = [];
        var userDevices = ShareData.userDevices;
        var userbackends = ShareData.userBackends;
        $scope.userBackendid = [];
        $scope.userBackendName = [];
        angular.forEach(userbackends, function (BackendItems) {
            $scope.userBackendid.push(BackendItems.backend.BackendID);
            $scope.userBackendName.push(BackendItems.backend.BackendName);
        });
        //$scope.forceUpdate = 'false';
        $scope.update = function () {
            $scope.forceUpdate='true';
        };
        if ($scope.forceUpdate == null)
        {
            $scope.forceUpdate = 'false';
        }
        var depthInfo={
            overview:'true',
            genericInfo:'true',
            approvers:'true'
        }
        var filtersinfo=
            {
                backends: $scope.userBackendid,
                backendName: $scope.userBackendName,
                reqStatus: $scope.rqStatus.name,
                apprStatus: $scope.apStatus.name,
                isChanged:'false',
                onlyChangedReq:'false'
            }
        var params={
            forceUpdate: $scope.forceUpdate,
            filters:filtersinfo,
            depth: depthInfo
        }
        //console.log(params)
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
        //Post request variable to update user method
        $http.post("/Landing/GetBackendApprovalrequestcount", requestsync, config).success(function (data) {
            console.log(data);
            angular.forEach(data, function (BackendItems) {
                //$scope.openRequest.push({ OpenRequests: BackendItems.backend.OpenRequests });
                //$scope.openApproval.push({ OpenApprovals: BackendItems.backend.OpenApprovals });
                $scope.backends.push({ BackendID: BackendItems.BackendID, BackendName: BackendItems.BackendName, Pending: BackendItems.WaitingCount, Approved: BackendItems.ApprovedCount, Rejected: BackendItems.RejectedCount });
            });
            $scope.pending = $scope.sum($scope.backends, 'Pending');
            var Approved= $scope.sum($scope.backends, 'Approved');
            var Rejected=$scope.sum($scope.backends, 'Rejected');
            $scope.completed = Approved + Rejected;
            ShareData.backendCount = $scope.backends;
            console.log($scope.backends);
            //$(".progress-bar-success").css("width", ($scope.Completed / openRequests)*100 + "%");
            //$(".progress-bar-danger").css("width", ($scope.Pending / openRequests) * 100 + "%");
        }).error(function (data, status) {
            console.log(data);
        });
    };
    //$scope.SyncUpdate = function () {
    //    window.location.reload();
    //};
    //On click completed
    $scope.completedCount = function () {
        var backendCount = ShareData.backendCount;
        var approvalStatus = "Completed";
        ShareData.aprStatus = approvalStatus;
        $scope.backends=[]
        angular.forEach(backendCount, function (backend) {
            var Approved = backend.Approved;
            var Rejected = backend.Rejected;
            var Completed = Approved + Rejected;
            $scope.backends.push({ BackendID: backend.BackendID, BackendName: backend.BackendName, Pending: Completed });
        });
    }
    //On click pending
    $scope.pendingCount = function () {
        var backendCount = ShareData.backendCount;
        var approvalStatus = "Waiting";
        ShareData.aprStatus = approvalStatus;
        $scope.backends = []
        angular.forEach(backendCount, function (backend) {
            $scope.backends.push({ BackendID: backend.BackendID, BackendName: backend.BackendName, Pending: backend.Pending });
        });
    }
    //On click backend count
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
//AngularJS controller to get pending approval details
app.controller('ApprovalDetailsController', function ($scope, $http, $location, $filter, ShareData) {
    $scope.init = function () {
        $scope.loading = true;
        $scope.hideButton = true;
        var backendid = ShareData.backendId;
        var userDevices = ShareData.userDevices;
        var approvalStatus = ShareData.aprStatus;
        var requestStatus= ShareData.reqStatus;
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
                reqStatus: requestStatus.name,
                apprStatus: approvalStatus,
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
       
        //console.log(requestsync);
        //Post request variable to GetApprovalDetails method
        $http.post("/Landing/GetApprovalDetails", requestsync, config).success(function (data) {
            //console.log(data);
            
            $scope.hideButton = true;
             angular.forEach(data, function (approvalItems) {
                 $scope.approvalTasks.push(approvalItems.approval);
             });
             if (approvalStatus == "Completed") {
                 $scope.hideButton = false;
             }
             $scope.currentPage = 1;
             $scope.totalItems = $scope.approvalTasks.length;
             $scope.numPerPage = 5;
             $scope.paginate = function (value) {
                 var begin, end, index;
                 begin = ($scope.currentPage - 1) * $scope.numPerPage;
                 end = begin + $scope.numPerPage;
                 index = $scope.approvalTasks.indexOf(value);
                 return (begin <= index && index < end);
             };
             $scope.loading = false;
        }).error(function (data, status) {
            $scope.loading = false;
             console.log(data);
        });      
    };
   // Show pendind task request information
    $scope.showDetails = function (requestId) {
        ShareData.detailTaskinfo = requestId;
        $location.path('/detailTaskInfo');
    };
    $scope.requestStatus = function (status, requestId) {
        $scope.decisionDate = $filter('date')(new Date(), 'yyyy-MM-dd HH:mm');
        var userDevices = ShareData.userDevices;

        var approvalDecision={
                DecisionDate: $scope.decisionDate,
                Comment:"Approved",
                Status:status
        }
        var approvalDetails = {
            ApprovalRequestID: requestId,
            ApprovalDecision:approvalDecision,
            DeviceID: ShareData.userDevices.DeviceID
        }
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        //console.log(approvalDetails);

        //Post request variable to SendApprovalstatus method
        $http.post("/Landing/SendApprovalstatus", approvalDetails, config).success(function (data) {
            //console.log(data);
                for (var i = 0; i < $scope.approvalTasks.length; i++) {
                    if ($scope.approvalTasks[i].RequestId === requestId) {
                        $scope.approvalTasks.splice(i, 1);
                        break;
                    }
                }
            //$location.path('/approvalDetails');
        }).error(function (data, status) {
            console.log(data);
        });
    };
    $scope.redirect = function () {
        $location.path('/approvalLanding');
    };
    //Remove row in Pending Approvals
    
});
//AngularJS controller to get details request information 
app.controller('ApprovalDetailstaskController', function ($scope, $http, $location,$filter, ShareData) {
    $scope.init = function () {
        $scope.hideButton = true;
        var requestId = ShareData.detailTaskinfo;
        var userDevices = ShareData.userDevices;
        var approvalStatus = ShareData.aprStatus;
        var requestStatus = ShareData.reqStatus;
        var depthInfo = {
            overview: 'true',
            genericInfo: 'true',
            approvers: 'true'
        }
        var filtersinfo =
            {
                backends: $scope.backend,
                reqStatus: requestStatus.name,
                apprStatus: approvalStatus.name,
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
        $scope.approvers = [];
        //Post request variable to GetRequestDetails method
        $http.post("/Landing/GetRequestDetails", requestDetails, config).success(function (data) {
            //console.log(data);
            $scope.hideButton = true;
            angular.forEach(data, function (approvalItems) {
                $scope.id = approvalItems.request.ID;
                $scope.title = approvalItems.request.Title;
                $scope.status = approvalItems.request.Status;
                $scope.created = approvalItems.request.Created;
                $scope.userID = approvalItems.request.Requester.UserID;
                $scope.name = approvalItems.request.Requester.Name;
                angular.forEach(approvalItems.request.Fields, function (overviewFields)
                {
                    $scope.fields.push(overviewFields);
                });
                angular.forEach(approvalItems.request.Approvers, function (approverlist) {
                    $scope.approvers.push(approverlist);
                });
            });
            if (approvalStatus == "Completed") {
                $scope.hideButton = false;
            }
        }).error(function (data, status) {
            console.log(data);
        });
    };

    $scope.requestStatus = function (status, requestId) {
        var userDevices = ShareData.userDevices;
        $scope.decisionDate = $filter('date')(new Date(), 'yyyy-MM-dd HH:mm');
        var approvalDecision = {
            DecisionDate: $scope.decisionDate,
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