angular.module("umbraco").controller("TagManager.TagManagerEditController",
	function ($scope, $routeParams, $location, TagManagerResource, notificationsService, navigationService, $route, treeService, appState) {

	    TagManagerResource.getById($routeParams.id).then(function (response) {
	        $scope.cmsTags = response.data;
	        $scope.selectedTag = $routeParams.id;
	    });

	    $scope.save = function (cmsTags) {
	        TagManagerResource.save(cmsTags).then(function (response) {
	            $scope.cmsTags = response.data;
	            var pathArray = ['-1', 'tagGroup-' + cmsTags.group];
	            notificationsService.success("Success", "'" + cmsTags.tag + "' has been saved");
	            pathArray.push(cmsTags.id);
	            navigationService.syncTree({ tree: 'TagManagerTree', path: pathArray, forceReload: true, activate: false }).then(
                    function (syncArgs) {
                        if ($routeParams.method == "edit") {
                            $location.path(syncArgs.node.routePath);
                            $route.reload();
                        }
                    });
	        });
	    };

		$scope.deleteTag = function (cmsTags) {
                // stop from firing again on double-click
                if ($scope.busy) {
                    return false;
                }
                //mark it for deletion (used in the UI)
                $scope.busy = true;

                TagManagerResource.deleteTag(cmsTags).then(function (response) {
                    // TODO: trap failure?
                    $scope.cmsTags = response.data;

                    notificationsService.success("Success", "'" + cmsTags.tag + "' has been deleted.");

                    //get the treeNode that we have selected
                    // TODO: check this works for menu based deletion
                    var node = appState.getTreeState("selectedNode");

                    // parent of the node - for redirect
                    var parent = node.parent();

                    //remove the node
                    treeService.removeNode(node);

                    // if the current edited item is the same one as we're deleting, we need to navigate elsewhere
                    // for if we want to use the menu to also delete as doesn't neccesarily require redirection                  
                    if ($location.path() == "/" + node.routePath) {
                        //set location to be parent
                        $location.path("/"+node.parent().routePath);
                    }

                    // return to not busy
                    $scope.busy = false;

                    // TODO: close the confirm delete dialogue
                    //navigationService.hideMenu();
                }, function (err) {

                    $scope.busy = false;
                    //check if response is ysod
                    if (err.status && err.status >= 500) {
                        dialogService.ysodDialog(err);
                    }
                    if (err.data && angular.isArray(err.data.notifications)) {
                        for (var i = 0; i < err.data.notifications.length; i++) {
                            notificationsService.showNotification(err.data.notifications[i]);
                        }
                    }
                });	        
	    };
    });