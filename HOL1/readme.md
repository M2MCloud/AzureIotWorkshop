# IoT Hub - Hands On Lab

## Contents

1. [Objectives and Requirements](#objectives-and-requirements)
    1. [Objectives](#objectives)
    1. [Requirements](#requirements)
1. [Instructions](#instructions)
   1. [Prerequisites](#prerequisites)
   1. [Create a Storage Account](#create-a-general-purpose-storage-account)
   1. [Create a Stream Analytics Job](#create-an-azure-stream-analytics-job)
   1. [Create The Job Input](#create-the-job-input)
   1. [Add The Archive Output](#add-the-archive-output)
   1. [Add A Query](#add-a-query)
   1. [Create a CosmosDB Account](#create-a-cosmosdb-account)
   1. [Add The Aggregated Data Output](#add-the-aggregated-data-output)
   1. [Amend The Query to Include Aggregated Data](#amend-the-query-for-aggregated-data-output)

## Objectives and Requirements

### Objectives
1. Create Azure Resource Group
1. Create an [Azure IoT Hub](https://azure.microsoft.com/en-gb/services/iot-hub/) 
1. Use [Azure IoT Hub](https://azure.microsoft.com/en-gb/services/iot-hub/) to manage a simulated IoT Asset
1. Send a Device Message(s) to the IoT Hub
1. Send a massive Device Message to the IoT Hub
1. Read and Write Device Twin Properties
1. Receive a message from the IoT Hub
    1. Cloud to Device Message 
    1. Direct Method Invocation
   

### Requirements

1. Microsoft Visual Studio Community, Professional or Enterprise 2017 (Windows 10)
1. Access to an Azure Subscription with Administrator permissions

---

## Instructions

### Prerequisites

Activities in this lab take place within the Azure Portal and Visual Studio. 

### Create a Resource Group
1. Navigate to the [Azure Portal](https://portal.azure.com)
1. Select "Resource Groups" from the menu on the left

    ![create resource group](content/addnewresourcegroup.png)
1. Click the Add button
   
   ![configure resource group](content/resourcegroupname.png)
1. Name the Resource Group `censis-workshop`

1. Click the Create button to Create your new Resource Group

    ![go to resource group](content/GoToResourceGroup.png)
1. Navigate to your newly created Resoure Group. You can also navigate to the your newly created Resource Group by clicking on "Resource Groups" in the main navigation menu on the left. 

### Create an Azure IoT Hub
1. Navigate to your Resource Group if you have not done so already

![add new resource to  resource group](content/AddResourceToResourceGroup.png)
1. Click the Add button to add a new resource to your Resource Group

![select iot hub resource](content/TypeThenSelectIoTHub.png)
1. In the search box type `iot hub` and select the IoT Hub resource that appears in the results list

![configure iot hub](content/IoTHubNameThenCreate.png)
1. Give you iot hub a unique name, e.g. `censis-workshop-somethingunique`
1. Click "Pricing and scale tier and select the "Free" pricing tier
1. Wait!  ^ Make sure you have selected "Pricing and scale tier" is on the "Free" tier ^ : ) 
1. In "Resource Group" use the newly created, existing `censis-workshop` resource group
1. In "Location" select "UK West"
1. Click the "Create" button to create a new IoT Hub
1. Navigate to the the newly created IoT Hub either by clicking on the notification that appears (or by using the "Search resources, services and docs" text box at the very top of the page)

### Use Azure IoT Hub to manage a simulated IoT Asset

We need to register a Device with the newly created IoT Hub so the hub can authorise the connect and send requests from the Device. 
1. Navigate to the newly create IoT Hub ("Resource Groups -> "censis-workshop" -> "name of your iot hub")

![go to the device explorer](content/DeviceExplorerCreateNewDevice.png)
1. Click on "Device Explorer on the left hand side menu
1. Click on "Add Device" 

![add a new device](content/DeviceExplorerAddDevice.png)
1. Give the Device a unique identifier that will allow the IoT Hub to identifiy this Device
1. Click on "Save" to create the Device

![select the new device](content/DeviceExplorerAddDevice.png)
1. Select the newly created Device by clicking on the Device in the Device Explorer list and view it's Device Details. 

![associate storage account container with the iot hub](content/DeviceExplorerFileUpload.png)
1. Associate a storage account container with IoT Hub by clicking on "File Upload" on the left hand side menu

![create storage account container with the iot hub](content/DeviceExplorerFileUploadContainer.png)
1. Select the previously created Storage Account, "censisworkshop" and create a new container within that Storage Account named "fileupload".

![save storage account container against the iot hub](content/DeviceExplorerFileUploadSave.png)
1. Select the newly created container and click "Save"

1. We're now ready to start messing around with the IoT Hub. 

### Send a Device Message(s) to the IoT Hub

1. 






### Create a General Purpose Storage Account

We need to create a storage account which will be used to store blob and table data.

1. In the `censis-workshop` Resource Group blade, select add in the top left

   ![add](content/add.png)
1. In the search box type "Storage Account" and select `Storage Account - blob, file, table, queue`

   ![search](content/storsearch.png)
1. Select `Storage Account - blob, file, table, queue` from the results and click `Create`

   ![createstor](content/storcreate.png)
1. In the `Create Storage Account` Blade, give your storage account a name. The name must be globally unique, 24 characters or fewer and contain only alpha-numeric characters. Configure the storage account settings as follows and click `Create`:

   ![newstor](content/newstor.png)

We won't be using the storage account until later. When the deployment succeeds, you don't need to go to the resource immediately. You can move straight on to the next section



