@export()
@description('''
  List the short names for the Azure locations (regions). The return value is an
  object where the keys are all the name of Azure locations and the values are
  the short names. For example: 'East US' => 'EUS'
''')
func listLocationShortNames() object => {
  eastus: 'EUS'
  southcentralus: 'SCUS'
  westus2: 'WUS2'
  westus3: 'WUS3'
  australiaeast: 'AE'
  southeastasia: 'SA'
  northeurope: 'NE'
  swedencentral: 'SC'
  uksouth: 'UKS'
  westeurope: 'WE'
  centralus: 'CUS'
  southafricanorth: 'SAN'
  centralindia: 'CI'
  eastasia: 'EA'
  japaneast: 'JE'
  koreacentral: 'KC'
  newzealandnorth: 'NZN'
  canadacentral: 'CC'
  francecentral: 'FC'
  germanywestcentral: 'GWC'
  italynorth: 'IN'
  norwayeast: 'NE'
  polandcentral: 'PC'
  spaincentral: 'SC'
  switzerlandnorth: 'SN'
  mexicocentral: 'MC'
  uaenorth: 'UAEN'
  brazilsouth: 'BS'
  israelcentral: 'IC'
  qatarcentral: 'QC'
  centralusstage: 'CUSS'
  eastusstage: 'EUSS'
  eastus2stage: 'EUS2S'
  northcentralusstage: 'NCUSS'
  southcentralusstage: 'SCUSS'
  westusstage: 'WUSS'
  westus2stage: 'WUS2S'
  asia: 'A'
  asiapacific: 'AP'
  australia: 'A'
  brazil: 'B'
  canada: 'C'
  europe: 'E'
  france: 'F'
  germany: 'G'
  global: 'G'
  india: 'I'
  israel: 'I'
  italy: 'I'
  japan: 'J'
  korea: 'K'
  newzealand: 'NZ'
  norway: 'N'
  poland: 'P'
  qatar: 'Q'
  singapore: 'S'
  southafrica: 'SA'
  sweden: 'S'
  switzerland: 'S'
  uae: 'UAE'
  uk: 'UK'
  unitedstates: 'US'
  unitedstateseuap: 'USEUAP'
  eastasiastage: 'EAS'
  southeastasiastage: 'SAS'
  brazilus: 'BUS'
  eastus2: 'EUS2'
  eastusstg: 'EUSSTG'
  northcentralus: 'NCUS'
  westus: 'WUS'
  japanwest: 'JW'
  jioindiawest: 'JIW'
  centraluseuap: 'CUSEUAP'
  eastus2euap: 'EUS2EUAP'
  southcentralusstg: 'SCUSSTG'
  westcentralus: 'WCUS'
  southafricawest: 'SAW'
  australiacentral: 'AC'
  australiacentral2: 'AC2'
  australiasoutheast: 'AS'
  jioindiacentral: 'JIC'
  koreasouth: 'KS'
  southindia: 'SI'
  westindia: 'WI'
  canadaeast: 'CE'
  francesouth: 'FS'
  germanynorth: 'GN'
  norwaywest: 'NW'
  switzerlandwest: 'SW'
  ukwest: 'UKW'
  uaecentral: 'UAEC'
  brazilsoutheast: 'BS'
}

@export()
@minValue(0)
@maxValue(999)
type Index = int

@export()
@description('Format a numeric index into a three digit, zero-filled string.')
func formatIndex(index Index) string => padLeft(index, 3, '0')

@export()
@description('Derive a short name for an Azure location (region). For example: \'East US\' => \'EUS\'')
func nameLocation(location string) string => listLocationShortNames()[location]

@export()
@description('List the available network delegations.')
var networkDelegations = {
  Dell_Storage_fileSystems: 'Dell.Storage/fileSystems'
  GitHub_Network_networkSettings: 'GitHub.Network/networkSettings'
  Informatica_DataManagement_organizations: 'Informatica.DataManagement/organizations'
  Microsoft_ApiManagement_service: 'Microsoft.ApiManagement/service'
  Microsoft_Apollo_npu: 'Microsoft.Apollo/npu'
  Microsoft_App_environments: 'Microsoft.App/environments'
  Microsoft_AVS_PrivateClouds: 'Microsoft.AVS/PrivateClouds'
  Microsoft_AzureCommunicationsGateway_networkSettings: 'Microsoft.AzureCommunicationsGateway/networkSettings'
  Microsoft_AzureCosmosDB_clusters: 'Microsoft.AzureCosmosDB/clusters'
  Microsoft_BareMetal_AzureHostedService: 'Microsoft.BareMetal/AzureHostedService'
  Microsoft_BareMetal_AzureVMware: 'Microsoft.BareMetal/AzureVMware'
  Microsoft_BareMetal_CrayServers: 'Microsoft.BareMetal/CrayServers'
  Microsoft_Batch_batchAccounts: 'Microsoft.Batch/batchAccounts'
  Microsoft_CloudTest_hostedpools: 'Microsoft.CloudTest/hostedpools'
  Microsoft_CloudTest_images: 'Microsoft.CloudTest/images'
  Microsoft_CloudTest_pools: 'Microsoft.CloudTest/pools'
  Microsoft_ContainerInstance_containerGroups: 'Microsoft.ContainerInstance/containerGroups'
  Microsoft_ContainerService_managedClusters: 'Microsoft.ContainerService/managedClusters'
  Microsoft_Databricks_workspaces: 'Microsoft.Databricks/workspaces'
  Microsoft_DBforMySQL_flexibleServers: 'Microsoft.DBforMySQL/flexibleServers'
  Microsoft_DBforMySQL_servers: 'Microsoft.DBforMySQL/servers'
  Microsoft_DBforMySQL_serversv2: 'Microsoft.DBforMySQL/serversv2'
  Microsoft_DBforPostgreSQL_flexibleServers: 'Microsoft.DBforPostgreSQL/flexibleServers'
  Microsoft_DBforPostgreSQL_serversv2: 'Microsoft.DBforPostgreSQL/serversv2'
  Microsoft_DBforPostgreSQL_singleServers: 'Microsoft.DBforPostgreSQL/singleServers'
  Microsoft_DelegatedNetwork_controller: 'Microsoft.DelegatedNetwork/controller'
  Microsoft_DevCenter_networkConnection: 'Microsoft.DevCenter/networkConnection'
  Microsoft_DevOpsInfrastructure_pools: 'Microsoft.DevOpsInfrastructure/pools'
  Microsoft_DocumentDB_cassandraClusters: 'Microsoft.DocumentDB/cassandraClusters'
  Microsoft_Fidalgo_networkSettings: 'Microsoft.Fidalgo/networkSettings'
  Microsoft_HardwareSecurityModules_dedicatedHSMs: 'Microsoft.HardwareSecurityModules/dedicatedHSMs'
  Microsoft_Kusto_clusters: 'Microsoft.Kusto/clusters'
  Microsoft_LabServices_labplans: 'Microsoft.LabServices/labplans'
  Microsoft_Logic_integrationServiceEnvironments: 'Microsoft.Logic/integrationServiceEnvironments'
  Microsoft_MachineLearningServices_workspaceComputes: 'Microsoft.MachineLearningServices/workspaceComputes'
  Microsoft_MachineLearningServices_workspaces: 'Microsoft.MachineLearningServices/workspaces'
  Microsoft_Netapp_scaleVolumes: 'Microsoft.Netapp/scaleVolumes'
  Microsoft_Netapp_volumes: 'Microsoft.Netapp/volumes'
  Microsoft_Network_dnsResolvers: 'Microsoft.Network/dnsResolvers'
  Microsoft_Network_networkWatchers: 'Microsoft.Network/networkWatchers'
  Microsoft_Orbital_orbitalGateways: 'Microsoft.Orbital/orbitalGateways'
  Microsoft_PowerAutomate_hostedRpa: 'Microsoft.PowerAutomate/hostedRpa'
  Microsoft_PowerPlatform_enterprisePolicies: 'Microsoft.PowerPlatform/enterprisePolicies'
  Microsoft_PowerPlatform_vnetaccesslinks: 'Microsoft.PowerPlatform/vnetaccesslinks'
  Microsoft_ServiceFabricMesh_networks: 'Microsoft.ServiceFabricMesh/networks'
  Microsoft_ServiceNetworking_trafficControllers: 'Microsoft.ServiceNetworking/trafficControllers'
  Microsoft_Singularity_accounts_networks: 'Microsoft.Singularity/accounts/networks'
  Microsoft_Singularity_accounts_npu: 'Microsoft.Singularity/accounts/npu'
  Microsoft_Sql_managedInstances: 'Microsoft.Sql/managedInstances'
  Microsoft_StoragePool_diskPools: 'Microsoft.StoragePool/diskPools'
  Microsoft_StreamAnalytics_streamingJobs: 'Microsoft.StreamAnalytics/streamingJobs'
  Microsoft_Synapse_workspaces: 'Microsoft.Synapse/workspaces'
  Microsoft_Web_hostingEnvironments: 'Microsoft.Web/hostingEnvironments'
  Microsoft_Web_serverFarms: 'Microsoft.Web/serverFarms'
  NGINX_NGINXPLUS_nginxDeployments: 'NGINX.NGINXPLUS/nginxDeployments'
  Oracle_Database_networkAttachments: 'Oracle.Database/networkAttachments'
  PaloAltoNetworks_Cloudngfw_firewalls: 'PaloAltoNetworks.Cloudngfw/firewalls'
  PureStorage_Block_storagePools: 'PureStorage.Block/storagePools'
  Qumulo_Storage_fileSystems: 'Qumulo.Storage/fileSystems'
}

@export()
@description('Derive a name for a Virtual Network. For example: vnet-<subscription purpose>-<region>-<000>')
func nameNetworkVnet(location string, purpose string, index Index) string =>
  'vnet-${toLower(purpose)}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a Subnet. For example: snet-<subscription purpose>-<region>-<000>')
func nameNetworkSubnet(location string, purpose string, index Index) string =>
  'snet-${toLower(purpose)}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a Network Security Group. For example: nsg-<policy name>-<000>')
func nameNetworkSecurityGroup(policyName string, index Index) string =>
  'nsg-${replace(policyName, ' ', '_')}-${formatIndex(index)}'


@export()
@description('Derive a name for an App Service Plan. For example: asp-<workload>-<space>-<region>-<000>')
func nameWebServerFarm(location string, spaceName string, workload string, index Index) string =>
  'asp-${workload}-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for an Web Application. For example: app-<workload>-<space>-<region>-<000>')
func nameWebApplication(location string, spaceName string, workload string, index Index) string =>
  'app-${workload}-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a Function Application. For example: fun-<workload>-<space>-<region>-<000>')
func nameFunctionApplication(location string, spaceName string, workload string, index Index) string =>
  'fun-${workload}-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a log analytics workspace')
func nameLogWorkspace(location string, spaceName string, index Index) string =>
  'log-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a SQL Server')
func nameSqlServer(location string, spaceName string, workload string, index Index) string =>
  'sqlsvr-${workload}-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a SQL Server Elastic Pool')
func nameSqlElasticPool(location string, spaceName string, workload string, index Index) string =>
  'sqlep-${workload}-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a PGSQL Server')
func namePostgreSqlServer(location string, spaceName string, workload string, index Index) string =>
  'pgsql-${workload}-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a MySQL Server')
func nameMySqlServer(location string, spaceName string, workload string, index Index) string =>
  'mysql-${workload}-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a Azure Monitor Private Link Scope. Example: pls-<spaceName>-<region>-<000>')
func nameInsightsPrivateLinkScope(location string, spaceName string, index Index) string =>
  'pls-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'

@export()
@description('Derive a name for a Storage Account. Example: st<purpose><region><000>')
func nameStorageAccount(location string, spaceName string, workload string, index Index) string =>
  'st${toLower(workload)}${toLower(spaceName)}${toLower(nameLocation(location))}${formatIndex(index)}'

@export()
@description('Derive a name for a Key Vault.')
func nameKeyVault(location string, spaceName string, workload string, index Index) string =>
  'kv-${workload}-${spaceName}-${toLower(nameLocation(location))}-${formatIndex(index)}'
