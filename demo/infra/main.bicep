targetScope = 'resourceGroup'

import * as n from 'names.bicep'

param location string = resourceGroup().location

var spaceName = 'demo'
var workload = 'secmgt'

// Log Workspace
module log 'br/public:avm/res/operational-insights/workspace:0.11.2' = {
  name: '${deployment().name}-log'
  params: {
    name: n.nameLogWorkspace(location, spaceName, 1)
    location: location
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Subnets: https://visualsubnetcalc.com/index.html?c=1N4IgbiBcIEwgNCARlEBGADAOm7g9DAKwIgC2qAggFoCqASgKIkDOUoGADmyBgI7cYATtwB2lDhwAEAZQCmgsAEsAxrJABfRGmGRQY6AAVBisAEMALrMkAZRSIDWGzen67YOvagBqAWQ2IAZg8QfRAaEQBXZlkAEyd1BKA
module vnet 'br/public:avm/res/network/virtual-network:0.7.0' = {
  name: '${deployment().name}-vnet1'
  params: {
    name: n.nameNetworkVnet(location, spaceName, 1)
    location: location
    addressPrefixes: [
      '10.0.0.0/25'
    ]
    subnets: [
      {
        name: n.nameNetworkSubnet(location, 'app', 1)
        addressPrefix: '10.0.0.0/27'
        delegation: n.networkDelegations.Microsoft_Web_serverFarms
      }
      {
        name: n.nameNetworkSubnet(location, 'plnk', 1)
        addressPrefix: '10.0.0.32/27'
      }
      {
        name: n.nameNetworkSubnet(location, 'vm', 1)
        addressPrefix: '10.0.0.64/27'
      }
      {
        name: 'AzureBastionSubnet'
        addressPrefix: '10.0.0.96/27'
      }
    ]
    diagnosticSettings: [
      {
        workspaceResourceId: log.outputs.resourceId
      }
    ]
  }
}

// Key Vault
module keyVault 'br/public:avm/res/key-vault/vault:0.13.0' = {
  name: '${deployment().name}-kv1'
  params: {
    name: n.nameKeyVault(location, spaceName, workload, 1)
    location: location
    privateEndpoints: [
      {
        subnetResourceId: vnet.outputs.subnetResourceIds[1]
      }
    ]
  }
}

// Storage Account
module storage 'br/public:avm/res/storage/storage-account:0.20.0' = {
  name: '${deployment().name}-stg1'
  params: {
    name: n.nameStorageAccount(location, workload, spaceName, 1)
    location: location
    kind: 'StorageV2'
    skuName: 'Standard_LRS'
    privateEndpoints: [
      {
        service: 'blob'
        subnetResourceId: vnet.outputs.subnetResourceIds[1]
      }
    ]
  }
}

// PostgreSQL
module pgsql 'br/public:avm/res/db-for-postgre-sql/flexible-server:0.12.0' = {
  name: '${deployment().name}-pgsql1'
  params: {
    name: n.namePostgreSqlServer(location, spaceName, workload, 1)
    location: location
    availabilityZone: 1
    highAvailabilityZone: 1
    highAvailability: 'SameZone'
    skuName: 'Standard_D2ds_v5'
    tier: 'GeneralPurpose'
    administratorLogin: 'admin20250612'
    administratorLoginPassword: 'InitialP@$$wordChange'
    storageSizeGB: 32
    version: '17'
    privateEndpoints: [
      {
        subnetResourceId: vnet.outputs.subnetResourceIds[1]
      }
    ]
    diagnosticSettings: [
      {
        workspaceResourceId: log.outputs.resourceId
      }
    ]
  }
}

// MariaDB
// module maria 'br/public:avm/res/db-for-my-sql/flexible-server:0.8.0' = {
//   name: '${deployment().name}-mysql1'
//   params: {
//     name: n.nameMySqlServer(location, spaceName, workload, 1)
//     location: location
//     availabilityZone: 1
//     highAvailabilityZone: 1
//     highAvailability: 'SameZone'
//     skuName: 'Standard_D2ds_v5'
//     tier: 'GeneralPurpose'
//     administratorLogin: 'admin20250612'
//     administratorLoginPassword: 'InitialP@$$wordChange'
//     storageSizeGB: 32
//     storageAutoGrow: 'Enabled'
//     version: '8.0.21'
//     privateEndpoints: [
//       {
//         subnetResourceId: vnet.outputs.subnetResourceIds[1]
//       }
//     ]
//     diagnosticSettings: [
//       {
//         workspaceResourceId: log.outputs.resourceId
//       }
//     ]
//   }
// }

// Virtual Machine (Jump)
module vm 'br/public:avm/res/compute/virtual-machine:0.15.0' = {
  name: '${deployment().name}-vm1'
  params: {
    name: n.nameVirtualMachine(location, spaceName, 'jump', 1)
    location: location
    adminUsername: 'garoyeri'
    adminPassword: 'InitialP@$$wordChange'
    disablePasswordAuthentication: false
    imageReference: {
      publisher: 'canonical'
      offer: 'ubuntu-24_04-lts'
      sku: 'server'
      version: 'latest'
    }
    nicConfigurations: [
      {
        ipConfigurations: [
          {
            subnetResourceId: vnet.outputs.subnetResourceIds[2]
          }
        ]
      }
    ]
    osDisk: {
      createOption: 'FromImage'
      managedDisk: {
        storageAccountType: 'Premium_LRS'
      }
    }
    osType: 'Linux'
    vmSize: 'Standard_D2ads_v6'
    zone: 1
    bootDiagnostics: true
    securityType: 'TrustedLaunch'
    secureBootEnabled: true
    vTpmEnabled: true
    encryptionAtHost: false
  }
}

module bastion 'br/public:avm/res/network/bastion-host:0.6.1' = {
  name: '${deployment().name}-bastion1'
  params: {
    name: 'bastion1'
    location: location
    virtualNetworkResourceId: vnet.outputs.resourceId
    diagnosticSettings: [
      {
        workspaceResourceId: log.outputs.resourceId
      }
    ]
  }
}
