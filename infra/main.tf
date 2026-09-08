terraform {
  required_version = ">= 1.6.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    # azapi exposes the Azure SQL free-offer ARM properties (useFreeLimit /
    # freeLimitExhaustionBehavior) that the azurerm provider does not surface.
    azapi = {
      source  = "azure/azapi"
      version = "~> 2.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }
}

provider "azurerm" {
  features {}

  # A fresh subscription can't register every resource provider (Microsoft.AVS
  # in particular hangs), and this config only needs Microsoft.Sql + Microsoft.Web.
  # Disable the blanket auto-registration; register those two once via the CLI
  # (see the README/steps) — they're already registered on most subscriptions.
  resource_provider_registrations = "none"
}

provider "azapi" {}

# Random suffix so the globally-unique names (SQL server, web app) don't collide.
resource "random_string" "suffix" {
  length  = 6
  lower   = true
  upper   = false
  numeric = true
  special = false
}

locals {
  sql_server_name = "sql-${var.project}-${random_string.suffix.result}"
  web_app_name    = "app-${var.project}-${random_string.suffix.result}"
  database_name   = "sqldb-${var.project}"

  # ADO.NET connection string for the free serverless database.
  # Encrypt=True + no TrustServerCertificate: Azure SQL presents a valid cert.
  connection_string = join("", [
    "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;",
    "Database=${local.database_name};",
    "User ID=${var.sql_admin_login};",
    "Password=${var.sql_admin_password};",
    "Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  ])
}

resource "azurerm_resource_group" "main" {
  name     = "rg-${var.project}"
  location = var.location
}

# ---- Azure SQL ----

resource "azurerm_mssql_server" "main" {
  name                         = local.sql_server_name
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password
}

# Created via azapi so we can set the free-offer properties. Serverless General
# Purpose, auto-pausing; the lifetime free offer gives 100k vCore-seconds + 32 GB
# per month, and AutoPause on exhaustion guarantees no charges.
resource "azapi_resource" "database" {
  type      = "Microsoft.Sql/servers/databases@2023-08-01-preview"
  name      = local.database_name
  parent_id = azurerm_mssql_server.main.id
  location  = azurerm_resource_group.main.location

  body = {
    sku = {
      name = "GP_S_Gen5_2"
    }
    properties = {
      collation                   = "SQL_Latin1_General_CP1_CI_AS"
      maxSizeBytes                = 34359738368 # 32 GB
      autoPauseDelay              = 60
      minCapacity                 = 0.5
      zoneRedundant               = false
      useFreeLimit                = true
      freeLimitExhaustionBehavior = "AutoPause"
    }
  }
}

# Special 0.0.0.0 rule = "Allow Azure services and resources to access this server"
# so the App Service can reach the database.
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Optional: your own public IP, so you can apply EF migrations from your machine.
# Set client_ip in terraform.tfvars; leave empty to skip.
resource "azurerm_mssql_firewall_rule" "client" {
  count            = var.client_ip == "" ? 0 : 1
  name             = "ClientMachine"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = var.client_ip
  end_ip_address   = var.client_ip
}

# ---- App Service ----

resource "azurerm_service_plan" "main" {
  name                = "plan-${var.project}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "F1" # Free tier
}

resource "azurerm_linux_web_app" "main" {
  name                = local.web_app_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_service_plan.main.location
  service_plan_id     = azurerm_service_plan.main.id
  https_only          = true

  site_config {
    always_on = false # F1 does not support Always On

    application_stack {
      dotnet_version = "10.0"
    }
  }

  # ASP.NET Core maps a "DefaultConnection" connection string to
  # ConnectionStrings:DefaultConnection, and "JwtSettings__Secret" to
  # JwtSettings:Secret — so the app reads config from the environment, not appsettings.
  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = local.connection_string
  }

  app_settings = {
    "JwtSettings__Secret"    = var.jwt_secret
    "ASPNETCORE_ENVIRONMENT" = var.aspnetcore_environment
  }
}
