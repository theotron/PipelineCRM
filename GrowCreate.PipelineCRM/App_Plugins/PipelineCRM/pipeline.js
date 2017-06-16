/*  -------------------------
    Pipelines
    ------------------------- */

angular.module("umbraco").controller("Pipeline.Pipeline.Browse",
	function ($scope, $routeParams, pipelineResource, organisationResource, notificationsService, navigationService, dialogService, entityResource, mediaResource, iconHelper, $filter, localizationService) {

	    // load UI
	    $scope.content = {
	        statusId: $routeParams.id,
	        isArchive: $routeParams.id == -1,
	        isUnassigned: $routeParams.id == -2,
            title: $routeParams.id == -2 ?
                localizationService.localize('pipeline_unassigned') : ($routeParams.id == -1 ? localizationService.localize('recycleBin') : localizationService.localize('pipeline_showAll'))
	    };

	    if ($routeParams.id > 0) {
	        pipelineResource.getStatusById($routeParams.id).then(function (response) {
	            $scope.content.title = response.data.Name;
	        });
	    }

	});

angular.module("umbraco").controller("Pipeline.PipelineEditController",
	function ($scope, $routeParams, userService, contactResource, taskResource, pipelineResource, organisationResource, propResource, notificationsService, navigationService, dialogService, entityResource, mediaResource, iconHelper, $filter, localizationService) {

	    // load UI
	    $scope.loaded = false;
	    $scope.Statuses = [];

	    pipelineResource.getUsers().then(function (response) {
	        $scope.Users = response.data;
	    });
	    pipelineResource.getStatuses().then(function (response) {
	        $scope.Statuses = response.data;
	    });
	    organisationResource.getAll().then(function (response) {
	        $scope.Organisations = response.data;
	    });
	    contactResource.getAll().then(function (response) {
	        $scope.Contacts = response.data;
	    });
	    userService.getCurrentUser().then(function (response) {
	        $scope.currentUserId = response.id;
	    });

	    $scope.content = {
	        tabs: [
	            { id: 1, label: localizationService.localize('pipeline_details') },
                { id: 2, label: localizationService.localize('pipeline_timeline') }
	        ]
	    };

	    // date pickers
	    $scope.datePickerConfig = {
	        view: 'datepicker',
	        config: {
	            pickDate: true,
	            pickTime: false,
	            pick12HourFormat: false,
	            useSeconds: false,
	            format: "YYYY-MM-DD"
	        }
	    };

	    $scope.createdDatepicker = angular.extend({}, $scope.datePickerConfig, { value: moment() });
	    $scope.completeDatepicker = angular.extend({}, $scope.datePickerConfig, { value: '' });

	    // get doc type definition
	    $scope.getCustomProps = function () {

	        $scope.pipeline.UserId = $scope.currentUserId;

	        propResource.getPropsByType('pipeline').then(function (response) {

	            $scope.customprops = response.data;
	            $scope.customprops.reverse().forEach(function (tab) {

	                // tabs
	                tab.id = $scope.content.tabs.length + 1;
	                $scope.content.tabs.splice(1, 0, { id: $scope.content.tabs.length + 1, label: tab.name });

	                var customProps = angular.fromJson($scope.pipeline.CustomProps);

	                // match values
	                if ($scope.pipeline.CustomProps) {
	                    tab.items.forEach(function (prop) {
	                        var match = _.findWhere(customProps, { id: prop.id });
	                        if (match) {
	                            prop.value = match.value;
	                        }
	                    });
	                }
	            });

	            $('.tab-content>.tab-pane:first').addClass('active'); //force first tab to show
	            $scope.loaded = true;

	        });
	    };

	    // create or edit pipeline
	    if ($routeParams.id == -1) {
	        $scope.pipeline = {};
	        $scope.pipeline.Tasks = [];
	        $scope.pipeline.StatusId = $routeParams.statusId || 0;
	        $scope.getCustomProps();
	    }
	    else {

	        pipelineResource.getPipelineById($routeParams.id).then(function (response) {

	            $scope.pipeline = response.data;
	            $scope.$watch('Statuses', function () {
	                $scope.Statuses.forEach(function (obj, i) {
	                    if ($scope.pipeline.StatusId && obj.Id == $scope.pipeline.StatusId) {
	                        $scope.pipeline.Status = obj;
	                    };
	                });
	            });
	            
	            if ($scope.pipeline.DateCreated.IsDate()) {
	                $scope.createdDatepicker = angular.extend({}, $scope.datePickerConfig, { value: moment.utc(new Date($scope.pipeline.DateCreated)) });
	            }

	            if ($scope.pipeline.DateComplete.IsDate()) {
	                $scope.completeDatepicker = angular.extend({}, $scope.datePickerConfig, { value: moment.utc(new Date($scope.pipeline.DateComplete)) });
	            }
	            
	            $scope.getCustomProps();
	        });
	    }

	    // save pipeline
	    $scope.save = function () {

	        //unload date pickers
	        $scope.pipeline.DateCreated = new Date($scope.createdDatepicker.value ? moment.utc(new Date($scope.createdDatepicker.value)) : '');
	        $scope.pipeline.DateComplete = new Date($scope.completeDatepicker.value ? moment.utc(new Date($scope.completeDatepicker.value)) : '');

	        // stringify custom props
	        if ($scope.customprops) {
	            var customProps = [];
	            $scope.customprops.forEach(function (tab) {
	                tab.items.forEach(function (prop) {
	                    if (prop.value) {
	                        customProps.push({
	                            id: prop.id,
	                            value: prop.value,
	                            alias: prop.alias
	                        });
	                    }
	                });
	            });
	            $scope.pipeline.CustomProps = angular.toJson(customProps);
	        }

	        pipelineResource.savePipeline($scope.pipeline).then(function (response) {
	            $scope.pipeline = response.data;
	            navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
	            notificationsService.success("OK", $scope.pipeline.Name + " has been saved");
	            $scope.pipelineForm.$dirty = false;

	            // if creating a new one, got to its Url
	            if ($routeParams.id == '-1') {
	                location.href = '#/pipelineCrm/pipelineCrmTree/edit/' + response.data.Id;
	            }
	        });
	    }

	    // archive / restore pipeline
	    $scope.archivePipeline = function () {
	        pipelineResource.archivePipeline($scope.pipeline).then(function (response) {
	            $scope.pipeline.Archived = true;
	            navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
	            notificationsService.success("OK", $scope.pipeline.Name + " has been archived");
	        });
	    };

	    $scope.restorePipeline = function () {
	        pipelineResource.restorePipeline($scope.pipeline).then(function (response) {
	            $scope.pipeline.Archived = false;
	            navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
	            notificationsService.success("OK", $scope.pipeline.Name + " has been restored");
	        });
	    };

	    $scope.duplicatePipeline = function () {
	        pipelineResource.duplicatePipeline($scope.pipeline).then(function (response) {
	            navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
	            notificationsService.success("OK", $scope.pipeline.Name + " has been duplicated");
	            location.href = '#/pipelineCrm/pipelineCrmTree/edit/' + response.data.Id;
	        });
	    };

	    $scope.deletePipeline = function () {

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/delete.confirm.html',
	            dialogData: { Type: 'opportunity' },
	            callback: function (data) {

	                pipelineResource.deleteById($scope.pipeline.Id, data.deleteLinks).then(function (pipeline) {
	                    notificationsService.success("OK", "The opportunity has been deleted.");
	                    location.href = '#/pipelineCrm/pipelineCrmTree/browse/-1';
	                });
	            }
	        });
	    };

	    // add new contacts
	    $scope.addContact = function () {

	        // we need to have an id before we can add dependencies
	        if ($routeParams.id == "-1") {
	            notificationsService.error("Oops...", "You must save the Opportunity before you add a new Contact.");
	            return false;
	        }

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/contact.add.html',
	            dialogData: {
	                OrganisationId: $scope.pipeline.OrganisationId
	            },
	            callback: function (contact) {

	                // we need to have an id before we can add contacts
	                if ($routeParams.Id < 0) {
	                    notificationsService.error("Nope.", "You must save the Opportunity before you add a new Contact.");
	                    return false;
	                }

	                contactResource.saveContact(contact).then(function (response) {
	                    var newContact = response.data;
	                    $scope.Contacts.push(newContact);
	                    $scope.pipeline.Contact = newContact;
	                    $scope.pipeline.ContactId = newContact.Id;

	                    //navigationService.syncTree({ tree: 'contactTree', path: [-1, -1], forceReload: true });
	                    notificationsService.success("OK", newContact.Name + " has been created");

	                    pipelineResource.savePipeline($scope.pipeline);
	                });
	            }
	        });
	    }

	    // add organisation dialog
	    $scope.addOrganisation = function () {

	        // we need to have an id before we can add dependencies
	        if ($routeParams.id == "-1") {
	            notificationsService.error("Oops...", "You must save the Opportunity before you add a new Organisation.");
	            return false;
	        }

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/organisation.add.html',
	            dialogData: {},
	            callback: function (data) {

	                organisationResource.saveOrganisation(data).then(function (response) {

	                    var newOrg = response.data;
	                    $scope.Organisations.unshift(newOrg);
	                    $scope.pipeline.Organisation = newOrg;
	                    $scope.pipeline.OrganisationId = newOrg.Id;

	                    pipelineResource.savePipeline($scope.pipeline);

	                    //navigationService.syncTree({ tree: 'organisationTree', path: [-1, -1], forceReload: true });
	                    notificationsService.success("OK", newOrg.Name + " has been created");
	                });
	            }
	        });
	    };


	});

/*  -------------------------
    Contacts
    ------------------------- */

angular.module("umbraco").controller("Pipeline.Contact.Browse",
	function ($scope, $routeParams, pipelineResource, organisationResource, contactResource, notificationsService, navigationService, dialogService, entityResource, mediaResource, iconHelper, $filter, localizationService) {

	    $scope.content = {
            typeId: $routeParams.id,
	        isArchive: $routeParams.id == -1,
	        title: $routeParams.id == -2 ?
                localizationService.localize('pipeline_unassigned') : ($routeParams.id == -1 ? localizationService.localize('recycleBin') : localizationService.localize('pipeline_showAll'))
	    };

	    if ($routeParams.id > 0){
	        contactResource.getContactTypeById($routeParams.id).then(function (response) {
	            $scope.content.title = response.data.Name;
	        });
	    }
	});


angular.module("umbraco").controller("Pipeline.ContactEditController",
	function ($scope, $routeParams, contactResource, pipelineResource, organisationResource, propResource, notificationsService, navigationService, dialogService, entityResource, iconHelper, localizationService) {

	    $scope.loaded = false;

	    contactResource.getContactTypes().then(function (response) {
	        $scope.ContactTypes = response.data;	        
	    });

	    $scope.content = {
	        tabs: [
                { id: 1, label: localizationService.localize('pipeline_information') },
                { id: 2, label: localizationService.localize('pipeline_timeline') },
                { id: 3, label: localizationService.localize('pipeline_opportunities') }
	        ],
	        isArchive: $routeParams.id == -1,
	        isCreate: $routeParams.create
	    };

	    // get doc type definition
	    $scope.getCustomProps = function () {
	        propResource.getPropsByType('contact').then(function (response) {

	            $scope.customprops = response.data;
	            $scope.customprops.reverse().forEach(function (tab) {

	                // tabs
	                tab.id = $scope.content.tabs.length + 1;
	                $scope.content.tabs.splice(1, 0, { id: $scope.content.tabs.length + 1, label: tab.name });

	                var customProps = angular.fromJson($scope.contact.CustomProps);

	                // match values
	                if ($scope.contact.CustomProps) {
	                    tab.items.forEach(function (prop) {
	                        var match = _.findWhere(customProps, { id: prop.id });
	                        if (match) {
	                            prop.value = match.value;
	                        }
	                    });
	                }
	            });

	            $('.tab-content>.tab-pane:first').addClass('active'); //force first tab to show
	            $scope.loaded = true;

	        });
	    };

	    if ($routeParams.id == -1) {
	        $scope.contact = {};
	        $scope.contact.Tasks = [];
            $scope.contact.TypeId = $routeParams.typeId || 0;
            $scope.getCustomProps();
	    }
	    else {

	        contactResource.getContactById($routeParams.id).then(function (response) {

	            $scope.contact = $scope.model = response.data;

	            // get contact pipelines
	            pipelineResource.getByContactId($routeParams.id).then(function (response) {
	                $scope.contact.Pipelines = response.data;
	            });

	            $scope.getCustomProps();
	        });
	    }

	   	// save contact
	    $scope.save = function (contact) {

	        // stringify custom props
	        if ($scope.customprops) {
	            var customProps = [];
	            $scope.customprops.forEach(function (tab) {
	                tab.items.forEach(function (prop) {
	                    if (prop.value) {
	                        customProps.push({
	                            id: prop.id,
	                            value: prop.value,
	                            alias: prop.alias
	                        });
	                    }
	                });
	            });
	            contact.CustomProps = angular.toJson(customProps);
	        }

	        contactResource.saveContact(contact).then(function (response) {
	            $scope.contact = $scope.model = angular.extend($scope.contact, response.data);
	            notificationsService.success("OK", contact.Name + " has been saved");
	            $scope.contactForm.$dirty = false;

	            // if creating a new one, got to its Url	            
	            if ($routeParams.id == '-1') {
	                location.href = '#/pipelineCrm/pipelineCrmTree/edit.contact/' + response.data.Id;
	            }
	        });
	    };

	    // archive / restore contact
	    $scope.archiveContact = function () {
	        contactResource.archiveContact($scope.contact).then(function (response) {
	            $scope.contact.Archived = true;
	            //navigationService.syncTree({ tree: 'contactTree', path: [-1, -1], forceReload: true });
	            notificationsService.success("OK", $scope.contact.Name + " has been archived");
	        });
	    };

	    $scope.restoreContact = function () {
	        contactResource.restoreContact($scope.contact).then(function (response) {
	            $scope.contact.Archived = false;
	            //navigationService.syncTree({ tree: 'contactTree', path: [-1, -1], forceReload: true });
	            notificationsService.success("OK", $scope.contact.Name + " has been restored");
	        });
	    };

	    $scope.deleteContact = function () {

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/delete.confirm.html',
	            dialogData: { Type: 'contact' },
	            callback: function (data) {

	                contactResource.deleteById($scope.contact.Id, data.deleteLinks).then(function () {
	                    notificationsService.success("OK", "Contact has been deleted");
	                    location.href = '#/pipelineCrm/pipelineCrmTree/contacts/-1';
	                });
	            }
	        });
	    };

	    // select organisations
	    $scope.selectOrganisation = function () {

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/organisation.select.html',
	            dialogData: {
	                Organisations: $scope.contact.Organisations || []
	            },
	            callback: function (response) {

	                // assign org to this contact and refresh the UI
	                $scope.contact.Organisations = _.union($scope.contact.Organisations, response.Organisations);
	                $scope.contact.OrganisationIds = _.pluck($scope.contact.Organisations, 'Id').join(',');

	                contactResource.saveContact($scope.contact);
	            }
	        });
	    };

	    $scope.removeOrganisation = function (org) {

	        $scope.contact.Organisations = _.reject($scope.contact.Organisations, function (_org) {
	            return _org.Id == org.Id;
	        });
	        $scope.contact.OrganisationIds = _.pluck($scope.contact.Organisations, 'Id').join(',');
	        contactResource.saveContact($scope.contact);
	    }

	    // add organisation dialog
	    $scope.addOrganisation = function () {

	        // we need to have an id before we can add organisations
	        if ($routeParams.id == "-1") {
	            notificationsService.error("Oops...", "You must save the Contact before you add a new Organisation.");
	            return false;
	        }

	        
	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/organisation.add.html',
	            dialogData: {},
	            callback: function (data) {

	                organisationResource.saveOrganisation(data).then(function (response) {

	                    var newOrg = response.data;
	                    $scope.contact.Organisations = $scope.contact.Organisations || [];
	                    $scope.contact.Organisations.push(newOrg);
	                    $scope.contact.OrganisationIds = _.pluck($scope.contact.Organisations, 'Id').join(',');

	                    //navigationService.syncTree({ tree: 'organisationTree', path: [-1, -1], forceReload: true });
	                    notificationsService.success("OK", newOrg.Name + " has been created");

	                    contactResource.saveContact($scope.contact);

	                });
	            }
	        });
	    };

	});

/*  -------------------------
    Organisations
    ------------------------- */

angular.module("umbraco").controller("Pipeline.Organisation.Browse",
	function ($scope, $routeParams, organisationResource, pipelineResource, notificationsService, navigationService, dialogService, entityResource, mediaResource, iconHelper, $filter, localizationService) {

	    // load UI	    
	    $scope.content = {
	        typeId: $routeParams.id,
	        isArchive: $routeParams.id == -1,
	        title: $routeParams.id == -2 ? localizationService.localize('pipeline_unassigned') :
                    ($routeParams.id == -1 ? localizationService.localize('recycleBin') :
                        localizationService.localize('pipeline_showAll'))
	    };

	    if ($routeParams.id > 0) {
	        organisationResource.getOrganisationTypeById($routeParams.id).then(function (response) {
	            $scope.content.title = response.data.Name;
	        });
	    }

	});


angular.module("umbraco").controller("Pipeline.OrganisationEditController",
	function ($scope, $routeParams, taskResource, contactResource, organisationResource, pipelineResource, propResource, notificationsService, navigationService, dialogService, entityResource, iconHelper, localizationService) {

	    $scope.loaded = false;

	    pipelineResource.getUsers().then(function (response) {
	        $scope.Users = response.data;
	    });

	    organisationResource.getOrgTypes().then(function (response) {
	        $scope.OrgTypes = response.data;
	    });

	    $scope.content = {
	        tabs: [
                { id: 1, label: localizationService.localize('pipeline_information') },
                { id: 2, label: localizationService.localize('pipeline_timeline') },
                { id: 3, label: localizationService.localize('pipeline_opportunities') }
	        ]
	    };

	    // get doc type definition
	    $scope.getCustomProps = function () {
	        propResource.getPropsByType('organisation').then(function (response) {

	            $scope.customprops = response.data;
	            $scope.customprops.reverse().forEach(function (tab) {

	                // tabs
	                tab.id = $scope.content.tabs.length + 1;
	                $scope.content.tabs.splice(1, 0, { id: $scope.content.tabs.length + 1, label: tab.name });

	                var customProps = angular.fromJson($scope.organisation.CustomProps);

	                // match values
	                if ($scope.organisation.CustomProps) {
	                    tab.items.forEach(function (prop) {
	                        var match = _.findWhere(customProps, { id: prop.id });
	                        if (match) {
	                            prop.value = match.value;
	                        }
	                    });
	                }
	            });

	            $('.tab-content>.tab-pane:first').addClass('active'); //force first tab to show
	            $scope.loaded = true;

	        });
	    };

	    if ($routeParams.id == -1) {
	        $scope.organisation = $scope.model = {};
	        $scope.organisation.Tasks = [];
	        $scope.organisation.TypeId = $routeParams.typeId || 0;
	        $scope.getCustomProps();
	    }
	    else {
	        organisationResource.getOrganisationById($routeParams.id).then(function (response) {

	            $scope.organisation = $scope.model = response.data;
	            $scope.getCustomProps();
	        });
	    }

	    // select contacts
	    $scope.selectContact = function () {

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/contact.select.html',
	            dialogData: {
	                OrganisationId: $scope.organisation.Id,
	                Contacts: $scope.organisation.Contacts || []
	            },
	            callback: function (response) {
	                contactResource.saveContacts(response.Contacts);
	            }
	        });
	    };

	    // add and remove contacts
	    $scope.addContact = function () {

	        // we need to have an id before we can add contacts
	        if ($routeParams.id == "-1") {
	            notificationsService.error("Oops...", "You must save the Organisation before you add a new Contact.");
	            return false;
	        }

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/contact.add.html',
	            dialogData: {},
	            callback: function (contact) {	                        

	                contact.OrganisationIds = $scope.organisation.Id.toString();
	                contactResource.saveContact(contact).then(function (response) {

	                    var newContact = response.data;
	                    $scope.organisation.Contacts = $scope.organisation.Contacts || [];
	                    $scope.organisation.Contacts.push(newContact);

	                    //navigationService.syncTree({ tree: 'contactTree', path: [-1, -1], forceReload: true });
	                    notificationsService.success("OK", newContact.Name + " has been created");
	                });
	            }
	        });
	    }

	    $scope.removeContact = function (contact) {

	        $scope.organisation.Contacts = _.reject($scope.organisation.Contacts, function (_contact) {
	            return _contact.Id == contact.Id;
	        });

	        var orgIds = contact.OrganisationIds.split(',');
	        orgIds = _.reject(orgIds, function (id) {
	            return id == $scope.organisation.Id;
	        });
	        contact.OrganisationIds = orgIds.join(',');
	        contactResource.saveContact(contact);
	    }

	    $scope.save = function (organisation) {

	        // stringify custom props
	        if ($scope.customprops) {
	            var customProps = [];
	            $scope.customprops.forEach(function (tab) {
	                tab.items.forEach(function (prop) {
	                    if (prop.value) {
	                        customProps.push({
	                            id: prop.id,
	                            value: prop.value,
	                            alias: prop.alias
	                        });
	                    }
	                });
	            });
	            organisation.CustomProps = angular.toJson(customProps);
	        }

	        organisationResource.saveOrganisation(organisation).then(function (response) {
	            $scope.organisation = $scope.model = response.data;	            
	            notificationsService.success("OK", organisation.Name + " has been saved");
	            $scope.organisationForm.$dirty = false;

	            // if creating a new one, got to its Url	            
	            if ($routeParams.id == '-1') {
	                location.href = '#/pipelineCrm/pipelineCrmTree/edit.organisation/' + response.data.Id;
	            }
	        });
	    };

	    // archive / restore org
	    $scope.archiveOrg = function () {
	        organisationResource.archiveOrganisation($scope.organisation).then(function (response) {
	            $scope.organisation.Archived = true;
	            notificationsService.success("OK", $scope.organisation.Name + " has been archived");
	        });
	    };

	    $scope.restoreOrg = function () {
	        organisationResource.restoreOrganisation($scope.organisation).then(function (response) {
	            $scope.organisation.Archived = false;
	            notificationsService.success("OK", $scope.organisation.Name + " has been restored");
	        });
	    };

	    $scope.deleteOrg = function () {

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/delete.confirm.html',
	            dialogData: { Type: 'organisation' },
	            callback: function (data) {

	                organisationResource.deleteOrganisationById($scope.organisation.Id, data.deleteLinks).then(function () {
	                    notificationsService.success("OK", "Organisation has been deleted");
	                    location.href = '#/pipelineCrm/pipelineCrmTree/organisations/-1';
	                });
	            }
	        });
	    };

	    // mark done
	    $scope.toggleTask = function (task) {
	        taskResource.toggleTask(task.Id).then(function (response) {
	            task.Done = !task.Done;
	        });
	    }

	    // contact origin menu
	    $scope.OriginOptions = [
            'Recommendation', 'Umbraco.com', 'Google', 'Adwords', 'Press', 'Other'
	    ];


	});


/*  -------------------------
    Segments
    ------------------------- */

angular.module("umbraco").controller("Pipeline.Segment.Browse",
	function ($scope, $routeParams, segmentResource, notificationsService, navigationService, dialogService, entityResource, mediaResource, iconHelper, $filter, localizationService) {

	    // load UI	    
	    $scope.content = {
	        typeId: $routeParams.id,
	        isArchive: $routeParams.id == -1,
	        title: $routeParams.id == -2 ? localizationService.localize('pipeline_unassigned') :
                    ($routeParams.id == -1 ? localizationService.localize('recycleBin') :
                        localizationService.localize('pipeline_showAll'))
	    };

	    if ($routeParams.id > 0) {
	        segmentResource.getSegmentTypeById($routeParams.id).then(function (response) {
	            $scope.content.title = response.data.Name;
	        });
	    }

	});


angular.module("umbraco").controller("Pipeline.SegmentEditController",
	function ($scope, $routeParams, taskResource, contactResource, organisationResource, segmentResource, pipelineResource, propResource, notificationsService, navigationService, dialogService, entityResource, iconHelper, localizationService) {

	    $scope.loaded = false;

	    segmentResource.getSegmentTypes().then(function (response) {
	        $scope.segmentTypes = response.data;
	    });	   

	    segmentResource.getSegmentCriteria().then(function (response) {
	        $scope.segmentCriteria = response.data;
	    });

	    $scope.content = {
	        tabs: [
                { id: 1, label: localizationService.localize('pipeline_information') },
                { id: 2, label: localizationService.localize('pipeline_timeline') }
	        ]
	    };

	    // get doc type definition
	    $scope.getCustomProps = function () {
	        
	        propResource.getPropsByType('segment').then(function (response) {

	            $scope.customprops = response.data;
	            $scope.customprops.reverse().forEach(function (tab) {

	                // tabs
	                tab.id = $scope.content.tabs.length + 1;
	                $scope.content.tabs.splice(1, 0, { id: $scope.content.tabs.length + 1, label: tab.name });

	                var customProps = angular.fromJson($scope.segment.CustomProps);

	                // match values
	                if ($scope.segment.CustomProps) {
	                    tab.items.forEach(function (prop) {
	                        var match = _.findWhere(customProps, { id: prop.id });
	                        if (match) {
	                            prop.value = match.value;
	                        }
	                    });
	                }
	            });

	            // release the UI
	            $('.tab-content>.tab-pane:first').addClass('active'); //force first tab to show
	            $scope.loaded = true;

	        });
	    };


	    // get criteria params
	    $scope.getCriteriaProps = function () {

	        propResource.getSegmentProps($scope.segment.Criteria).then(function (response) {

	            $scope.criteriaProps = _.pluck(response.data, 'items')[0] || [];
	            $scope.criteriaProps.reverse().forEach(function (tab) {

	                // match values
	                var customProps = angular.fromJson($scope.segment.CriteriaProps);
	                if ($scope.segment.CriteriaProps) {
	                    $scope.criteriaProps.forEach(function (prop) {
	                        var match = _.findWhere(customProps, { id: prop.id });
	                        if (match) {
	                            prop.value = match.value;
	                        }
	                    });
	                }
	                $scope.runPreview();
	            });

	        });

	    };

	    if ($routeParams.id == -1) {
	        $scope.segment = $scope.model = {};
	        $scope.segment.Tasks = [];
	        $scope.segment.TypeId = $routeParams.typeId || 0;
	        $scope.getCustomProps();
	    }
	    else {
	        segmentResource.getSegmentById($routeParams.id).then(function (response) {
	            $scope.segment = $scope.model = response.data;	            
	            $scope.getCustomProps();
	            $scope.getCriteriaProps();
	            $scope.runPreview();
	        });
	    }
	    
	    // run segment report
	    $scope.runPreview = function () {
	        	     
	        segmentResource.getSegmentPreview($routeParams.id, $scope.segment.Criteria).then(function (response) {
	            $scope.segment.Results = response.data;
	        });
	    };

	    // select contacts
	    $scope.selectContact = function () {

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/contact.select.html',
	            dialogData: {
	                segmentId: $scope.segment.Id,
	                Contacts: $scope.segment.Contacts || []
	            },
	            callback: function (response) {
	                $scope.segment.Contacts = _.union($scope.segment.Contacts, response.Contacts);
	            }
	        });
	    };

	    // add and remove contacts
	    $scope.addContact = function () {

	        // we need to have an id before we can add contacts
	        if ($routeParams.id == -1) {
	            notificationsService.error("Oops...", "You must save the Segment before you add a new Contact.");
	            return false;
	        }

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/contact.add.html',
	            dialogData: {},
	            callback: function (contact) {
	                        
	                contactResource.saveContact(contact).then(function (response) {
	                    var newContact = response.data;
	                    $scope.segment.Contacts = $scope.segment.Contacts || [];
	                    $scope.segment.Contacts.push(newContact);
	                    notificationsService.success("OK", newContact.Name + " has been created");
	                });
	            }
	        });
	    }

	    $scope.removeContact = function (contact) {

	        $scope.segment.Contacts = _.reject($scope.segment.Contacts, function (_contact) {
	            return _contact.Id == contact.Id;
	        });	        
	    }

	    // select organisations
	    $scope.selectOrganisation = function () {

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/organisation.select.html',
	            dialogData: {
	                segmentId: $scope.segment.Id,
	                Organisations: $scope.segment.Organisations || []
	            },
	            callback: function (response) {
	                $scope.segment.Organisations = _.union($scope.segment.Organisations, response.Organisations);
	            }
	        });
	    };

	    // add and remove contacts
	    $scope.addOrganisation = function () {

	        // we need to have an id before we can add organisations
	        if ($routeParams.id == -1) {
	            notificationsService.error("Oops...", "You must save the Segment before you add a new Organisation.");
	            return false;
	        }

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/organisation.add.html',
	            dialogData: {},
	            callback: function (org) {
	                      
	                organisationResource.saveOrganisation(org).then(function (response) {
	                    var newOrg = response.data;
	                    $scope.segment.Organisations = $scope.segment.Organisations || [];
	                    $scope.segment.Organisations.push(newOrg);
	                    notificationsService.success("OK", newOrg.Name + " has been created");
	                });
	            }
	        });
	    }

	    $scope.removeOrganisation = function (org) {

	        $scope.segment.Organisations = _.reject($scope.segment.Organisations, function (_org) {
	            return _org.Id == org.Id;
	        });
	    }

	    $scope.save = function (segment) {

            // join in contact and org Ids
	        $scope.segment.ContactIds = _.pluck($scope.segment.Contacts, 'Id').join(',');
	        $scope.segment.OrganisationIds = _.pluck($scope.segment.Organisations, 'Id').join(',');

	        // stringify custom props
	        if ($scope.customprops) {
	            var customProps = [];
	            $scope.customprops.forEach(function (tab) {
	                tab.items.forEach(function (prop) {
	                    if (prop.value) {
	                        customProps.push({
	                            id: prop.id,
	                            value: prop.value,
	                            alias: prop.alias
	                        });
	                    }
	                });
	            });
	            segment.CustomProps = angular.toJson(customProps);
	        }

	        // stringify criteria props
	        if ($scope.criteriaProps) {
	            var criteriaProps = [];
	            $scope.criteriaProps.forEach(function (prop) {
	                if (prop.value) {
	                    criteriaProps.push({
	                        id: prop.id,
	                        value: prop.value,
	                        alias: prop.alias
	                    });
	                };
	            });
	            segment.CriteriaProps = angular.toJson(criteriaProps);
	        }

	        segmentResource.saveSegment(segment).then(function (response) {
	            notificationsService.success("OK", segment.Name + " has been saved");
                $scope.segmentForm.$dirty = false;
                
	            // if creating a new one, got to its Url
                if ($routeParams.id == -1) {
                    location.href = '#/pipelineCrm/pipelineCrmTree/edit.segment/' + response.data.Id;
                } else {
                    $scope.runPreview();
                }
	        });
	    };

	    // archive / restore segment
	    $scope.archiveSegment = function () {
	        segmentResource.archiveSegment($scope.segment).then(function (response) {
	            $scope.segment.Archived = true;
	            //navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
	            notificationsService.success("OK", $scope.segment.Name + " has been archived");
	        });
	    };

	    $scope.restoreSegment = function () {
	        segmentResource.restoreSegment($scope.segment).then(function (response) {
	            $scope.segment.Archived = false;
	            //navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
	            notificationsService.success("OK", $scope.segment.Name + " has been restored");
	        });
	    };

	    $scope.deleteSegment = function () {

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/delete.confirm.html',
	            dialogData: { Type: 'segment' },
	            callback: function (data) {

	                segmentResource.deleteSegmentById($scope.segment.Id, data.deleteLinks).then(function () {
	                    notificationsService.success("OK", "segment has been deleted");
	                    location.href = '#/pipelineCrm/pipelineCrmTree/segments/-1';
	                });
	            }
	        });
	    };
	    
	});

/*  -------------------------
    Settings
    ------------------------- */

angular.module("umbraco").controller("Pipeline.Settings",
    function ($scope, $routeParams, pipelineResource, organisationResource, contactResource, segmentResource, prefResource, navigationService, notificationsService, $timeout, localizationService) {
        var timer;

        // load UI
        $scope.loaded = false;

        $scope.content = {
            tabs: [
                { id: 1, label: localizationService.localize('pipeline_opportunities') },
                { id: 2, label: localizationService.localize('pipeline_organisations') },
                { id: 3, label: localizationService.localize('pipeline_contacts') },
                { id: 4, label: localizationService.localize('pipeline_segments') },
                { id: 5, label: localizationService.localize('pipeline_notifications') }
            ],
        };

        //User Prefs
        prefResource.getUserPreferences().then(function (response) {
            $scope.preferences = response.data;

            $scope.digestHour = [
                { value: "none", name: "None" },
                { value: "daily", name: "Daily" },
                { value: "weekly", name: "Weekly" }
            ];

            $scope.loaded = true;
        });

        pipelineResource.getStatuses().then(function (response) {
            $scope.statuses = response.data;
            $scope.loaded = true;
        });

        organisationResource.getOrgTypes().then(function (response) {
            $scope.orgTypes = response.data;
            $scope.loaded = true;
        });

        contactResource.getContactTypes().then(function (response) {
            $scope.contactTypes = response.data;
            $scope.loaded = true;
        });

        segmentResource.getSegmentTypes().then(function (response) {
            $scope.segmentTypes = response.data;
            $scope.loaded = true;
        });

        $scope.newStatus = {};
        $scope.newType = {};
        $scope.newOrgType = {};
        $scope.newContactType = {};
        $scope.newSegmentType = {};

        // pipeline statuses
        $scope.addStatus = function () {
            $scope.statuses.push({
                Name: $scope.newStatus.Name,
                updated: true
            });
            $scope.newStatus.Name = '';
        };

        $scope.updateStatus = function (status) {
            status.updated = true;
        };

        $scope.removeStatus = function (status) {
            status.deleted = true;
        };        

        // org types
        $scope.addOrgType = function () {
            $scope.orgTypes.push({
                Name: $scope.newOrgType.Name,
                updated: true
            });
            $scope.newOrgType.Name = '';
        };

        $scope.updateOrgType = function (type) {
            type.updated = true;
        };

        $scope.removeOrgType = function (type) {
            type.deleted = true;
        };

        // contact types
        $scope.addContactType = function () {
            $scope.contactTypes.push({
                Name: $scope.newContactType.Name,
                updated: true
            });
            $scope.newContactType.Name = '';
        };

        $scope.updateContactType = function (type) {
            type.updated = true;
        };

        $scope.removeContactType = function (type) {
            type.deleted = true;
        };

        // contact types
        $scope.addSegmentType = function () {
            $scope.segmentTypes.push({
                Name: $scope.newSegmentType.Name,
                updated: true
            });
            $scope.newSegmentType.Name = '';
        };

        $scope.updateSegmentType = function (type) {
            type.updated = true;
        };

        $scope.removeSegmentType = function (type) {
            type.deleted = true;
        };

        // persist and verify
        $scope.save = function () {

            $scope.statuses.forEach(function (status) {
                if (status.updated) {
                    pipelineResource.saveStatus(status);
                }
                if (status.deleted) {
                    pipelineResource.deleteStatus(status.Id);
                }
            });

            $scope.orgTypes.forEach(function (type) {
                if (type.updated) {
                    organisationResource.saveOrgType(type);
                }
                if (type.deleted) {
                    organisationResource.deleteOrgType(type.Id);
                }
            });

            $scope.contactTypes.forEach(function (type) {
                if (type.updated) {
                    contactResource.saveContactType(type);
                }
                if (type.deleted) {
                    contactResource.deleteContactType(type.Id);
                }
            });

            $scope.segmentTypes.forEach(function (type) {
                if (type.updated) {
                    segmentResource.saveSegmentType(type);
                }
                if (type.deleted) {
                    segmentResource.deleteSegmentType(type.Id);
                }
            });

            prefResource.savePreferences($scope.preferences);
            notificationsService.success("Changes have been saved");
            navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
            $scope.settingsForm.$dirty = false;
        }
    });

/*  -------------------------
    Components
    ------------------------- */

angular.module("umbraco").controller("Pipeline.Grid",
	function ($scope, $routeParams, segmentResource, pipelineResource, organisationResource, contactResource, navigationService, dialogService, notificationsService, localizationService) {

        // load statuses and types
	    $scope.$watch('type', function () {
	        if ($scope.type == 'P') {
	            pipelineResource.getStatuses().then(function (response) {
	                $scope.Statuses = response.data;
	            });

	        } else if ($scope.type == 'C') {
	            contactResource.getContactTypes().then(function (response) {
	                $scope.Types = response.data;
	            });

	        } else if ($scope.type == 'O') {
	            organisationResource.getOrgTypes().then(function (response) {
	                $scope.Types = response.data;
	            });
	        } else if ($scope.type == 'S') {
	            segmentResource.getSegmentTypes().then(function (response) {
	                $scope.Types = response.data;
	            });
	        }
	    });

	    // init UI
	    $scope.loaded = false;
	    $scope.currentPage = 1;
	    $scope.totalPages = 1;
	    $scope.reverse = false;
	    $scope.searchTerm = '';

	    function fetchData() {           

	        $scope.loaded = false;
	        if ($scope.type == 'P') {
	            $scope.predicate = $scope.predicate || 'DateComplete';
	            pipelineResource.getPaged($scope.currentPage, $scope.predicate, !$scope.reverse ? "desc" : "asc", $scope.searchTerm, ($scope.contactId || $scope.organisationId ? 0 : $routeParams.id), $scope.contactId, $scope.organisationId).then(function (response) {
	                $scope.results = response.data.Pipelines;
	                $scope.totalPages = response.data.TotalPages;                    
	                $scope.loaded = true;
	            }, function (response) {
	                notificationsService.error("Error", "Could not load pipelines");
	            });

	        } else if ($scope.type == 'C') {
	            $scope.predicate = $scope.predicate || 'DateUpdated';
                contactResource.getPaged($scope.currentPage, $scope.predicate, !$scope.reverse ? "desc" : "asc", $scope.searchTerm, $routeParams.id).then(function (response) {
	                $scope.results = response.data.Contacts;
	                $scope.totalPages = response.data.TotalPages;                    
	                $scope.loaded = true;
	            }, function (response) {
	                notificationsService.error("Error", "Could not load contacts");
	            });

	        } else if ($scope.type == 'O') {
	            $scope.predicate = $scope.predicate || 'DateUpdated';
	            organisationResource.getPaged($scope.currentPage, $scope.predicate, !$scope.reverse ? "desc" : "asc", $scope.searchTerm, $routeParams.id).then(function (response) {
	                $scope.results = response.data.Organisations;
	                $scope.totalPages = response.data.TotalPages;
	                $scope.loaded = true;
	            }, function (response) {
	                notificationsService.error("Error", "Could not load organisations");
	            });
	        } else if ($scope.type == 'S') {
	            $scope.predicate = $scope.predicate || 'DateUpdated';
	            segmentResource.getPaged($scope.currentPage, $scope.predicate, !$scope.reverse ? "desc" : "asc", $scope.searchTerm, $routeParams.id).then(function (response) {
	                $scope.results = response.data.Segments;
	                $scope.totalPages = response.data.TotalPages;
	                $scope.loaded = true;
	            }, function (response) {
	                notificationsService.error("Error", "Could not load segments");
	            });
	        }
	    };

        // grid functions
	    $scope.order = function (predicate) {
	        $scope.reverse = ($scope.predicate === predicate) ? !$scope.reverse : false;
	        $scope.predicate = predicate;
	        $scope.currentPage = 1;
	        fetchData();
	    };

	    $scope.prevPage = function () {
	        if ($scope.currentPage > 1) {
	            $scope.currentPage--;
	            fetchData();
	        }
	    };

	    $scope.nextPage = function () {
	        if ($scope.currentPage < $scope.totalPages) {
	            $scope.currentPage++;
	            fetchData();
	        }
	    };

	    $scope.setPage = function (pageNumber) {
	        $scope.currentPage = pageNumber;
	        fetchData();
	    };	    

	    $scope.search = function (searchFilter) {
	        $scope.searchTerm = searchFilter;
	        $scope.currentPage = 1;
	        fetchData();
	    };

	    $scope.anySelected = function () {
	        return _.findWhere($scope.results, { selected: true });
	    };

	    $scope.getNumber = function (num) {
	        return new Array(num);
	    }

	    $scope.selectAll = function ($event) {
	        $scope.isSelectedAll = !$scope.isSelectedAll;
	        $scope.results.forEach(function (item) {
	            item.selected = $scope.isSelectedAll;
	        });
	    };

	    $scope.isRowSelected = function (row) {
	        return row.selected;
	    };

        // move and delete items
	    $scope.changeSelected = function (status) {

	        var selected = [],
                confirmation = "";

	        $scope.results.forEach(function (item) {
	            if (item.selected) {
	                if (status == -1) {
	                    item.Archived = true;
	                    confirmation = "archived";
	                } else if (status == 0) {
	                    item.Archived = false;
	                    confirmation = "restored";
	                } else {
	                    item.StatusId = item.TypeId = status.Id;
	                    confirmation = "moved to " + status.Name;
	                }
	                selected.push(item);
	            }
	        });

	        if (selected.length) {

	            if ($scope.type == 'P') {
	                pipelineResource.savePipelines(selected).then(function (response) {
	                    notificationsService.success("OK", selected.length + " items have been " + confirmation);
	                    fetchData();
	                });
	            }
	            else if ($scope.type == 'O') {
	                organisationResource.saveOrganisations(selected).then(function (response) {
	                    notificationsService.success("OK", selected.length + " items have been " + confirmation);
	                    fetchData();
	                });
	            }
	            else if ($scope.type == 'C') {
	                contactResource.saveContacts(selected).then(function (response) {
	                    notificationsService.success("OK", selected.length + " items have been " + confirmation);
	                    fetchData();
	                });
	            }
	            else if ($scope.type == 'S') {
	                segmentResource.saveSegments(selected).then(function (response) {
	                    notificationsService.success("OK", selected.length + " items have been " + confirmation);
	                    fetchData();
	                });
	            }
	        }
	    };

	    $scope.deleteSelected = function () {

	        var selected = _.where($scope.results, { selected: true }),
                selectedIds = _.pluck(selected, 'Id').join(',');

	        if (selected.length) {
	            var dialog = dialogService.open({
	                template: '/App_Plugins/PipelineCRM/dialogs/delete.confirm.html',
	                dialogData: { Type: 'group of items' },
	                callback: function (data) {

	                    if ($scope.type == 'P') {
	                        pipelineResource.deletePipelines(selectedIds, data.deleteLinks).then(function (response) {
	                            fetchData();
	                        });
	                    }
	                    else if ($scope.type == 'O') {
	                        organisationResource.deleteOrganisations(selectedIds, data.deleteLinks).then(function (response) {
	                            fetchData();
	                        });
	                    }
	                    else if ($scope.type == 'C') {
	                        contactResource.deleteContacts(selectedIds, data.deleteLinks).then(function (response) {
	                            fetchData();
	                        });
	                    }
	                    else if ($scope.type == 'S') {
	                        segmentResource.deleteSegments(selectedIds, data.deleteLinks).then(function (response) {
	                            fetchData();
	                        });
	                    }
	                    notificationsService.success("OK", selected.length + " items have been deleted");
	                }
	            });
	        }
	    }

	    $scope.$watchCollection('[type,contactId,organisationId]', function () {
	        if ($scope.type)
	            fetchData(); // kick it!
	    });
	});

angular.module("umbraco").controller("Pipeline.Timeline",
	function ($scope, $timeout, $routeParams, $filter, pipelineResource, dialogService, taskResource, notificationsService) {
	    
	    // mark task with date, and make date repeater
	    $scope.splitTimeline = function () {	        
	        $scope.months = [];
	        if ($filter('orderBy')($scope.tasks,['DateDue','DateCreated'],true)) {
	            $scope.tasks.forEach(function (task) {

	                var sortDate = task.DateDue.toString().IsDate() ? task.DateDue : task.DateCreated,
                        created = new Date(sortDate),
                        month = created.getMonth(),
                        year = created.getFullYear();
	                //monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

                    var taskMonth = $scope.months.filter(function (_month) {
	                    return _month.stamp == month + year;
	                })[0] || {
	                    title: moment(sortDate).format('MMMM YYYY'),
	                    stamp: month + year,
	                    tasks: []
	                };

	                taskMonth.tasks.push(task);

	                if (taskMonth.tasks.length == 1) {
	                    $scope.months.unshift(taskMonth);
	                }

	            });
	        };
	    };

	    // create, edit and delete  task
	    $scope.editTask = function (task) {

	        var dialog = dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/task.add.html',
	            dialogData: task || {},
	            callback: function (newTask) {

	                // we need to have an id before we can add contacts
	                if ($routeParams.Id < 0) {
	                    notificationsService.error("Nope.", "You must save the Pipeline before you add a new Touchpoint.");
	                    return false;
	                }

                    newTask.Member = {}; // gotcha: do NOT post Member, or the Member.Id gets read as Id << ng-bollocks

	                // get the parent object
                    newTask = $scope.addParentId(newTask);

	                taskResource.saveTask(newTask).then(function (response) {
	                    var taskSaved = response.data;
	                    if (task) {
	                        task = taskSaved;
	                    } else {
	                        $scope.tasks = $scope.tasks || [];
	                        $scope.tasks.push(taskSaved);
	                    }
	                    $scope.splitTimeline();
	                    notificationsService.success("OK", "Touchpoint has been saved");

	                });
	            }
	        });
	    }

	    $scope.deleteTask = function (task) {

	        var taskId = task.Id,
                dialog = dialogService.open({
                    template: '/App_Plugins/PipelineCRM/dialogs/delete.confirm.html',
                    dialogData: { Type: 'task' },
                    callback: function () {

                        taskResource.deleteById(taskId).then(function (task) {
                            $scope.tasks = _.without($scope.tasks, _.findWhere($scope.tasks, { Id: taskId }));
                            $scope.splitTimeline();
                            notificationsService.success("OK", "Task has been deleted");
                        });
                    }
                });
	    }

	    $scope.shareTask = function (task) {

	        var dialog = dialogService.open({
                    template: '/App_Plugins/PipelineCRM/dialogs/task.share.html',
                    dialogData: task,
                    callback: function () {                        
                    }
                });
	    }

	    // mark done
	    $scope.toggleTask = function (task) {
	        taskResource.toggleTask(task.Id).then(function (response) {
	            task.DateComplete = response.data.DateComplete
	            task.Done = !task.Done;
	        });
	    }

	   	// get id of parent object
	    $scope.addParentId = function (newTask) {
	        if ($scope.parentType == 'pipeline') {
	            newTask.PipelineId = newTask.PipelineId || $scope.parent.Id;
	        } else if ($scope.parentType == 'organisation') {
	            newTask.OrganisationId = newTask.OrganisationId || $scope.parent.Id;
	        } else if ($scope.parentType == 'contact') {
	            newTask.ContactId = newTask.ContactId || $scope.parent.Id;
	        } else if ($scope.parentType == 'segment') {
	            newTask.SegmentId = newTask.SegmentId || $scope.parent.Id;
	        }
	        return newTask;
	    };

	    $scope.$watchCollection('[parent,tasks,parentType,summary]', function () {
	        if ($scope.parent) {
	            $scope.splitTimeline(); // kick it!
	        }
	    });

    });

/*  -------------------------
    Dashboards
    ------------------------- */

    angular.module("umbraco").controller("Pipeline.Dashboards.Tasks",
    function ($scope, $routeParams, taskResource, pipelineResource, dialogService) {
	    taskResource.getMyTasks().then(function (response) {
	        $scope.tasks = response.data;
	        $scope.loaded = true;
	    });
    });

    angular.module("umbraco").controller("Pipeline.Dashboards.Pipelines",
    function ($scope, $routeParams, $filter, pipelineResource, organisationResource, prefResource, dialogService, navigationService, notificationsService, localizationService) {

        $scope.CurrentPage = 0;
        $scope.TotalPages = 1;

        prefResource.getConfig().then(function (response) {
            if (response.data['UseBoard'] != 'true') {
                location.href = '#pipeline/pipelineCrmTree/browse/0';
            } else {
                pipelineResource.getStatuses().then(function (response) {
                    $scope.Statuses = response.data;
                    $scope.Statuses.unshift({ Id: -1, Name: 'Unassigned' });

                    $scope.$watch('pipelines', function () {
                        refreshBoard();
                    });
                });

                // get all pipelines
                $scope.pipelines = $scope.pipelinesList = [];
                $scope.loadPage();
            }
        });
	    

	    // load next page
	    $scope.loadPage = function () {
	        $scope.loadedData = false;
	        $scope.CurrentPage = $scope.CurrentPage + 1;

	        pipelineResource.getPaged($scope.CurrentPage,'','','',0).then(function (response) {
	            $scope.TotalPages = response.data.TotalPages;
	            $scope.ItemsPerPage = response.data.ItemsPerPage;
	            $scope.TotalItems = response.data.TotalItems;
	            $scope.PageItems = Math.min($scope.TotalItems, $scope.ItemsPerPage * $scope.CurrentPage);
	            $scope.pipelines = $scope.pipelinesList = _.union($scope.pipelines, response.data.Pipelines);
	            $scope.loadedData = $scope.loaded = true;
	            refreshBoard();
	        });
	    }

	    // refresh board when cards move
	    var refreshBoard = function () {
	        if ($scope.pipelines && $scope.pipelines.length) {
	            $scope.Statuses.forEach(function (status) {
	                status.pipelines = _.filter($scope.pipelines, function (pipeline) {
	                    return (!pipeline.StatusId && status.Id == -1) || pipeline.StatusId == status.Id;
	                });

	                var sum = 0;
	                status.pipelines.forEach(function (pipeline) {
	                    sum = sum + $filter('adjustedValue')(pipeline);
	                });
	                status.totalValue = sum;

	            });
	            
	            if (!$scope.$$phase)
	                $scope.$apply();
	        }
	    };

	    //dragdrop 
	    $scope.onDrop = function (pipelineId, statusId) {

	        var pipeline = _.findWhere($scope.pipelines, { Id: pipelineId });
	        if (pipeline && pipeline.StatusId != statusId) {

	            pipeline.SortOrder = _.filter($scope.pipelines, function (p) { return p.StatusId == statusId }).length;
	            pipeline.StatusId = statusId;

	            refreshBoard();
	            pipelineResource.savePipeline(pipeline).then(function (response) {
	                navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
	            });
	        }
	        return false;
	    };

	    $scope.onCardDrop = function (sourceId, targetId, dropPos) {

	        if (sourceId == targetId)
	            return false;

	        var dragged = _.findWhere($scope.pipelines, { Id: sourceId }),
	            dropped = _.findWhere($scope.pipelines, { Id: targetId });

	        if (dragged && dropped) {

	            dragged.StatusId = dropped.StatusId;
	            var newSlot = dropped.SortOrder;
	            dragged.SortOrder = dropPos == 0 ? newSlot : newSlot - 1;
	            dropped.SortOrder = dropPos == 0 ? newSlot - 1 : newSlot;

	            var list = _.sortBy(_.filter($scope.pipelines, function (p) {
	                return p.StatusId == dropped.StatusId;
	            }), 'SortOrder');

	            for (i = 0; i < list.length; i++) {
	                list[i].SortOrder = i;
	            }

	            refreshBoard();
	            pipelineResource.savePipelines(list).then(function (response) {
	                navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
	            });
	        }
	        return false;
	    };

	    // preview dialog
	    $scope.previewPipeline = function (pipeline) {

	        dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/pipeline.summary.html',
	            dialogData: pipeline
	        });
	    };

	    // add pipeline dialog
	    $scope.addPipeline = function (status) {

            dialogService.open({
	            template: '/App_Plugins/PipelineCRM/dialogs/pipeline.add.html',
	            dialogData: {
	                StatusId: Math.max(0, status.Id),
	                Status: status
	            },
	            callback: function (pipeline) {
	                pipelineResource.quickCreatePipeline(pipeline).then(function (response) {
	                    console.log(response.data);
	                    $scope.pipelines.push(response.data);
	                    refreshBoard();
	                    navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
	                });
	            }
	        });	        
	    };
    });


/*  -------------------------
    Dialogs
    ------------------------- */

    angular.module("umbraco").controller("Pipeline.Dialogs.SelectContacts",
        function ($scope, $routeParams, contactResource, dialogService) {
            
            $scope.unassigned = [];

            $scope.$watch('searchTerm', function () {
                if ($scope.searchTerm && $scope.searchTerm.length) {
                    contactResource.getContactsByName($scope.searchTerm).then(function (response) {
                        $scope.unassigned = response.data;
                    });
                } else {
                    $scope.unassigned = [];
                }
            });

            $scope.isSelected = function (contact) {
                return _.findWhere($scope.dialogData.Contacts, { Id: contact.Id });
            };

            $scope.toggle = function (contact) {

                $scope.dialogData.Contacts = $scope.dialogData.Contacts || [];
                var orgIds = contact.OrganisationIds ? contact.OrganisationIds.split(',') : [];

                if (_.findWhere($scope.dialogData.Contacts, { Id: contact.Id })) {
                    orgIds = _.reject(orgIds, function (id) {
                        return id == $scope.dialogData.OrganisationId;
                    });
                    $scope.dialogData.Contacts = _.without($scope.dialogData.Contacts, _.findWhere($scope.dialogData.Contacts, { Id: contact.Id }));

                } else {
                    orgIds.push($scope.dialogData.OrganisationId);
                    $scope.dialogData.Contacts.push(contact);
                }

                contact.OrganisationIds = orgIds.join(',');
            };

        }
    );

    angular.module("umbraco").controller("Pipeline.Dialogs.SelectOrganisations",
        function ($scope, $routeParams, organisationResource, dialogService) {

            $scope.organisations = [];

            $scope.$watch('searchTerm', function () {
                if ($scope.searchTerm && $scope.searchTerm.length) {
                    organisationResource.getOrganisationsByName($scope.searchTerm).then(function (response) {
                        $scope.organisations = response.data;
                    });
                } else {
                    $scope.organisations = [];
                }
            });

            $scope.isSelected = function (org) {
                return _.findWhere($scope.dialogData.Organisations, { Id: org.Id });
            };

            $scope.toggle = function (org) {

                $scope.dialogData.Organisations = $scope.dialogData.Organisations || [];

                if (_.findWhere($scope.dialogData.Organisations, { Id: org.Id })) {
                    $scope.dialogData.Organisations = _.without($scope.dialogData.Organisations, _.findWhere($scope.dialogData.Organisations, { Id: org.Id }));
                } else {
                    $scope.dialogData.Organisations.push(org);
                }
            };

        }
    );

    angular.module("umbraco").controller("Pipeline.Dialogs.Summary",
        function ($scope, $routeParams, taskResource, pipelineResource, dialogService, labelResource, navigationService) {

            pipelineResource.getUsers().then(function (response) {
                $scope.Users = response.data;
            });

            labelResource.getAll().then(function (response) {
                $scope.Labels = response.data;
            });

            $scope.toggleLabel = function (label) {

                var labelIds = $scope.dialogData.LabelIds ? $scope.dialogData.LabelIds.split(',').map(Number) : [],
                    labels = $scope.dialogData.Labels || [];

                if (labelIds.indexOf(label.Id) > -1) {
                    labelIds = _.without(labelIds, label.Id);
                    labels = _.without(labels, _.findWhere(labels, { Id: label.Id }));
                } else {
                    labelIds.push(label.Id);
                    labels.push(label);
                }

                $scope.dialogData.LabelIds = labelIds ? labelIds.join(',') : '';
                $scope.dialogData.Labels = labels;
                $scope.save();
            };

            $scope.save = function () {
                pipelineResource.savePipeline($scope.dialogData);
            };

            $scope.archive = function () {
                pipelineResource.archivePipeline($scope.dialogData).then(function (response) {
                    $scope.close();
                    //navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
                    $scope.dialogData.Archived = true;
                });
            };

            $scope.duplicate = function () {
                pipelineResource.duplicatePipeline($scope.dialogData).then(function (response) {
                    $scope.close();
                    //navigationService.syncTree({ tree: 'pipelineCrmTree', path: [-1, -1], forceReload: true });
                    notificationsService.success("OK", $scope.pipeline.Name + " has been duplicated");
                    location.href = '#/pipelineCrm/pipelineCrmTree/edit/' + response.data.Id;
                });
            };

        }
    );

    angular.module("umbraco").controller("Pipeline.Dialogs.AddTask",
        function ($scope, $routeParams, pipelineResource, dialogService, localizationService, userService) {

            $scope.isTask = $scope.dialogData.DateDue && $scope.dialogData.DateDue.toString().IsDate();

            $scope.toggleTask = function () {
                $scope.isTask = !$scope.isTask;
            }

            pipelineResource.getUsers().then(function (response) {
                $scope.Users = response.data;
            });

            userService.getCurrentUser().then(function (response) {
                $scope.dialogData.UserId = response.id;
            });

            $scope.Types = [
                'Meeting', 'Call', 'Email', 'Send file', 'Other' // TODO: move to settings
            ];

            // date picker
            $scope.datePickerConfig = {
                view: 'datepicker',
                config: {
                    pickDate: true,
                    pickTime: false,
                    pick12HourFormat: false,
                    useSeconds: false,
                    format: "YYYY-MM-DD"
                },
                value: ''
            };

            $scope.taskDatepicker = $scope.dialogData.DateDue && $scope.dialogData.DateDue.toString().IsDate() ?
                angular.extend({}, $scope.datePickerConfig, { value:moment($scope.dialogData.DateDue) }) : $scope.datePickerConfig;

            $scope.reminderDatepicker = $scope.dialogData.Reminder && $scope.dialogData.Reminder.toString().IsDate() ?
                angular.extend({}, $scope.datePickerConfig, { value: moment($scope.dialogData.Reminder) }) : $scope.datePickerConfig; 

            $scope.presubmit = function () { 
                $scope.dialogData.DateDue = new Date($scope.isTask && $scope.taskDatepicker.value ? moment.utc(new Date($scope.taskDatepicker.value)) : '');
                $scope.dialogData.Reminder = new Date($scope.isTask && $scope.reminderDatepicker.value ? moment.utc(new Date($scope.reminderDatepicker.value)) : '');

                $scope.isTask = false;
                if ($scope.dialogData.DateDue && $scope.dialogData.DateDue.toString().IsDate()) {
                    $scope.dialogData.Overdue = moment().diff($scope.dialogData.DateDue, 'days');
                    $scope.isTask = true;
                }
                
                $scope.submit($scope.dialogData);
            };

            // file picker
            $scope.openMediaPicker = function () {
                dialogService.mediaPicker({
                    multiPicker: true,
                    callback: function (media) {

                        $scope.dialogData.Media = $scope.dialogData.Media || [];
                        media.forEach(function (file) {

                            var newMedia = {
                                Id: file.id,
                                Name: file.name,
                                Url: file.image
                            };
                            
                            if (!_.where($scope.dialogData.Media, { 'Id': newMedia.Id }).length) {
                                $scope.dialogData.Media.push(newMedia);
                                $scope.dialogData.Files = _.pluck($scope.dialogData.Media, 'Id').join(',');
                            }
                        });
                    }
                });
            }

            $scope.removeMedia = function (media) {
                $scope.dialogData.Media = _.without($scope.dialogData.Media, _.findWhere($scope.dialogData.Media, { Id: media.Id }));
                $scope.dialogData.Files = _.pluck($scope.dialogData.Media, 'Id').join(',');
            }           
        }
    );

    angular.module("umbraco").controller("Pipeline.Dialogs.ShareTask",
        function ($scope, $routeParams, pipelineResource, dialogService, localizationService) {
            
            $scope.permissions = [
                { id: 0, name: 'Just me' },
                { id: 1, name: 'Pipeline users' },
                { id: 2, name: 'Contact' },
                { id: 3, name: 'Everyone' }
            ];
        }
    );

    angular.module("umbraco").controller("Pipeline.Dialogs.AddPipeline",
        function ($scope, $routeParams, taskResource, pipelineResource, dialogService, labelResource, navigationService, userService) {

            pipelineResource.getUsers().then(function (response) {
                $scope.Users = response.data;                                
            });

            userService.getCurrentUser().then(function (response) {
                $scope.dialogData.UserId = response.id;
            });
            
        }
    );

/*  -------------------------
    Properties
    ------------------------- */

    angular.module("umbraco").controller("Pipeline.Properties.MediaPicker",
        function ($scope, $routeParams, pipelineResource, dialogService) {

            // file picker
            $scope.openMediaPicker = function () {

                dialogService.mediaPicker({
                    multiPicker: true,
                    callback: function (media) {

                        $scope.$parent.model.Media = $scope.$parent.model.Media || [];
                        media.forEach(function (file) {

                            var newMedia = {
                                Id: file.id,
                                Name: file.name,
                                Url: file.image
                            };

                            if (!_.where($scope.$parent.model.Media, { 'Id': newMedia.Id }).length) {
                                $scope.$parent.model.Media.push(newMedia);                                
                                $scope.$parent.model.Files = _.pluck($scope.$parent.model.Media, 'Id').join(',');
                            }
                        });
                    }
                });
            }

            $scope.removeMedia = function (media) {
                $scope.$parent.model.Media = _.without($scope.$parent.model.Media, _.findWhere($scope.$parent.model.Media, { Id: media.Id }));
                $scope.$parent.model.Files = _.pluck($scope.$parent.model.Media, 'Id').join(',');

            }

        });


String.prototype.splice = function (idx, rem, s) {
    return (this.slice(0, idx) + s + this.slice(idx + Math.abs(rem)));
};

String.prototype.IsDate = function () {
    return this && this.toString() != 'Invalid date' && this.toString().indexOf('0001-01-01') == -1;
};

String.prototype.numberify = function () {

    var input = this;

    //clearing left side zeros
    while (input.charAt(0) == '0') {
        input = input.substr(1);
    }

    input = input.replace(/[^\d.\',']/g, '');

    var point = input.indexOf(".");
    if (point >= 0) {
        input = input.slice(0, point + 3);
    }

    var decimalSplit = input.split(".");
    var intPart = decimalSplit[0];
    var decPart = decimalSplit[1];

    intPart = intPart.replace(/[^\d]/g, '');
    if (intPart.length > 3) {
        var intDiv = Math.floor(intPart.length / 3);
        while (intDiv > 0) {
            var lastComma = intPart.indexOf(",");
            if (lastComma < 0) {
                lastComma = intPart.length;
            }

            if (lastComma - 3 > 0) {
                intPart = intPart.splice(lastComma - 3, 0, ",");
            }
            intDiv--;
        }
    }
    if (decPart === undefined) {
        decPart = "";
    }
    else {
        decPart = "." + decPart;
    }
    return intPart + decPart;
};

angular.module('umbraco.directives')
    .directive('currencyInput', function () {
        return {
            restrict: 'A',
            scope: {
                field: '='
            },
            replace: true,
            template: '<input type="text" class="umb-editor umb-textstring" ng-model="field" ng-required />',
            link: function (scope, element, attrs) {
              
                scope.$watch(attrs.ngModel, function (v) {
                    if (scope.field)
                        scope.field = scope.field.toString().numberify();
                });

                $(element).bind('keyup', function (e) {
                    scope.$apply(function () { scope.field = scope.field.numberify(); });
                });

            }
        };
    })
    .directive('pipelineTimeline', function () {
        return {
            restrict: 'E',
            scope: {
                tasks: '=',
                parentType: '=',
                parent: '=',
                summary: '='
            },            
            templateUrl: '/App_Plugins/PipelineCRM/views/timeline.html'
        };
    })
    .directive('pipelineGrid', function () {
        return {
            restrict: 'E',
            scope: {
                type: '=',
                statusId: '=',
                viewId: '=',
                organisationId: '=',
                contactId: '='
            },
            templateUrl: function(tElement, tAttrs) {
                return '/App_Plugins/PipelineCRM/views/' + tAttrs.templateUrl;
            }
        };
    });

// drag drop

angular.module('umbraco.directives')
    .directive('pipelineBoardDrag', function() {
        return {
            link: function($scope, element, attrs) {

                var dragData = '';
                $scope.$watch(attrs.pipelineBoardDrag, function(newValue) {
                    dragData = newValue;
                });

                element.bind('dragstart', function(event) {
                    event.originalEvent.dataTransfer.setData("Text", dragData.Id);
                });
            }
        };
    })
    .directive('pipelineBoardDrop', function() {
        return {
            link: function($scope, element, attrs) {

                var dragOverClass = 'over';

                //  Prevent the default behavior. This has to be called in order for drob to work
                cancel = function(event) {
                    if (event.preventDefault) {
                        event.preventDefault();
                    }

                    if (event.stopPropigation) {
                        event.stopPropigation();
                    }
                    return false;
                };

                element.bind('dragover', function(event) {
                    cancel(event);
                    event.originalEvent.dataTransfer.dropEffect = 'move';
                    element.addClass(dragOverClass);
                });

                element.bind('drop', function(event) {
                    cancel(event);
                    element.removeClass(dragOverClass);
                    var droppedData = JSON.parse(event.originalEvent.dataTransfer.getData('Text'));
                    $scope.onDrop(droppedData, +element.attr('statusId'));
                });

                element.bind('dragleave', function(event) {
                    element.removeClass(dragOverClass);
                });
            }
        }
    })
    .directive('pipelineBoardCardDrop', function() {
        return {
            link: function($scope, element, attrs) {

                //  Prevent the default behavior. This has to be called in order for drob to work
                cancel = function (event) {
                    if (event.preventDefault) {
                        event.preventDefault();
                    }

                    if (event.stopPropigation) {
                        event.stopPropigation();
                    }
                    return false;
                };

                element.bind('dragover', function (event) {
                    cancel(event);
                    event.originalEvent.dataTransfer.dropEffect = 'move';
                    element.parent().addClass(+element.attr('dropPos') > 0 ? 'dropBefore' : 'dropAfter');
                });

                element.bind('drop', function (event) {
                    cancel(event);
                    element.parent().removeClass('dropBefore').removeClass('dropAfter');
                    var droppedData = JSON.parse(event.originalEvent.dataTransfer.getData('Text'));
                    $scope.onCardDrop(droppedData, +element.attr('pipelineId'), element.attr('dropPos'));
                });

                element.bind('dragleave', function (event) {
                    element.parent().removeClass('dropBefore').removeClass('dropAfter');
                });
            }
        }
    });

angular.module('umbraco.filters')
    .filter('listNames', function () {
        return function (input) {
            var list = '';
            input.forEach(function (item) {
                list += (list != '' ? ',' : '') + item.Name;
            });
            return list;
        };
    })
    .filter('toNumber', function () {
        return function (input) {            
            if (input) {
                return parseInt(input.toString().replace(',', ''));
            }
            return null;
        };
    })
    .filter('adjustedValue', function () {
        return function (pipeline) {
            if (pipeline && pipeline.Value) {
                var value = parseInt(pipeline.Value.toString().replace(',',''));

                // todo: remove this ugliness!
                if (pipeline.StatusId == 4) {
                    return 0;
                } else if (pipeline.StatusId == 3) {
                    return value;
                }

                if (pipeline.Probability)
                {
                    return value * pipeline.Probability / 100;
                }
                else
                {
                    return value;
                }                
            }
            return 0;
        };
    })
    .filter('sumPipelineValue', function () {
        return function(data, byProbability) {

            console.log(data);
            if (typeof (data) === 'undefined') {
                return 0;
            }
            var sum = 0;
            data.forEach(function(pipeline) {
                sum = sum + (byProbability ? (pipeline.Value * pipeline.Probability / 100) : pipeline.Value);
            });
            return sum;
        };
    });


angular.module("umbraco.resources")
    .factory("pipelineResource", function ($http) {
	    return {
	        getAll: function (getLinks) {
	            return $http.get("/umbraco/backoffice/PipelineCrm/PipelineApi/GetAll?getLinks=" + getLinks);
	        },
	        getPaged: function (pageNumber, sortColumn, sortOrder, searchTerm, statusId, contactId, organisationId) {
	            return $http.get('/umbraco/backoffice/PipelineCrm/PipelineApi/GetPaged?pageNumber=' + pageNumber + '&sortColumn=' + (sortColumn || '') + '&sortOrder=' + (sortOrder || '') + '&searchTerm=' + searchTerm + '&statusId=' + (statusId || 0) + '&contactId=' + (contactId || 0) + '&organisationId=' + (organisationId || 0));
	        },
	        getUsers: function (id) {
	            return $http.get("/umbraco/backoffice/PipelineCrm/PipelineApi/GetUsers");
	        },
	        getByStatusId: function (id) {
	            if (id >= 0) {
	                return $http.get("/umbraco/backoffice/PipelineCrm/PipelineApi/GetByStatusId?id=" + id + '&getLinks=true');
	            } else {
	                return $http.get("/umbraco/backoffice/PipelineCrm/PipelineApi/GetArchived");
	            }
	        },
	        getPipelineById: function (id) {
	            return $http.get("/umbraco/backoffice/PipelineCrm/PipelineApi/GetById?id=" + id);
	        },
	        getByContactId: function (id) {
	            return $http.get("/umbraco/backoffice/PipelineCrm/PipelineApi/GetByContactId?id=" + id);
	        },
	        getByOrganisationId: function (id) {
	            return $http.get("/umbraco/backoffice/PipelineCrm/PipelineApi/GetByOrganisationId?id=" + id);
	        },
                
	        savePipeline: function (pipeline) {
	            return $http.post("/umbraco/backoffice/PipelineCrm/PipelineApi/PostSave", angular.toJson(pipeline));
	        },
            savePipelines: function (pipelines) {
	            return $http.post("/umbraco/backoffice/PipelineCrm/PipelineApi/PostSavePipelines", angular.toJson(pipelines));
	        },
	        quickCreatePipeline: function (pipeline) {
	            return $http.post("/umbraco/backoffice/PipelineCrm/PipelineApi/QuickCreate", angular.toJson(pipeline));
	        },
	        deleteById: function (id, deleteLinks) {
	            return $http.delete("/umbraco/backoffice/PipelineCrm/PipelineApi/DeleteById?id=" + id + '&deleteLinks=' + deleteLinks);
	        },
	        deletePipelines: function (ids, deleteLinks) {
	            return $http.delete("/umbraco/backoffice/PipelineCrm/PipelineApi/DeletePipelinesById?ids=" + ids + '&deleteLinks=' + deleteLinks);
	        },
	        getStatuses: function () {
	            return $http.get("/umbraco/backoffice/PipelineCrm/StatusApi/GetAll");
	        },
            saveStatus: function(status) {
                return $http.post("/umbraco/backoffice/PipelineCrm/StatusApi/PostSave", angular.toJson(status));
            },
            deleteStatus: function (status) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/StatusApi/DeleteById?id=" + status);
            },

	        getPipelineTasks: function (id) {
	            return $http.get("/umbraco/backoffice/PipelineCrm/TaskApi/GetByPipeline/" + id);
	        },
            archivePipeline: function (pipeline) {
                return $http.post("/umbraco/backoffice/PipelineCrm/PipelineApi/Archive", angular.toJson(pipeline));
            },
            restorePipeline: function (pipeline) {
                return $http.post("/umbraco/backoffice/PipelineCrm/PipelineApi/Restore", angular.toJson(pipeline));
            },
            duplicatePipeline: function (pipeline) {
                return $http.post("/umbraco/backoffice/PipelineCrm/PipelineApi/Duplicate", angular.toJson(pipeline));
            },

            getPipelineValue: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/PipelineApi/GetCurrentValue");
            },
            getPendingAnswer: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/PipelineApi/GetPendingAnswer");
            },

            getStatusById: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/StatusApi/GetById?id=" + id);
            }
	    };
	})
    .factory("taskResource", function ($http) {
        return {
            getMyTasks: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/TaskApi/GetMyTasks");
            },
            saveTask: function (task) {
                return $http.post("/umbraco/backoffice/PipelineCrm/TaskApi/PostSave", angular.toJson(task));
            },
            deleteById: function (id) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/TaskApi/DeleteById?id=" + id);
            },
            toggleTask: function (id) {
                return $http.post("/umbraco/backoffice/PipelineCrm/TaskApi/PostToggle?id=" + id);
            },
            sendNote: function (id, recipient) {
                return $http.post("/umbraco/backoffice/PipelineCrm/TaskApi/SendNote?id=" + id + "&recipient=" + recipient);
            }
        }
    })
    .factory("organisationResource", function ($http) {
        return {
            getAll: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/OrganisationApi/GetAll");
            },
            getPaged: function (pageNumber, sortColumn, sortOrder, searchTerm, typeId) {
                return $http.get('/umbraco/backoffice/PipelineCrm/OrganisationApi/GetPaged?pageNumber=' + pageNumber + '&sortColumn=' + (sortColumn || '') + '&sortOrder=' + (sortOrder || '') + '&searchTerm=' + searchTerm + '&typeId=' + (typeId || ''));
            },
            getOrganisationsByName: function (name) {
                return $http.get('/umbraco/backoffice/PipelineCrm/OrganisationApi/GetOrganisationsByName?name=' + name);
            },
            getOrganisations: function (id) {
                if (id == -2) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/OrganisationApi/GetUnassigned");
                } else if (id == -1) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/OrganisationApi/GetArchived");
                } else if (id == 0) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/OrganisationApi/GetAll");
                } else if (id > 0) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/OrganisationApi/GetByTypeId?id=" + id);
                }
            },
            getArchived: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/OrganisationApi/GetArchived");
            },
            getOrganisationsByTypeId: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/OrganisationApi/GetByTypeId?id=" + id);
            },
            getUnassigned: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/OrganisationApi/GetUnassigned");
            },

            deleteOrganisationById: function (id, deleteLinks) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/OrganisationApi/DeleteById?id=" + id + '&deleteLinks=' + deleteLinks);
            },
            deleteOrganisations: function (ids, deleteLinks) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/OrganisationApi/DeleteOrgsById?ids=" + ids + '&deleteLinks=' + deleteLinks);
            },

            getOrganisationById: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/OrganisationApi/GetById?id=" + id);
            },
            archiveOrganisation: function (organisation) {
                return $http.post("/umbraco/backoffice/PipelineCrm/OrganisationApi/Archive", angular.toJson(organisation));
            },
            restoreOrganisation: function (organisation) {
                return $http.post("/umbraco/backoffice/PipelineCrm/OrganisationApi/Restore", angular.toJson(organisation));
            },
            saveOrganisation: function (organisation) {
                return $http.post("/umbraco/backoffice/PipelineCrm/OrganisationApi/PostSave", angular.toJson(organisation));
            },
            saveOrganisations: function (orgs) {
                return $http.post("/umbraco/backoffice/PipelineCrm/OrganisationApi/PostSaveOrganisations", angular.toJson(orgs));
            },

            getOrgTypes: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/OrgTypeApi/GetAll");
            },
            saveOrgType: function (organisation) {
                return $http.post("/umbraco/backoffice/PipelineCrm/OrgTypeApi/PostSave", angular.toJson(organisation));
            },
            deleteOrgType: function (organisation) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/OrgTypeApi/DeleteById?id=" + organisation);
            },
            getOrganisationTypeById: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/OrgTypeApi/GetById?id=" + id);
            }
        }
    })
    .factory("contactResource", function ($http) {
        return {
            getAll: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/ContactApi/GetAll");
            },
            getPaged: function (pageNumber, sortColumn, sortOrder, searchTerm, typeId) {
                return $http.get('/umbraco/backoffice/PipelineCrm/ContactApi/GetPaged?pageNumber=' + pageNumber + '&sortColumn=' + (sortColumn || '') + '&sortOrder=' + (sortOrder || '') + '&searchTerm=' + searchTerm + '&typeId=' + (typeId || ''));
            },
            getContactsByName: function (name) {
                return $http.get('/umbraco/backoffice/PipelineCrm/ContactApi/GetContactsByName?name=' + name);
            },
            getContacts: function (id) {
                if (id == -2){
                    return $http.get("/umbraco/backoffice/PipelineCrm/ContactApi/GetUnassigned");
                } else if (id == -1) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/ContactApi/GetArchived");
                } else if (id == 0) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/ContactApi/GetAll");
                } else if (id > 0) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/ContactApi/GetByContactTypeId?id=" + id);
                }
            },
            getContactById: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/ContactApi/GetById?id=" + id);
            },
            getByOrganisationId: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/ContactApi/GetByOrganisationId?id=" + id);
            },
            getCustomProps: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/custompropertyapi/GetCustomProps?type=Contact");
            },
            getUnassigned: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/ContactApi/GetUnassigned");
            },
            saveContact: function (contact) {
                return $http.post("/umbraco/backoffice/PipelineCrm/ContactApi/PostSave", angular.toJson(contact));
            },
            saveContacts: function (contacts) {
                return $http.post("/umbraco/backoffice/PipelineCrm/ContactApi/PostSaveContacts", angular.toJson(contacts));
            },
            archiveContact: function (contact) {
                return $http.post("/umbraco/backoffice/PipelineCrm/ContactApi/Archive", angular.toJson(contact));
            },
            restoreContact: function (contact) {
                return $http.post("/umbraco/backoffice/PipelineCrm/ContactApi/Restore", angular.toJson(contact));
            },
            deleteById: function (id, deleteLinks) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/ContactApi/DeleteById?id=" + id + '&deleteLinks=' + deleteLinks);
            },
            deleteContacts: function (ids, deleteLinks) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/ContactApi/DeleteContactsById?ids=" + ids + '&deleteLinks=' + deleteLinks);
            },

            getContactTypes: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/ContactTypeApi/GetAll");
            },
            saveContactType: function (type) {
                return $http.post("/umbraco/backoffice/PipelineCrm/ContactTypeApi/PostSave", angular.toJson(type));
            },
            deleteContactType: function (type) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/ContactTypeApi/DeleteById?id=" + type);
            },
            getContactTypeById: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/ContactTypeApi/GetById?id=" + id);
            }
        }
    })
    .factory("labelResource", function ($http) {
        return {
            getAll: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/LabelApi/GetAll");
            }
        }
    })
    .factory("propResource", function ($http) {
        return {
            getPropsByType: function (type) {
                return $http.get("/umbraco/backoffice/PipelineCrm/custompropertyapi/GetCustomProps?type=" + type);
            },
            getPropsByDocType: function (alias) {
                return $http.get("/umbraco/backoffice/PipelineCrm/custompropertyapi/GetCustomProps?docTypeAlias=" + alias);
            },
            getSegmentProps: function (name) {
                return $http.get("/umbraco/backoffice/PipelineCrm/custompropertyapi/GetCriteriaProps?criteriaName=" + name);
            }
        }
    })    
    .factory("prefResource", function ($http) {
        return {
            getConfig: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/PreferencesApi/GetConfig");
            },
            getUserPreferences: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/PreferencesApi/GetUserPreferences");
            },
            savePreferences: function (preferences) {
                return $http.post("/umbraco/backoffice/PipelineCrm/PreferencesApi/PostSave", angular.toJson(preferences));
            }
        }
    })
    .factory("segmentResource", function ($http) {
        return {
            getAll: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetAll");
            },
            getPaged: function (pageNumber, sortColumn, sortOrder, searchTerm, typeId) {
                return $http.get('/umbraco/backoffice/PipelineCrm/SegmentApi/GetPaged?pageNumber=' + pageNumber + '&sortColumn=' + (sortColumn || '') + '&sortOrder=' + (sortOrder || '') + '&searchTerm=' + searchTerm + '&typeId=' + (typeId || ''));
            },
            getSegmentsByName: function (name) {
                return $http.get('/umbraco/backoffice/PipelineCrm/SegmentApi/GetSegmentsByName?name=' + name);
            },
            getSegments: function (id) {
                if (id == -2) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetUnassigned");
                } else if (id == -1) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetArchived");
                } else if (id == 0) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetAll");
                } else if (id > 0) {
                    return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetByTypeId?id=" + id);
                }
            },
            getArchived: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetArchived");
            },
            getSegmentsByTypeId: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetByTypeId?id=" + id);
            },
            getUnassigned: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetUnassigned");
            },
            getSegmentCriteria: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetCriteria");
            },
            getSegmentPreview: function (id, criteria) {
                return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetContacts?id=" + id + '&criteria=' + criteria);
            },

            deleteSegmentById: function (id, deleteLinks) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/SegmentApi/DeleteById?id=" + id);
            },
            deleteSegments: function (ids, deleteLinks) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/SegmentApi/DeleteSegmentsById?ids=" + ids);
            },

            getSegmentById: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/SegmentApi/GetById?id=" + id);
            },
            archiveSegment: function (Segment) {
                return $http.post("/umbraco/backoffice/PipelineCrm/SegmentApi/Archive", angular.toJson(Segment));
            },
            restoreSegment: function (Segment) {
                return $http.post("/umbraco/backoffice/PipelineCrm/SegmentApi/Restore", angular.toJson(Segment));
            },
            saveSegment: function (Segment) {
                return $http.post("/umbraco/backoffice/PipelineCrm/SegmentApi/PostSave", angular.toJson(Segment));
            },
            saveSegments: function (segments) {
                return $http.post("/umbraco/backoffice/PipelineCrm/SegmentApi/PostSaveSegments", angular.toJson(segments));
            },
                       
            getSegmentTypes: function () {
                return $http.get("/umbraco/backoffice/PipelineCrm/SegmentTypeApi/GetAll");
            },
            saveSegmentType: function (Segment) {
                return $http.post("/umbraco/backoffice/PipelineCrm/SegmentTypeApi/PostSave", angular.toJson(Segment));
            },
            deleteSegmentType: function (Segment) {
                return $http.delete("/umbraco/backoffice/PipelineCrm/SegmentTypeApi/DeleteById?id=" + Segment);
            },
            getSegmentTypeById: function (id) {
                return $http.get("/umbraco/backoffice/PipelineCrm/SegmentTypeApi/GetById?id=" + id);
            }
        }
    })
;