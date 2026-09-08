output "web_app_name" {
  description = "Name of the App Service — use this as the app-name in the GitHub Actions deploy."
  value       = azurerm_linux_web_app.main.name
}

output "web_app_url" {
  description = "Public URL of the deployed API."
  value       = "https://${azurerm_linux_web_app.main.default_hostname}"
}

output "sql_server_fqdn" {
  description = "Fully-qualified name of the Azure SQL server."
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "database_name" {
  description = "Name of the Azure SQL database."
  value       = azapi_resource.database.name
}

output "connection_string" {
  description = "ADO.NET connection string — use it to apply EF migrations (needs client_ip set in the SQL firewall)."
  value       = local.connection_string
  sensitive   = true
}
