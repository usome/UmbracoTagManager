angular.module("umbraco").controller("TagManager.TagManagerEditController",
    function ($scope, $routeParams, $location, TagManagerResource, notificationsService, navigationService, $route, treeService, appState, $timeout) {

        TagManagerResource.getById($routeParams.id).then(function (response) {
            $scope.cmsTags = response.data;
            $scope.selectedTag = $routeParams.id;
        });

        $scope.save = function (cmsTags) {
            TagManagerResource.save(cmsTags).then(function (response) {
                $scope.cmsTags = response.data;
                var pathArray = ['-1', 'tagGroup-' + cmsTags.group];
                notificationsService.success("Success", "'" + cmsTags.tag + "' has been saved");

                // Use $timeout to ensure the reload occurs after the scope has been updated
                $timeout(function () {
                    // Reload the route or update the UI as needed
                    $route.reload();
                });

                // Reload the entire tree
                navigationService.syncTree({ tree: 'TagManagerTree', path: ['-1'], forceReload: true, activate: false }).then(
                    function () {
                        // Add any specific logic after syncing the tree if needed
                    }
                );
            });
        };

        $scope.deleteTag = function (cmsTags) {
            // stop from firing again on double-click
            if ($scope.busy) {
                return false;
            }
            // mark it for deletion (used in the UI)
            $scope.busy = true;

            TagManagerResource.deleteTag(cmsTags).then(function (response) {
                $scope.cmsTags = response.data;

                notificationsService.success("Success", "'" + cmsTags.tag + "' has been deleted.");

                // Get the treeNode that we have selected
                var node = appState.getTreeState("selectedNode");

                // Remove the node
                treeService.removeNode(node);

                if ($location.path() == "/TagManager/TagManagerTree/edit/" + cmsTags.id) {
                    $location.path("/TagManager/TagManagerTree/overview/default");
                }

                // Return to not busy
                $scope.busy = false;

                $timeout(function () {
                    $route.reload();

                    var pathArray = ['-1', 'tagGroup-' + (cmsTags.group || 'default')];
                    navigationService.syncTree({ tree: 'TagManagerTree', path: pathArray, forceReload: true, activate: false }).then(
                        function () {
                        }
                    );
                });
            }, function (err) {
                // Handle error
                $scope.busy = false;
                // Check if response is ysod
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






//display block

angular.module("umbraco").controller("TagManager.TagDisplayController", function ($scope, TagManagerResource, $routeParams, $location, $route, $timeout, notificationsService) {
    // Initialize an array to hold selected tags
    $scope.selectedTags = [];

    // Fetch tags for a specific group from the server
    TagManagerResource.getAllTagsInGroup($routeParams.id).then(function (response) {
        $scope.cmsTags = response.data;
        $scope.selectedTag = $routeParams.id;

        // Initialize an array to hold the tags
        $scope.tags = $scope.cmsTags.map(function (tag) {
            // Use 'tag.Id' as the property representing the numeric ID
            var tagId = tag.id;

            // Ensure tag and noTaggedNodes properties are not null or undefined
            var tagName = tag.tag || 'Unknown Tag';
            var tagCount = tag.noTaggedNodes !== null && tag.noTaggedNodes !== undefined
                ? tag.noTaggedNodes.toString()
                : '0';

            return {
                id: tagId,
                name: `${tagName} (${tagCount})`,
                selected: false, // Add a selected property
            };
        });
    });

    // Function to toggle the selection of a tag
    $scope.toggleSelectTag = function (tag) {
        tag.selected = !tag.selected;

        if (tag.selected) {
            // Add to selected tags array
            $scope.selectedTags.push(tag.id);
        } else {
            // Remove from selected tags array
            var index = $scope.selectedTags.indexOf(tag.id);
            if (index !== -1) {
                $scope.selectedTags.splice(index, 1);
            }
        }
    };

    // Function to filter tags based on the search query
    $scope.filterTags = function (tag) {
        return !$scope.searchQuery || tag.name.toLowerCase().includes($scope.searchQuery.toLowerCase());
    };

    // Function to navigate to the create tag view
    $scope.navigateToCreateTag = function () {
        $location.path('/TagManager/TagManagerTree/create');
    };

    // Function to navigate to the edit tag view
    $scope.navigateToEditTag = function (tagId) {
        $location.path('/TagManager/TagManagerTree/edit/' + tagId);
    };


   // Function to delete selected tags
$scope.deleteSelectedTags = function () {
    if ($scope.selectedTags.length === 0) {
        return;
    }

    var deletedTagsCount = $scope.selectedTags.length;

    var deletePromises = $scope.selectedTags.map(function (tagId) {
        return TagManagerResource.deleteTag({ id: tagId }).catch(function (error) {
            console.error("Error deleting tag with ID " + tagId + ": ", error);
            throw error; 
        });
    });

    Promise.all(deletePromises)
        .then(function () {
            $scope.selectedTags = [];
            $route.reload(); 

            var message = "Deleted " + deletedTagsCount + " tag" + (deletedTagsCount > 1 ? "s" : "");
            notificationsService.success("Success", message);
        })
        .catch(function (error) {
            console.error("Error during tag deletion: ", error);
        });
};



    // Function to clear the selection of all tags
    $scope.clearSelection = function () {
        // Iterate through all tags and set selected to false
        $scope.tags.forEach(function (tag) {
            tag.selected = false;
        });

        // Clear the selectedTags array
        $scope.selectedTags = [];
    };
    // Add a flag to control the visibility of the confirmation dialog
    $scope.showConfirmationDialog = false;

    // Function to show the confirmation dialog
    $scope.showConfirmation = function () {
        $scope.showConfirmationDialog = true;
    };

    // Function to hide the confirmation dialog
    $scope.hideConfirmation = function () {
        $scope.showConfirmationDialog = false;
    };

    // Function to confirm the delete operation
    $scope.confirmDelete = function () {
        // Call the deleteSelectedTags function
        $scope.deleteSelectedTags();

        // Hide the confirmation dialog
        $scope.hideConfirmation();
    };

    // Function to cancel the delete operation
    $scope.cancelDelete = function () {
        // Hide the confirmation dialog
        $scope.hideConfirmation();
    };

});


angular.module("umbraco").controller("TagManager.TagManagerCreateController",
    function ($scope, TagManagerResource, notificationsService, navigationService, $location, $route, $routeParams, $timeout, $window) {

        $scope.cmsTags = {
            tag: "",
            group: ""
        };

        $scope.save = function (cmsTags) {
            if (!cmsTags.tag) {
                notificationsService.error("Error", "Tag name is required.");
                return;
            }

            TagManagerResource.createTag(cmsTags).then(function (response) {
                notificationsService.success("Success", "'" + cmsTags.tag + "' has been Created");

                $location.path('/TagManager/TagManagerTree/overview/default');

                $route.reload();
            }, function (err) {
                notificationsService.error("Error", "Failed to create a new tag. Please try again.");
            });
        };
    });


