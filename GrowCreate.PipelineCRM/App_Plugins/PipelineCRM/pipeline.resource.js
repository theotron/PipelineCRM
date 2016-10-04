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
            getPropsByDocType: function (name) {
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