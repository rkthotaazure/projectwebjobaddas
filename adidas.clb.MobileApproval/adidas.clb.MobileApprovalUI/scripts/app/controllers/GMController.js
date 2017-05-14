//AngularJS controller to check user is eisits or not
app.controller('HomeController', function ($scope, $http, $location, $route, ShareData, CommonService) {
    $scope.cs = CommonService;
    $scope.showSideMenu = false;    
    //On page load check user is exisits or not 
    $scope.init = function () {
        $scope.userResults = "";
        $http.get("/UserProfile/LogedinUser").success(function (data) {            
            $scope.Email = data.Mail;
                $scope.Fullname = data.DisplayName;
                }).error(function (data, status) {
            console.log(data);
                });
        //Getting user exisits or not from Personalization API redirect to create/update page
        $http.get("/Home/CheckUserExisits").success(function (data) {            
            if (data.UserID != null) {
                //Getting Loged in user details from Azure AD ADAL
                var userbackends = [];
                var userdevices = [];
                //Storing in ShareData factory and retrive where ever we need details
                //Iterate backends details and bind to backends scope
                angular.forEach(data.userbackends, function (backendItems) {
                    userbackends.push(backendItems.backend);
                });
                //Iterate Devices details and bind to devices scope
                angular.forEach(data.userdevices, function (deviceItems) {
                    userdevices.push(deviceItems.device);
                });
                ShareData.userDevices = data.userdevices;
                ShareData.userBackends = data.userbackends;
                ShareData.CompletedRequestsSync = data.CompletedRequestsSync;
                ShareData.AutoRefreshValue = data.AutoRefreshValue;
                ShareData.ShowwaitingMessage = false;                
                //if update user stay in that page to update user, otherwise redirect to approvallanding page
                if ($location.$$path != '/updateUser') {
                    //Redirect to landing page
                    if ($location.$$path == '/approvalLanding') {                        
                        $route.reload('/approvalLanding');
                    }
                    else {
                        ShareData.reload = true;
                        $location.path('/approvalLanding');
                    }
                }


            }
            else {
                //Redirect to create page                
                $location.path('/createUser');
            }
        }).error(function (data, status) {
            console.log(data);
        });
    };
    $scope.sideMenuToggle = function () {
        $scope.showSideMenu = true
        if ($location.$$path != '/createUser') {
            $scope.showUpdateProfile = true;
        }
        else {
            $scope.showUpdateProfile = false;
        }
    }
    $scope.redirectcontrol = function (path) {
        $location.path('/' + path);
        $scope.showSideMenu = false;

    }
});

//AngularJS controller to load backend details and post submitted data to Home Controller
app.controller('CreateUserController', function ($scope, $http, $location, ShareData, CommonService) {
    $scope.cs = CommonService;
    $scope.completedTaskOptions = [];
    $scope.selectedAutoRefreshOption = { value: 10, text: "10 minutes" };
    $scope.autoRefreshOptions = [];

    // Bind the backend deatils in CreateNewUser on page load
    $scope.init = function () {
        $scope.cs.pageTitle = "Create Profile";
        $scope.completedTaskOptions = [
            { text: "1 week", value: 7, classText: "" },
            { text: "2 weeks", value: 14, classText: "active" },
            { text: "3 weeks", value: 21, classText: "" },
            { text: "1 month", value: 30, classText: "" },
            { text: "2 months", value: 60, classText: "" },
            { text: "3 months", value: 90, classText: "" }
        ];

        for (var count = 1; count <= 10; count++) {
            $scope.autoRefreshOptions.push({
                value: count,
                text: count + " minutes"
            });
        }
        $scope.autoRefreshOptions[0].text = "1 minute";

        //Getting backends from Personalization API 
        $http.get("/Home/GetBackends").success(function (data) {            
            $scope.backends = [];
            $scope.devices = [];
            //Iterate backends details getting from Personalization API and bind the backends object
            angular.forEach(data, function (backendItems) {
                $scope.backends.push(backendItems.backend);
            });            
        }).error(function (data, status) {
            console.log(data);
        });
        //Getting Loged in user details from Azure AD ADAL 
        $http.get("/UserProfile/LogedinUser").success(function (data) {            
            $scope.Email = data.Mail;
            $scope.Fullname = data.DisplayName;
        }).error(function (data, status) {
            console.log(data);
        });
    };


    //Change completed task selection
    $scope.selectCompletedTaskOption = function (option) {
        $scope.completedTaskOptions.forEach(function (item) {
            item.classText = "";
        });
        option.classText = "active";
    };

    //change auto refresh selection
    $scope.changeAutoRefreshOption = function (autoRefreshOption) {
        $scope.selectedAutoRefreshOption.value = autoRefreshOption.value;
        $scope.selectedAutoRefreshOption.text = autoRefreshOption.text;
    };

    //Add New row in My Backends section on click
    $scope.addNewBackend = function (backends) {
        $scope.backends.push({
            'BackendID': "",
            'DefaultUpdateFrequency': ""
        });
    };
    
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
        $scope.showloader = true;
        var userbackends = [];
        var updatefrq = "";
        //Iterate backends and push to userbackends variable
        angular.forEach($scope.backends, function (backend) {
            updatefrq = $scope.item;
            userbackends.push({ backend: backend });
        });
        var userdevices = [];
        var currentdevice = {
            'DeviceName': $scope.DeviceName,
            'DeviceBrand': $scope.DeviceOS,
        }
        userdevices.push({ device: currentdevice });
        //Iterate userdevices and push to userdevices variable
        angular.forEach($scope.devices, function (deviceitems) {
            userdevices.push({ device: deviceitems });
        });
        //completed task config value selection
        var completedTaskOption = 14;
        $scope.completedTaskOptions.forEach(function (option) {
            if (option.classText === "active") {
                completedTaskOption = option.value;
            }
        });
        //auto refresh value is in $scope.selectedAutoRefreshOption.value in minutes

        //Storing in ShareData factory and retrive where ever we need details
        ShareData.userDevices = userdevices;
        ShareData.userBackends = userbackends;
        ShareData.ShowwaitingMessage = true;
        //Create the request object
        var request = {
            FirstName: $scope.FirstName,
            LastName: $scope.LastName,
            Fullname: $scope.Fullname,
            Email: $scope.Email,
            Domain: $scope.Domain,
            DeviceName: $scope.DeviceName,
            DeviceOS: $scope.DeviceOS,
            userbackends: userbackends,
            userdevices: userdevices,
            CompletedRequestsSync: completedTaskOption,
            AutoRefreshValue: $scope.selectedAutoRefreshOption.value
        }
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }        
        //post Submited data to HomeController CreateNew action and redirect to Approval Landing page
        $http.post("/Home/CreateNew", (request), config)
            .success(function (data, status, headers, config) {
                window.setTimeout(function () {
                    $scope.$apply(function () {
                        $scope.showloader = false;
                    });
                }, 1500);                
                $scope.cs.showNotifications("Profile Craeted Successfully.", "");
                ShareData.reload = true;
                window.setTimeout(function () {
                    $scope.$apply(function () {
                        $location.path('/approvalLanding');
                    });
                }, 1500);
            })
            .error(function (data, status, header, config) {
                console.log(data);
            });
    };
});
//AngularJS controller to load user details and post submitted data to Home Controller
app.controller('UpdateUserController', function ($scope, $http, $location, ShareData, CommonService) {
    $scope.cs = CommonService;
    $scope.completedTaskOptions = [];
    $scope.selectedAutoRefreshOption = { };
    $scope.autoRefreshOptions = [];
    // Bind the User deatils in UpdateUser page load
    $scope.init = function () {
        $scope.cs.pageTitle = "Update Profile";
        $scope.showloader = true;

        $scope.completedTaskOptions = [
            { text: "1 week", value: 7, classText: "" },
            { text: "2 weeks", value: 14, classText: "" },
            { text: "3 weeks", value: 21, classText: "" },
            { text: "1 month", value: 30, classText: "" },
            { text: "2 months", value: 60, classText: "" },
            { text: "3 months", value: 90, classText: "" }
        ];

        for (var count = 1; count <= 10; count++) {
            $scope.autoRefreshOptions.push({
                value: count,
                text: count + " minutes"
            });
        }
        $scope.autoRefreshOptions[0].text = "1 minute";

        $http.get("/Home/GetUserInfo").success(function (data) {
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.showloader = false;
                });
            }, 1500);          
            $scope.FirstName = data.FirstName;
            $scope.LastName = data.LastName;
            $scope.Fullname = data.Fullname;
            $scope.Domain = data.Domain;
            $scope.Email = data.Email;
            $scope.DeviceName = data.DeviceName;
            $scope.DeviceOS = data.DeviceOS;
            ShareData.CompletedRequestsSync = data.CompletedRequestsSync;
            if (data.AutoRefreshValue != null) {
                if (data.AutoRefreshValue > 1) {
                    $scope.selectedAutoRefreshOption = { value: data.AutoRefreshValue, text: data.AutoRefreshValue + " minutes" };
                }
                else {
                    $scope.selectedAutoRefreshOption = { value: data.AutoRefreshValue, text: data.AutoRefreshValue + " minute" };
                }
                ShareData.AutoRefreshValue = data.AutoRefreshValue;
            }
            else {
                ShareData.AutoRefreshValue = 1;
            }
            //Checking completed tasks config value
            var completedFlag = false;
            if (data.CompletedRequestsSync != null) {
                $scope.completedTaskOptions.forEach(function (option) {
                    if (option.value == data.CompletedRequestsSync) {
                        option.classText = "active";
                        completedFlag = true;
                    }
                });
            }
            if (!completedFlag) {
                $scope.completedTaskOptions[1].classText = "active";
            }
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

    //Change completed task selection
    $scope.selectCompletedTaskOption = function (option) {
        $scope.completedTaskOptions.forEach(function (item) {
            item.classText = "";
        });
        option.classText = "active";
    };

    //change auto refresh selection
    $scope.changeAutoRefreshOption = function (autoRefreshOption) {
        $scope.selectedAutoRefreshOption.value = autoRefreshOption.value;
        $scope.selectedAutoRefreshOption.text = autoRefreshOption.text;
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
    };
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
        $scope.showloader = true;
        var userbackends = [];
        //Iterate backends and push to userbackends variable
        angular.forEach($scope.backends, function (backend) {
            userbackends.push({ backend: backend });
        });
        var userdevices = [];
        //Iterate devices and push to userdevices variable
        angular.forEach($scope.devices, function (device) {
            userdevices.push({ device: device });
        });
        var completedTaskOption = 14;
        $scope.completedTaskOptions.forEach(function(option) {
            if (option.classText === "active") {
                completedTaskOption = option.value;
            }
        });
        var request = {
            FirstName: $scope.FirstName,
            LastName: $scope.LastName,
            Fullname: $scope.Fullname,
            Email: $scope.Email,
            Domain: $scope.Domain,
            DeviceName: $scope.DeviceName,
            DeviceOS: $scope.DeviceOS,
            userbackends: userbackends,
            userdevices: userdevices,
            CompletedRequestsSync: completedTaskOption,
            AutoRefreshValue: $scope.selectedAutoRefreshOption.value,
        }
        //Storing in ShareData factory and retrive where ever we need details
        ShareData.userDevices = userdevices;
        ShareData.userBackends = userbackends;
        ShareData.CompletedRequestsSync = completedTaskOption;
        ShareData.AutoRefreshValue = $scope.selectedAutoRefreshOption.value;
        ShareData.ShowwaitingMessage = false;
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        //Post request variable to update user method
        $http.post("/Home/UpdateUser", JSON.stringify(request), config)
            .success(function (data, status, headers, config) {                
                //Redirect to approval landing page
                window.setTimeout(function () {
                    $scope.$apply(function () {
                        $scope.showloader = false;
                    });
                }, 1500);               
                $scope.cs.showNotifications("Profile Updated Successfully.", "");
                window.setTimeout(function () {
                    $scope.$apply(function () {
                        $location.path('/approvalLanding');
                    });
                }, 1500);
            }).error(function (data, status, header, config) {
                console.log(data);
            });
    };
    //On click cancel button go to landing page 
    $scope.GotoLandingpage = function () {
        $location.path('/approvalLanding');
        /*var userbackends = [];
        angular.forEach($scope.backends, function (backend) {
            userbackends.push({ backend: backend });
        });
        var userdevices = [];
        angular.forEach($scope.devices, function (device) {
            userdevices.push({ device: device });
        });
        var request = {
            FirstName: $scope.FirstName,
            LastName: $scope.LastName,
            Fullname: $scope.Fullname,
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
        //Post request variable to update user method
        $http.post("/Home/UpdateUser", JSON.stringify(request), config)
            .success(function (data, status, headers, config) {                
                $location.path('/approvalLanding');

            }).error(function (data, status, header, config) {
                console.log(data);
            });*/
    };

});

//AngularJS controller to get backend approval details
app.controller('ApprovalLandingController', function ($scope, $http, $location, ShareData, $interval, CommonService, $timeout) {
    $scope.cs = CommonService;
    $scope.tasksuccess = ShareData.ShowwaitingMessage;
    $scope.showloader = true;
    $scope.showcontent = false;
    // Bind the User Backend deatils in approavl landing page load
    $scope.init = function () {        
        $scope.cs.pageTitle = "My Approvals";
        $scope.requestStatus = [
            { id: 1, name: 'In Progress' },
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

        if ($scope.forceUpdate == null) {
            $scope.forceUpdate = 'false';
        }
        var depthInfo = {
            overview: 'true',
            genericInfo: 'true',
            approvers: 'true'
        }
        var filtersinfo =
            {
                backends: $scope.userBackendid,
                backendName: $scope.userBackendName,
                reqStatus: $scope.rqStatus.name,
                apprStatus: $scope.apStatus.name,
                isChanged: 'false',
                onlyChangedReq: 'false',
                CompletedRequestsSync: ShareData.CompletedRequestsSync
            }
        var params = {
            forceUpdate: $scope.forceUpdate,
            filters: filtersinfo,
            depth: depthInfo
        }
        
        var requestsync = {
            client: userDevices[0],
            parameters: params
        }
        $scope.requestsync = requestsync;        
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }
        $scope.config = config;
        $scope.getcount = function () {
            //2017-04-25
            //Post request variable to update user method
            $http.post("/Landing/GetBackendApprovalrequestcount", requestsync, config).success(function (data) {
                console.log(data);
                $scope.backends = [];
                if (ShareData.ShowwaitingMessage) {
                    $scope.cs.showNotifications("Please try after sometime. Your requests are being synched.", "");                    
                    ShareData.ShowwaitingMessage = false;
                }
                else {
                    $scope.tasksuccess = false;
                }               
                $scope.ShowMessage = false;
                $scope.lastsynch = new Date();
                window.setTimeout(function () {
                    $scope.$apply(function () {
                        $scope.showloader = false;
                    });
                }, 1000);
                $scope.showcontent = true;
                angular.forEach(data, function (BackendItems) {                    
                    if (ShareData.aprStatus == "Waiting") {                        
                        $scope.backends.push({ BackendID: BackendItems.BackendID, BackendName: BackendItems.BackendName, Count: BackendItems.WaitingCount, Pending: BackendItems.WaitingCount, Approved: BackendItems.ApprovedCount, Rejected: BackendItems.RejectedCount, Urgent: BackendItems.UrgentPendingCount });
                    }
                    else {                        
                        $scope.backends.push({ BackendID: BackendItems.BackendID, BackendName: BackendItems.BackendName, Count: BackendItems.ApprovedCount + BackendItems.RejectedCount, Pending: BackendItems.WaitingCount, Approved: BackendItems.ApprovedCount, Rejected: BackendItems.RejectedCount, Urgent: BackendItems.UrgentPendingCount });
                    }

                });
                $scope.pending = $scope.sum($scope.backends, 'Pending');
                $scope.urgent = $scope.sum($scope.backends, 'Urgent');
                var Approved = $scope.sum($scope.backends, 'Approved');
                var Rejected = $scope.sum($scope.backends, 'Rejected');
                $scope.completed = Approved + Rejected;
                $scope.drawRequestCountCirclesNew($scope.urgent + $scope.pending + $scope.completed, $scope.urgent, $scope.pending, $scope.completed, 'canvas', 'procentpending', '20', 70);
                //$scope.drawRequestCountCircles($scope.urgent + $scope.pending + $scope.completed, $scope.pending, 'canvaspending', 'procenturgent');
                ShareData.backendCount = $scope.backends;                               
            }).error(function (data, status) {
                console.log(data);
            });
        }
        var myCall = function () {
            if (ShareData.reload) {
                $scope.getcount();
            }
            else {
                ShareData.reload = true;
                $scope.stop();
            }
        };
        var promice;
        //// stops the interval
        $scope.stop = function () {
            $interval.cancel(promice);
        };
        $scope.start = function () {
            // stops any running interval to avoid two intervals running at the same time
            $scope.stop();
            //adhir
            //convert AutoRefreshValue from minutes to milliseconds
            var refreshMilliSeconds;
            if (ShareData.AutoRefreshValue != null) {
                refreshMilliSeconds = (ShareData.AutoRefreshValue * 60000);
            }
            else {
                refreshMilliSeconds = 600000;
            }
            promice = $interval(myCall, refreshMilliSeconds);
        };
        //to call method in every interval
        if (ShareData.reload) {
            $scope.start();
        }
        //to call method without delay at first time on page load
        myCall();
    };
    //$scope.SyncUpdate = function () {
    //    $scope.showloader = true;
    //    $scope.tasksuccess = false;
    //    $scope.ShowMessage = false;
    //    $scope.getcount();
    //};
    //$scope.forceUpdate = 'false';
    $scope.update = function () {
        $scope.showloader = true;
        $scope.requestsync.parameters.forceUpdate = 'true';        
        //Post request variable to update user method
        //2017-04-25
        $http.post("/Landing/ForceUpdate", $scope.requestsync, $scope.config).success(function (data) {
            console.log(data);
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.showloader = false;
                });
            }, 2000);
                      
            $scope.cs.showNotifications("We are trying to check if you have any requests. Please wait for some time !", "");                          
            $scope.start();            
            $scope.requestsync.parameters.forceUpdate = 'false';
        }).error(function (data, status) {
            console.log(data);
            $scope.requestsync.parameters.forceUpdate = 'false';
        });
    };
    
    //On click backend count
    $scope.redirectToDetailsPage = function (backendid) {
        ShareData.backendId = backendid;
        $scope.stop();
        $location.path('/approvalDetails');
    };
    $scope.sum = function (items, prop) {
        return items.reduce(function (a, b) {
            return a + b[prop];
        }, 0);
    };

    $scope.drawRequestCountCirclesPending = function (Total, startDegree, pending, canvasid, procentid) {
        var can = document.getElementById(canvasid),
            spanProcent = document.getElementById(procentid),
             c = can.getContext('2d');
        c.lineCap = 'round';
        var posX = can.width / 2,
            posY = can.height / 2;
        if (Total > 0 && pending > 0) {
            var fps = 1000 / 200,
            procent = 0,
            oneProcent = (360 / Total),
            result = oneProcent * pending;
            var deegres = 0;
            var acrIntervalPending = setInterval(function () {
                deegres += 1;
                procent = deegres / oneProcent;

                c.beginPath();
                c.strokeStyle = '#ffff31';
                c.lineWidth = '15';
                c.arc(posX, posY, 70, startDegree, startDegree + (Math.PI / 180) * (270 + deegres));
                c.stroke();
                if (deegres >= result || procent.toFixed() >= pending) {
                    clearInterval(acrIntervalPending);
                }
            }, fps);
        }
    }

    $scope.drawArc = function (canvasId, total, arcValue, arcColor, arcStartDegree, arcWidth) {
        //Canvas context
        var canvas = document.getElementById(canvasId);
        var c = canvas.getContext('2d');
        //c.lineCap = 'round';

        //center point
        var posX = canvas.width / 2;
        var posY = canvas.height / 2;

        var fps = 1000 / 200;
        var procent = 0;
        var oneProcent = (360 / total);
        var result = (oneProcent * arcValue) + arcStartDegree;

        var degree = arcStartDegree;
        c.beginPath();
        var arcDrawInterval = setInterval(function () {
            degree += 1;
            procent = degree / oneProcent;

            c.clearRect(0, 0, canvas.width, canvas.height);

            c.beginPath();
            c.strokeStyle = arcColor;
            c.lineWidth = arcWidth;
            c.arc(posX, posY, 70, (Math.PI / 180) * (270 + arcStartDegree), (Math.PI / 180) * (270 + degree));
            //c.fillStyle = arcColor;
            //c.fill();
            c.stroke();
            //if (degree >= result || procent.toFixed() >= arcValue) {
            if (degree >= result) {
                clearInterval(arcDrawInterval);
            }
        }, fps);
    };

    $scope.degreesToRadians = function(degrees) {
        return degrees * (Math.PI / 180);
    }

    $scope.radiansToDegrees = function (radians) {
        return radians * (180 / Math.PI);
    };


    $scope.drawArcs = function (canvasId, total, coordinates, arcWidth, radius) {
        //Canvas context
        var canvas = document.getElementById(canvasId);
        //RK
        if (canvas != null)
        {
            var c = canvas.getContext('2d');
            //c.lineCap = 'round';

            //center point
            var posX = canvas.width / 2;
            var posY = canvas.height / 2;

            var fps = 1000 / 200;
            //var procent = 0;
            //var oneProcent = (360 / total);
            //var resultUrgent = (oneProcent * coordinates.urgent.end);
            //var resultPending = (oneProcent * coordinates.pending.end);
            //var resultCompleted = (oneProcent * coordinates.completed.end);

            var degreeUrgent = coordinates.urgent.start;
            var degreePending = coordinates.pending.start;
            var degreeCompleted = coordinates.completed.start;

            var arcDrawInterval = setInterval(function () {
                if (degreeUrgent < coordinates.urgent.end) {
                    degreeUrgent += 1;
                }
                if (degreePending < coordinates.pending.end) {
                    degreePending += 1;
                }
                if (degreeCompleted < coordinates.completed.end) {
                    degreeCompleted += 1;
                }

                //procentUrgent = degreeUrgent / oneProcent;
                //procentPending = degreePending / oneProcent;
                //procentCompleted = degreeCompleted / oneProcent;

                c.beginPath();
                c.clearRect(0, 0, canvas.width, canvas.height);
                c.lineWidth = arcWidth;

                c.strokeStyle = coordinates.urgent.color;
                c.arc(posX, posY, radius, $scope.degreesToRadians(coordinates.urgent.start) - Math.PI / 2, $scope.degreesToRadians(degreeUrgent) - Math.PI / 2, false);
                //c.arc(posX, posY, 70, (Math.PI / 180) * (270 + coordinates.urgent.start), (Math.PI / 180) * (270 + degreeUrgent));
                c.stroke();

                c.beginPath();
                c.strokeStyle = coordinates.pending.color;
                c.arc(posX, posY, radius, $scope.degreesToRadians(coordinates.pending.start) - Math.PI / 2, $scope.degreesToRadians(degreePending) - Math.PI / 2, false);
                //c.arc(posX, posY, 70, (Math.PI / 180) * (270 + coordinates.pending.start), (Math.PI / 180) * (270 + degreePending));
                c.stroke();

                c.beginPath();
                c.strokeStyle = coordinates.completed.color;
                c.arc(posX, posY, radius, $scope.degreesToRadians(coordinates.completed.start) - Math.PI / 2, $scope.degreesToRadians(degreeCompleted) - Math.PI / 2, false);
                //c.arc(posX, posY, 70, (Math.PI / 180) * (270 + coordinates.completed.start), (Math.PI / 180) * (270 + degreeCompleted));
                c.stroke();

                if (degreeUrgent >= coordinates.urgent.end && degreePending >= coordinates.pending.end && degreeCompleted >= coordinates.completed.end) {
                    clearInterval(arcDrawInterval);
                }
            }, fps);
        }
        
    };

    $scope.drawRequestCountCirclesPostRender = function (total, urgent, pending, completed, canvasId, circleMessageId) {
        $timeout(function () {
            $scope.drawRequestCountCirclesNew(total, urgent, pending, completed, canvasId, circleMessageId, "12", 30);
        });
    };

    $scope.drawRequestCountCirclesNew = function (total, urgent, pending, completed, canvasId, circleMessageId, arcWidth, radius) {
        try {
            document.getElementById(circleMessageId).innerHTML = "<span class='spanTask'>Requests</span><span class='spanTaskCount'>" + total + "</span>";
        }
        catch (err) { }
        if (total > 0) {
            var urgent = {
                start: 0,
                end: (360 / total) * urgent,
                color: "#d8abbc"
            };
            var pending = {
                start: urgent.end,
                end: ((360 / total) * pending) + urgent.end,
                color: "#b35b7c"
            };
            var completed = {
                start: pending.end,
                end: ((360 / total) * completed) + pending.end,
                color: "#331d33"
            };
            var coordinates = {
                urgent: urgent,
                pending: pending,
                completed: completed
            };

            $scope.drawArcs(canvasId, total, coordinates, arcWidth, radius);

            /*var urgentColorCode = "#f0ab00";
            var pendingColorCode = "#fd83aa";
            var completedColorCode = "#67f0ac";
            if (urgent > 0 && pending > 0 && completed > 0) {
                $scope.drawArc(canvasUrgentId, total, urgent, urgentColorCode, 0, arcWidth);
                var urgentEndDegree = (360 / total) * urgent;

                //var startDegree = (Math.PI / 180) * (270 + urgentEndDegree);
                $scope.drawArc(canvasPendingId, total, pending, pendingColorCode, urgentEndDegree, arcWidth);
                var pendingEndDegree = ((360 / total) * pending) + urgentEndDegree;

                //startDegree = (Math.PI / 180) * (270 + pendingEndDegree);
                $scope.drawArc(canvasCompletedId, total, completed, completedColorCode, pendingEndDegree, arcWidth);
            }
            //when any of the value is 0
            else {
                //when urgent is 0
                if (urgent === 0) {
                    //when pending is 0 which means completed is > 0
                    if (pending === 0) {
                        //draw completed arc
                        $scope.drawArc(canvasCompletedId, total, completed, completedColorCode, 0, arcWidth);
                    }
                    //when pending is > 0
                    else {
                        //draw pending arc
                        $scope.drawArc(canvasPendingId, total, pending, pendingColorCode, 0, arcWidth);
                        var pendingEndDegree = (360 / total) * pending;

                        //when completed is > 0
                        if (completed > 0) {
                            $scope.drawArc(canvasCompletedId, total, completed, completedColorCode, pendingEndDegree, arcWidth);
                        }
                    }
                }
                //when urgent is > 0
                else {
                    //draw urgent arc
                    $scope.drawArc(canvasUrgentId, total, urgent, urgentColorCode, 0, arcWidth);
                    var urgentEndDegree = (360 / total) * urgent;

                    //when pending is 0 which means completed is > 0
                    if (pending === 0) {
                        //draw completed arc
                        if (completed > 0) {
                            $scope.drawArc(canvasCompletedId, total, completed, completedColorCode, urgentEndDegree, arcWidth);
                        }
                    }
                        //when pending is > 0
                    else {
                        //draw pending arc
                        $scope.drawArc(canvasPendingId, total, pending, pendingColorCode, urgentEndDegree, arcWidth);
                        var pendingEndDegree = (360 / total) * pending;
                    }
                }
            }*/
        }
        else {
            document.getElementById(circleMessageId).innerHTML = "<span class='spanTask'>Requests</span><span class='spantaskcount'>0</span>";
            $scope.drawArc(canvasUrgentId, total, total, "#b1b1b1", 0, arcWidth);
        }
    };

    $scope.drawRequestCountCircles = function (Total, Current, canvasid, procentid) {
        var can = document.getElementById(canvasid),
            spanProcent = document.getElementById(procentid),
             c = can.getContext('2d');
        c.lineCap = 'round';
        var posX = can.width / 2,
            posY = can.height / 2;
        if (Total > 0 && Current > 0) {
            var fps = 1000 / 200,
            procent = 0,
            oneProcent = (360 / Total),
            result = oneProcent * Current;
            var deegres = 0;
            var acrInterval = setInterval(function () {
                deegres += 1;
                c.clearRect(0, 0, can.width, can.height);
                procent = deegres / oneProcent;

                spanProcent.innerHTML = "<span class='spanTask'>Requests</span><span class='spantaskcount'>" + Total + "</span>";

                c.beginPath();
                c.arc(posX, posY, 70, (Math.PI / 180) * 270, (Math.PI / 180) * (270 + 360));
                c.strokeStyle = '#301831';
                c.lineWidth = '15';
                c.stroke();
                c.beginPath();
                if (canvasid == 'canvaspending') {
                    c.strokeStyle = '#b85c7d';
                }
                if (canvasid == 'canvascompleted') {
                    c.strokeStyle = '#009933';
                }
                c.lineWidth = '15';
                c.arc(posX, posY, 70, (Math.PI / 180) * 270, (Math.PI / 180) * (270 + deegres));
                c.stroke();
                if (deegres >= result || procent.toFixed() >= Current) clearInterval(acrInterval);
            }, fps);
        }
        else {
            c.clearRect(0, 0, can.width, can.height);
            c.beginPath();
            c.arc(posX, posY, 70, (Math.PI / 180) * 270, (Math.PI / 180) * (270 + 360));
            c.strokeStyle = '#b1b1b1';
            c.lineWidth = '10';
            c.stroke();
            spanProcent.innerHTML = "<span class='spanTask'>Requests</span><span class='spantaskcount'>0</span>";
        }
    }
    
    $scope.drawbackendRequestCountCircles = function (Total, Current, canvasid) {       
        var id = 'canvaspending' + canvasid;
        $timeout(function () {
            var can = document.getElementById(id),
                 c = can.getContext('2d');
            c.lineCap = 'round';
            var posX = can.width / 2,
                posY = can.height / 2;
            if (Total > 0 && Current > 0) {
                var fps = 1000 / 200,
                procent = 0,
                oneProcent = (360 / Total),
                result = oneProcent * Current;
                var deegres = 0;
                var acrInterval = setInterval(function () {
                    deegres += 1;
                    c.clearRect(0, 0, can.width, can.height);
                    procent = deegres / oneProcent;

                    c.beginPath();
                    c.arc(posX, posY, 70, (Math.PI / 180) * 270, (Math.PI / 180) * (270 + 360));
                    c.strokeStyle = '#c3c3c3';
                    c.lineWidth = '12';
                    c.stroke();
                    c.beginPath();
                    c.strokeStyle = '#703a6f';
                    c.lineWidth = '12';
                    c.arc(posX, posY, 70, (Math.PI / 180) * 270, (Math.PI / 180) * (270 + deegres));
                    c.stroke();
                    if (deegres >= result) clearInterval(acrInterval);
                }, fps);
            }
            else {
                c.clearRect(0, 0, can.width, can.height);
                c.beginPath();
                c.arc(posX, posY, 70, (Math.PI / 180) * 270, (Math.PI / 180) * (270 + 360));
                c.strokeStyle = '#c3c3c3';
                c.lineWidth = '12';
                c.stroke();
            }
        })
                       
    }
});
//AngularJS controller to get pending approval details
app.controller('ApprovalDetailsController', function ($scope, $http, $location, $filter, ShareData, CommonService) {
    $scope.cs = CommonService;
    $scope.emptyRecord = false;
    $scope.comment = {
        text: "",
        commentActive: false,
        status: "",
        requestId: "",
        taskId: "",
        errorClass: ""
    };

    $scope.init = function () {
        $scope.cs.pageTitle = "Requests List";
        $scope.loading = true;
        $scope.showpending = true;
        $scope.hideButton = true;
        var backendid = ShareData.backendId;
        var userDevices = ShareData.userDevices;
        var approvalStatus = ShareData.aprStatus;
        var requestStatus = ShareData.reqStatus;
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
                onlyChangedReq: 'false',
                CompletedRequestsSync: ShareData.CompletedRequestsSync
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
        $scope.requestsync = requestsync;
        $scope.config = config;
        if (ShareData.aprStatus == "Completed") {
            $scope.showcompletedtasks();
            $scope.showpending = false;
            $scope.showaction = false;
        }
        else {
            ShareData.aprStatus = "Waiting";
            //Post request variable to GetApprovalDetails method
            $http.post("/Landing/GetApprovalDetails", requestsync, config).success(function (data) {
                if (approvalStatus == "Completed") {
                    $scope.hideButton = false;
                }
                $scope.hideButton = true;
                if (data.length == 0) {
                    $scope.ShowwaitingMessage = true;
                }
                ShareData.pendingtasks = {};
                angular.forEach(data, function (approvalItems) {
                    //if (approvalItems.approval.Status == "Waiting") {
                        $scope.approvalTasks.push(approvalItems.approval);
                   // }
                });
                if ($scope.approvalTasks.length === 0) {
                    $scope.emptyRecord = true;
                }
                else {
                    $scope.emptyRecord = false;
                }
                ShareData.pendingtasks = data;

                window.setTimeout(function () {
                    $scope.$apply(function () {
                        $scope.loading = false;
                    });
                }, 1500);
            }).error(function (data, status) {
                $scope.emptyRecord = true;
                window.setTimeout(function () {
                    $scope.$apply(function () {
                        $scope.loading = false;
                    });
                }, 1500);
            });
        }
               
    };
    $scope.sorttasks = function () {
        if ($scope.showpending) {
            return 'RequestId';
        }
        else {
            return 'DecisionDate';
        }
    }
    $scope.showpendingtasks = function () {
        $scope.loading = true;
        $scope.requestsync.parameters.filters.apprStatus = "Waiting";
        $http.post("/Landing/GetApprovalDetails", $scope.requestsync, $scope.config).success(function (data) {            
            ShareData.pendingtasks = data;
            $scope.approvalTasks = [];
            ShareData.aprStatus = "Waiting";
            angular.forEach(ShareData.pendingtasks, function (approvalItems) {
                //if (approvalItems.approval.Status == "Waiting") {
                    $scope.approvalTasks.push(approvalItems.approval);
                //}
            });
            if ($scope.approvalTasks.length === 0) {
                $scope.emptyRecord = true;
            }
            else {
                $scope.emptyRecord = false;
            }
            $scope.showpending = true;
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.loading = false;
                });
            }, 1500);
        }).error(function (data, status) {
            $scope.emptyRecord = true;
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.loading = false;
                });
            }, 1500);
        });
        
    }
    $scope.showcompletedtasks = function () {
        $scope.loading = true;
        $scope.requestsync.parameters.filters.apprStatus = "Completed";
        $http.post("/Landing/GetApprovalDetails", $scope.requestsync, $scope.config).success(function (data) {            
            ShareData.pendingtasks = data;
            $scope.approvalTasks = [];
            ShareData.aprStatus = "Completed";
            angular.forEach(ShareData.pendingtasks, function (approvalItems) {
                //if (approvalItems.approval.Status != "Waiting") {
                    $scope.approvalTasks.push(approvalItems.approval);
                //}
            });
            if ($scope.approvalTasks.length === 0) {
                $scope.emptyRecord = true;
            }
            else {
                $scope.emptyRecord = false;
            }
            $scope.showpending = false;
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.loading = false;
                });
            }, 1500);
        }).error(function (data, status) {
            $scope.emptyRecord = true;
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.loading = false;
                });
            }, 1500);
        });
        

    }
    // Show pendind task request information
    $scope.showDetails = function (requestId, taskid) {
        ShareData.detailTaskinfo = requestId;
        ShareData.detailTaskId = taskid;
        $location.path('/detailTaskInfo');
    };

    $scope.requestStatus = function (status, requestId, taskId) {
        $scope.hideButton = true;
        var appComment = "";
        if (typeof $scope.comment === 'undefined' || $scope.comment.length === 0) {
            appComment = "";
        }
        else {
            appComment = $scope.comment.text;
        }
        $scope.decisionDate = $filter('date')(new Date(), 'yyyy-MM-dd HH:mm');
        var userDevices = ShareData.userDevices;
        $scope.loading = true;
        var approvalDecision = {
            DecisionDate: $scope.decisionDate,
            Comment: appComment,
            Status: status
        }
        var approvalDetails = {
            ApprovalRequestID: taskId,
            ApprovalDecision: approvalDecision,
            DeviceID: ShareData.userDevices[0].device.DeviceID
        }
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }

        //Post request variable to SendApprovalstatus method
        $http.post("/Landing/SendApprovalstatus", approvalDetails, config).success(function (data) {
            $scope.closeComment();
            for (var i = 0; i < $scope.approvalTasks.length; i++) {
                if ($scope.approvalTasks[i].ServiceLayerTaskID === taskId) {
                    $scope.approvalTasks.splice(i, 1);
                    //reduce total items in pagination
                    $scope.totalItems = $scope.totalItems - 1;
                    break;
                }
            }
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.loading = false;
                });
            }, 1500);
            $scope.taskrequestid = requestId;
            $scope.taskrequeststatus = status;            
            var notificationText = 'Request ' + requestId + ' ' + status + ' successfully!';
            $scope.cs.showNotifications(notificationText, "");
        }).error(function (data, status) {
            console.log(data);
            $scope.hideButton = true;
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.loading = false;
                });
            }, 1500);
        });
    };
    $scope.redirect = function () {
        $location.path('/approvalLanding');
    };
    //Remove row in Pending Approvals

    $scope.openComment = function (status, requestId, taskId) {
        $scope.comment = {
            text: "",
            commentActive: true,
            status: status,
            requestId: requestId,
            taskId: taskId,
            errorClass: ""
        };
    };

    $scope.closeComment = function () {
        $scope.comment = {
            text: "",
            commentActive: false,
            status: "",
            requestId: "",
            taskId: "",
            errorClass: ""
        };
    };

    $scope.confirmComment = function () {
        if ($scope.comment.text.length === 0) {
            $scope.comment.errorClass = "error";
            return;
        }
        else {
            $scope.requestStatus($scope.comment.status, $scope.comment.requestId, $scope.comment.taskId);
        }
    };
});
//AngularJS controller to get details request information 
app.controller('ApprovalDetailstaskController', function ($scope, $http, $location, $filter, $window, ShareData, CommonService) {
    $scope.cs = CommonService;
    $scope.comment = {
        text: "",
        commentActive: false,
        status: "",
        requestId: "",
        taskId: "",
        errorClass: ""
    };

    $scope.init = function () {
        $scope.cs.pageTitle = "Request Details";        
        $scope.showloader = true;
        var requestId = ShareData.detailTaskinfo;
        var taskId = ShareData.detailTaskId;
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
                onlyChangedReq: 'false',
                CompletedRequestsSync: ShareData.CompletedRequestsSync
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
        $scope.requestDetails = requestDetails;
        $scope.TaskID = taskId;
        $scope.config = config;        
        $scope.fields = [];
        $scope.approvers = [];
        //Post request variable to GetRequestDetails method
        $http.post("/Landing/GetRequestDetails", requestDetails, config).success(function (data) {            
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.showloader = false;
                });
            }, 1500);
            angular.forEach(data, function (approvalItems) {
                if (approvalItems.request != null) {
                    $scope.id = approvalItems.request.ID;
                    $scope.title = approvalItems.request.Title;
                    $scope.status = approvalItems.request.Status;
                    $scope.created = approvalItems.request.Created;
                    $scope.userID = approvalItems.request.Requester.UserID;
                    $scope.name = approvalItems.request.Requester.Name;
                    angular.forEach(approvalItems.request.Fields, function (overviewFields) {
                        $scope.fields.push(overviewFields);
                    });
                    angular.forEach(approvalItems.request.Approvers, function (approverList) {
                        if (approverList.Comment.indexOf(ADIDAS.GM.CONSTANTS.CONFIG("MobileIconKeyword")) > -1) {
                            approverList.iconClass = ADIDAS.GM.CONSTANTS.CONFIG("MobileIconClass");
                            approverList.Comment = approverList.Comment.replace(ADIDAS.GM.CONSTANTS.CONFIG("MobileIconKeyword"), "");
                        }
                        if (approverList.Comment.indexOf(ADIDAS.GM.CONSTANTS.CONFIG("BackendIconKeyword")) > -1) {
                            approverList.iconClass = ADIDAS.GM.CONSTANTS.CONFIG("BackendIconClass");
                            approverList.Comment = approverList.Comment.replace(ADIDAS.GM.CONSTANTS.CONFIG("BackendIconKeyword"), "");
                        }
                        $scope.approvers.push(approverList);
                    });
                }
                else {
                    $scope.showretry = true;
                }

            });
            if (approvalStatus == "Completed") {
                $scope.hidePendingButton = false;
                $scope.hideCompletedButton = true;
                $scope.TaskButtonText = "Completed";
            }
            else {
                $scope.hidePendingButton = true;
                $scope.hideCompletedButton = false;
            }
        }).error(function (data, status) {
            console.log(data);
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.showloader = false;
                });
            }, 1500);

        });
    };

    $scope.openpdf = function () {
        //Post request variable to GetRequestDetails method   
        var newwindow = $window.open();
        $http.post("/Landing/GetRequestPDF", $scope.requestDetails)
        .success(function (data) {            
            if (data == null || data == "") {
                newwindow.close();                
                $scope.cs.showNotifications("PDF is not available for this Request.", "");
            }
            else {
                newwindow.location = data;
                if ($window.navigator.appVersion.indexOf("Mac") == -1) {
                    window.setTimeout(function () {
                        $scope.$apply(function () {
                            newwindow.close();
                        });
                    }, 2500);
                }
            }
        }).error(function (data, status) {
            console.log(data);
        })
    };

    $scope.openComment = function (status, requestId, taskId) {
        $scope.comment = {
            text: "",
            commentActive: true,
            status: status,
            requestId: requestId,
            taskId: taskId,
            errorClass: ""
        };
    };

    $scope.closeComment = function () {
        $scope.comment = {
            text: "",
            commentActive: false,
            status: "",
            requestId: "",
            taskId: "",
            errorClass: ""
        };
    };

    $scope.confirmComment = function () {
        if ($scope.comment.text.length === 0) {
            $scope.comment.errorClass = "error";
            return;
        }
        else {
            $scope.requestStatus($scope.comment.status, $scope.comment.requestId, $scope.comment.taskId);
        }
    };

    $scope.requestStatus = function (status, requestId, taskid) {
       //No idea where the comment will go. Comment is saved in $scope.comment.text property. To set the $scope.comment.text to empty string once the record is saved
        $scope.showloader = true;
        var appComment = "";
        if (typeof $scope.comment === 'undefined' || $scope.comment.length === 0) {
            appComment = "";
        }
        else {
            appComment = $scope.comment.text;
        }
        var userDevices = ShareData.userDevices;
        $scope.decisionDate = $filter('date')(new Date(), 'yyyy-MM-dd HH:mm');
        var approvalDecision = {
            DecisionDate: $scope.decisionDate,
            Comment: appComment,
            Status: status
        }
        var approvalDetails = {
            ApprovalRequestID: taskid,
            ApprovalDecision: approvalDecision,
            DeviceID: ShareData.userDevices[0].device.DeviceID
        }
        var config = {
            headers: {
                'Content-Type': 'application/json'
            }
        }       
        $http.post("/Landing/SendApprovalstatus", approvalDetails, config).success(function (data) {
            $scope.closeComment();
            var notificationtext='Request '+ status+ ' successfully!';
            $scope.cs.showNotifications(notificationtext, "");
            $scope.hideButton = false;
            $scope.hidePendingButton = false;
            $scope.hideCompletedButton = true;
            $scope.TaskButtonText = "Pending Tasks";
            window.setTimeout(function () {
                $scope.$apply(function () {
                    $scope.showloader = false;
                });
            }, 1500);
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